using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ComputerController.Models;
using ComputerController.Services;
using System.Collections.ObjectModel;

namespace ComputerController.ViewModels;

public partial class ComputerControlViewModel : ObservableObject
{
    private readonly LLMService _llmService;
    private readonly PythonExecutor _pythonExecutor;
    private readonly SettingsService _settingsService;

    [ObservableProperty]
    private string _userInput = string.Empty;

    [ObservableProperty]
    private string _generatedCode = string.Empty;

    [ObservableProperty]
    private string _operationDescription = string.Empty;

    [ObservableProperty]
    private string? _executionResult;

    [ObservableProperty]
    private bool _isProcessing;

    [ObservableProperty]
    private bool _showConfirmation;

    [ObservableProperty]
    private LLMProvider _selectedProvider;

    [ObservableProperty]
    private bool _isConnected;

    [ObservableProperty]
    private string _selectedModel = string.Empty;

    public ObservableCollection<ConversationEntry> ConversationHistory { get; } = new();
    public ObservableCollection<string> FavoriteCommands { get; } = new();

    public ComputerControlViewModel(LLMService llmService, PythonExecutor pythonExecutor, SettingsService settingsService)
    {
        _llmService = llmService;
        _pythonExecutor = pythonExecutor;
        _settingsService = settingsService;
        
        _selectedProvider = _settingsService.GetSelectedProvider();
        UpdateConnectionStatus();
        LoadSelectedModel();
        LoadFavorites();
    }

    [RelayCommand]
    private void LoadFavorites()
    {
        FavoriteCommands.Clear();
        foreach (var cmd in _settingsService.GetFavoriteCommands())
        {
            FavoriteCommands.Add(cmd);
        }
    }

    [RelayCommand]
    private void AddToFavorites(string command)
    {
        if (!string.IsNullOrWhiteSpace(command) && !FavoriteCommands.Contains(command))
        {
            _settingsService.AddFavoriteCommand(command);
            LoadFavorites();
        }
    }

    [RelayCommand]
    private void RemoveFromFavorites(string command)
    {
        if (FavoriteCommands.Contains(command))
        {
            _settingsService.RemoveFavoriteCommand(command);
            LoadFavorites();
        }
    }

    [RelayCommand]
    private void UseCommand(string command)
    {
        UserInput = command;
    }

    [RelayCommand]
    private async Task ExecuteAsync()
    {
        if (string.IsNullOrWhiteSpace(UserInput) || IsProcessing)
            return;

        IsProcessing = true;
        ExecutionResult = null;

        try
        {
            // Reload the selected model from settings before making the API call
            LoadSelectedModel();
            
            var result = await _llmService.GenerateCodeAsync(UserInput, SelectedProvider, SelectedModel);
            GeneratedCode = result.Code;
            OperationDescription = result.Description;
            ShowConfirmation = true;
        }
        catch (Exception ex)
        {
            ExecutionResult = $"Error generating code: {ex.Message}";
        }
        finally
        {
            IsProcessing = false;
        }
    }

    [RelayCommand]
    private async Task ConfirmExecutionAsync()
    {
        ShowConfirmation = false;
        
        if (string.IsNullOrWhiteSpace(GeneratedCode))
            return;

        var currentUserInput = UserInput;
        var currentGeneratedCode = GeneratedCode;

        try
        {
            var result = await _pythonExecutor.ExecuteAsync(GeneratedCode);
            ExecutionResult = result;

            // Add to conversation history
            _llmService.AddToHistory(currentUserInput, currentGeneratedCode, result);
            RefreshConversationHistory();
        }
        catch (Exception ex)
        {
            var errorMessage = $"Error: {ex.Message}";
            ExecutionResult = errorMessage;

            // Add to conversation history even if there was an error
            _llmService.AddToHistory(currentUserInput, currentGeneratedCode, errorMessage);
            RefreshConversationHistory();
        }
    }

    [RelayCommand]
    private void CancelExecution()
    {
        ShowConfirmation = false;
    }

    [RelayCommand]
    private void Clear()
    {
        UserInput = string.Empty;
        GeneratedCode = string.Empty;
        OperationDescription = string.Empty;
        ExecutionResult = null;
    }

    [RelayCommand]
    private void ClearHistory()
    {
        _llmService.ClearHistory();
        RefreshConversationHistory();
    }

    public void UpdateConnectionStatus()
    {
        IsConnected = _llmService.IsConnected(SelectedProvider);
    }

    public void UpdateProvider(LLMProvider provider)
    {
        SelectedProvider = provider;
        UpdateConnectionStatus();
        LoadSelectedModel();
    }

    private void LoadSelectedModel()
    {
        SelectedModel = SelectedProvider switch
        {
            LLMProvider.Groq => _settingsService.GetSelectedGroqModel(),
            LLMProvider.Ollama => _settingsService.GetSelectedOllamaModel(),
            _ => SelectedProvider.GetDefaultModel()
        };
    }

    private void RefreshConversationHistory()
    {
        ConversationHistory.Clear();
        // Show newest first
        var history = _llmService.ConversationHistory.ToList();
        history.Reverse();
        foreach (var entry in history)
        {
            ConversationHistory.Add(entry);
        }
    }

    public string GetProviderIcon() => Helpers.ProviderHelper.GetIcon(SelectedProvider);
    
    public Microsoft.UI.Xaml.Media.SolidColorBrush GetProviderColor() => Helpers.ProviderHelper.GetColorBrush(SelectedProvider);
}
