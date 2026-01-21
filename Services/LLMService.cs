using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using ComputerController.Models;

namespace ComputerController.Services;

public class LLMService
{
    private readonly HttpClient _httpClient;
    private readonly SettingsService _settingsService;
    private readonly List<ConversationEntry> _conversationHistory = new();
    private const int MaxHistoryEntries = 10;

    public LLMService(SettingsService settingsService)
    {
        _settingsService = settingsService;
        _httpClient = new HttpClient
        {
            Timeout = TimeSpan.FromSeconds(120)
        };
    }

    public IReadOnlyList<ConversationEntry> ConversationHistory => _conversationHistory.AsReadOnly();

    public bool IsConnected(LLMProvider provider)
    {
        if (!provider.RequiresApiKey())
        {
            return true; // Ollama doesn't need API key
        }

        var apiKey = _settingsService.GetApiKey(provider);
        return !string.IsNullOrWhiteSpace(apiKey);
    }

    public async Task<GeneratedCode> GenerateCodeAsync(string userRequest, LLMProvider provider, string? selectedModel = null, CancellationToken cancellationToken = default)
    {
        if (provider.RequiresApiKey())
        {
            var apiKey = _settingsService.GetApiKey(provider);
            if (string.IsNullOrWhiteSpace(apiKey))
            {
                throw new InvalidOperationException($"No API key provided for {provider.GetDisplayName()}. Please set your API key in settings.");
            }
        }

        var prompt = CreatePrompt(userRequest, provider);
        var request = CreateRequest(prompt, provider, selectedModel);

        try
        {
            var response = await _httpClient.SendAsync(request, cancellationToken);
            
            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync(cancellationToken);
                throw new HttpRequestException($"HTTP error {response.StatusCode}: {errorContent}");
            }

            var responseContent = await response.Content.ReadAsStringAsync(cancellationToken);
            var result = ParseResponse(responseContent, provider);
            
            return result;
        }
        catch (TaskCanceledException)
        {
            throw new TimeoutException("Request timed out. Please try again.");
        }
    }

    public void AddToHistory(string userRequest, string generatedCode, string? executionResult)
    {
        var entry = new ConversationEntry(userRequest, generatedCode, executionResult, DateTime.Now);
        _conversationHistory.Add(entry);

        // Keep only the last 10 entries
        while (_conversationHistory.Count > MaxHistoryEntries)
        {
            _conversationHistory.RemoveAt(0);
        }
    }

    public void ClearHistory()
    {
        _conversationHistory.Clear();
    }

    public async Task<List<string>> FetchAvailableOllamaModelsAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            var response = await _httpClient.GetAsync("http://localhost:11434/api/tags", cancellationToken);
            
            if (!response.IsSuccessStatusCode)
            {
                return new List<string>();
            }

            var content = await response.Content.ReadAsStringAsync(cancellationToken);
            var tagsResponse = JsonSerializer.Deserialize<OllamaTagsResponse>(content);
            
            return tagsResponse?.Models?.Select(m => m.Name).ToList() ?? new List<string>();
        }
        catch
        {
            return new List<string>();
        }
    }

    public List<string> GetAvailableGroqModels()
    {
        return new List<string>
        {
            "openai/gpt-oss-120b",
            "llama-3.3-70b-versatile",
            "llama3-70b-8192",
            "llama3-8b-8192",
            "mixtral-8x7b-32768",
            "gemma2-9b-it"
        };
    }

    private string CreatePrompt(string userRequest, LLMProvider provider)
    {
        var basePrompt = provider == LLMProvider.Ollama
            ? $"""
            Generate Python code for: {userRequest}
            
            Rules:
            1. Include all imports (os, shutil, pathlib)
            2. Use try/except for error handling
            3. Print clear messages
            4. No input() or interactive code
            5. Return ONLY the Python code
            """
            : """
            You are a helpful AI assistant that generates Python code to control a Windows computer.
            The user will describe what they want to do in natural language, and you should generate safe, appropriate Python code to accomplish that task.
            
            IMPORTANT SAFETY RULES:
            1. Generate code for file operations (move, copy, rename, delete, create directories) AND listing/inspecting files
            2. Always use proper error handling with try-catch blocks
            3. Never generate code that could harm the system or access sensitive data (like system passwords)
            4. ALWAYS include ALL necessary import statements at the top (import os, import shutil, import pathlib, etc.)
            4. ALWAYS include ALL necessary import statements at the top (import os, import shutil, import pathlib, etc.)
            5. Use the os, shutil, pathlib, and other standard Python libraries
            6. Always check if files/directories exist before operating on them
            7. Provide clear, descriptive output messages
            8. NEVER use input() or any interactive prompts - the code must run automatically
            9. Do not ask for user confirmation in the code - assume the user has already confirmed
            """;

        var contextPrompt = basePrompt;

        // Add conversation history for context (less for Ollama to be faster)
        if (_conversationHistory.Count > 0 && provider != LLMProvider.Ollama)
        {
            contextPrompt += "\n\nPREVIOUS CONVERSATION HISTORY:\n";
            var recentHistory = _conversationHistory.TakeLast(3);
            var index = 1;
            foreach (var entry in recentHistory)
            {
                contextPrompt += $"\n--- Previous Command {index} ---\n";
                contextPrompt += $"User: {entry.UserRequest}\n";
                contextPrompt += $"Generated Code:\n{entry.GeneratedCode}\n";
                if (!string.IsNullOrEmpty(entry.ExecutionResult))
                {
                    contextPrompt += $"Result: {entry.ExecutionResult}\n";
                }
                index++;
            }
            contextPrompt += "\nYou can reference files, folders, or results from the previous commands above.\n";
        }
        else if (_conversationHistory.Count > 0 && provider == LLMProvider.Ollama)
        {
            // Minimal context for Ollama
            var lastEntry = _conversationHistory.Last();
            contextPrompt += $"\nLast command: {lastEntry.UserRequest}\n";
        }

        contextPrompt += $"""
        
        Current User Request: {userRequest}
        
        Generate Python code that:
        1. STARTS with ALL necessary import statements (import os, import shutil, import pathlib, etc.)
        2. Safely performs the requested operation
        3. Includes proper error handling
        4. Provides user feedback through print statements
        5. Is ready to execute without any user interaction
        6. Can reference previous results if the user is asking for follow-up operations
        7. Runs completely automatically (no input(), confirm prompts, or user interaction)
        
        CRITICAL: The code will run in a non-interactive environment. Do NOT include:
        - input() calls
        - confirmation prompts
        - any code that waits for user input
        
        Return ONLY the Python code, no explanations or markdown formatting.
        """;

        return contextPrompt;
    }

    private HttpRequestMessage CreateRequest(string prompt, LLMProvider provider, string? selectedModel)
    {
        var request = new HttpRequestMessage(HttpMethod.Post, provider.GetBaseUrl());
        request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

        var model = selectedModel ?? provider.GetDefaultModel();

        object requestBody;

        switch (provider)
        {
            case LLMProvider.DeepSeek:
            case LLMProvider.OpenAI:
            case LLMProvider.Groq:
                var apiKey = _settingsService.GetApiKey(provider);
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", apiKey);
                
                // Use System message for better instruction following
                var systemMessage = "You are a Python Code Generator. Your ONLY purpose is to output executable Python code. " +
                                  "Do NOT output any explanations, markdown, conversational text, or apologies. " +
                                  "If a request is unclear, generate code that prints an error message. " +
                                  "Return ONLY valid Python code.";

                requestBody = new
                {
                    model,
                    messages = new[]
                    {
                        new { role = "system", content = systemMessage },
                        new { role = "user", content = prompt }
                    },
                    temperature = 0.1,
                    max_tokens = 1000
                };
                break;

            case LLMProvider.Claude:
                var claudeApiKey = _settingsService.GetApiKey(provider);
                request.Headers.Add("x-api-key", claudeApiKey);
                request.Headers.Add("anthropic-version", "2023-06-01");
                requestBody = new
                {
                    model,
                    max_tokens = 1000,
                    messages = new[]
                    {
                        new { role = "user", content = prompt }
                    }
                };
                break;

            case LLMProvider.Ollama:
                requestBody = new
                {
                    model,
                    prompt,
                    stream = false,
                    options = new
                    {
                        temperature = 0.1,
                        top_p = 0.9,
                        max_tokens = 1000
                    }
                };
                break;

            default:
                throw new ArgumentException($"Unknown provider: {provider}");
        }

        var json = JsonSerializer.Serialize(requestBody);
        request.Content = new StringContent(json, Encoding.UTF8, "application/json");

        return request;
    }

    private GeneratedCode ParseResponse(string responseContent, LLMProvider provider)
    {
        string content;

        switch (provider)
        {
            case LLMProvider.DeepSeek:
            case LLMProvider.OpenAI:
            case LLMProvider.Groq:
                var openAiResponse = JsonSerializer.Deserialize<OpenAIResponse>(responseContent);
                content = openAiResponse?.Choices?.FirstOrDefault()?.Message?.Content 
                    ?? throw new InvalidOperationException("Invalid response from API");
                break;

            case LLMProvider.Claude:
                var claudeResponse = JsonSerializer.Deserialize<ClaudeResponse>(responseContent);
                content = claudeResponse?.Content?.FirstOrDefault()?.Text 
                    ?? throw new InvalidOperationException("Invalid response from API");
                break;

            case LLMProvider.Ollama:
                var ollamaResponse = JsonSerializer.Deserialize<OllamaResponse>(responseContent);
                content = ollamaResponse?.Response 
                    ?? throw new InvalidOperationException("Invalid response from API");
                break;

            default:
                throw new ArgumentException($"Unknown provider: {provider}");
        }

        // Clean up the response to extract just the code
        var cleanCode = content.Trim()
            .Replace("```python", "")
            .Replace("```", "")
            .Trim();

        return new GeneratedCode(cleanCode, "Generated Python code");
    }

    // Response models
    private class OpenAIResponse
    {
        [JsonPropertyName("choices")]
        public List<OpenAIChoice>? Choices { get; set; }
    }

    private class OpenAIChoice
    {
        [JsonPropertyName("message")]
        public OpenAIMessage? Message { get; set; }
    }

    private class OpenAIMessage
    {
        [JsonPropertyName("content")]
        public string? Content { get; set; }
    }

    private class ClaudeResponse
    {
        [JsonPropertyName("content")]
        public List<ClaudeContent>? Content { get; set; }
    }

    private class ClaudeContent
    {
        [JsonPropertyName("text")]
        public string? Text { get; set; }
    }

    private class OllamaResponse
    {
        [JsonPropertyName("response")]
        public string? Response { get; set; }
    }

    private class OllamaTagsResponse
    {
        [JsonPropertyName("models")]
        public List<OllamaModelInfo>? Models { get; set; }
    }

    private class OllamaModelInfo
    {
        [JsonPropertyName("name")]
        public string Name { get; set; } = string.Empty;
    }
}
