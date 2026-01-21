namespace ComputerController.Models;

public enum LLMProvider
{
    DeepSeek,
    OpenAI,
    Claude,
    Groq,
    Ollama
}

public static class LLMProviderExtensions
{
    public static string GetDisplayName(this LLMProvider provider) => provider switch
    {
        LLMProvider.DeepSeek => "DeepSeek",
        LLMProvider.OpenAI => "OpenAI",
        LLMProvider.Claude => "Anthropic Claude",
        LLMProvider.Groq => "Groq",
        LLMProvider.Ollama => "Ollama (Local)",
        _ => provider.ToString()
    };

    public static string GetBaseUrl(this LLMProvider provider) => provider switch
    {
        LLMProvider.DeepSeek => "https://api.deepseek.com/v1/chat/completions",
        LLMProvider.OpenAI => "https://api.openai.com/v1/chat/completions",
        LLMProvider.Claude => "https://api.anthropic.com/v1/messages",
        LLMProvider.Groq => "https://api.groq.com/openai/v1/chat/completions",
        LLMProvider.Ollama => "http://localhost:11434/api/generate",
        _ => throw new ArgumentException($"Unknown provider: {provider}")
    };

    public static string GetDefaultModel(this LLMProvider provider) => provider switch
    {
        LLMProvider.DeepSeek => "deepseek-chat",
        LLMProvider.OpenAI => "gpt-4",
        LLMProvider.Claude => "claude-3-sonnet-20240229",
        LLMProvider.Groq => "llama3-70b-8192",
        LLMProvider.Ollama => "llama3.2:3b",
        _ => throw new ArgumentException($"Unknown provider: {provider}")
    };

    public static bool RequiresApiKey(this LLMProvider provider) => provider switch
    {
        LLMProvider.DeepSeek => true,
        LLMProvider.OpenAI => true,
        LLMProvider.Claude => true,
        LLMProvider.Groq => true,
        LLMProvider.Ollama => false,
        _ => true
    };

    public static string GetApiKeyUrl(this LLMProvider provider) => provider switch
    {
        LLMProvider.DeepSeek => "https://platform.deepseek.com",
        LLMProvider.OpenAI => "https://platform.openai.com",
        LLMProvider.Claude => "https://console.anthropic.com",
        LLMProvider.Groq => "https://console.groq.com",
        LLMProvider.Ollama => "",
        _ => ""
    };
}
