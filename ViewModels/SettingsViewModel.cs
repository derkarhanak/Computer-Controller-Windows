using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ComputerController.Models;
using ComputerController.Services;
using ComputerController.Helpers;
using System.Collections.ObjectModel;

namespace ComputerController.ViewModels;

public partial class SettingsViewModel : ObservableObject
{
    private readonly LLMService _llmService;
    private readonly SettingsService _settingsService;

    [ObservableProperty]
    private LLMProvider _selectedProvider;

    [ObservableProperty]
    private bool _isConnected;

    [ObservableProperty]
    private bool _isDarkMode;

    [ObservableProperty]
    private string _selectedGroqModel = string.Empty;

    [ObservableProperty]
    private string _selectedOllamaModel = string.Empty;

    public ObservableCollection<string> AvailableGroqModels { get; } = new();
    public ObservableCollection<string> AvailableOllamaModels { get; } = new();

    public SettingsViewModel(LLMService llmService, SettingsService settingsService)
    {
        _llmService = llmService;
        _settingsService = settingsService;

        _selectedProvider = _settingsService.GetSelectedProvider();
        _isDarkMode = _settingsService.GetIsDarkMode();
        _selectedGroqModel = _settingsService.GetSelectedGroqModel();
        _selectedOllamaModel = _settingsService.GetSelectedOllamaModel();

        UpdateConnectionStatus();
        LoadAvailableModels();
    }

    partial void OnSelectedProviderChanged(LLMProvider value)
    {
        _settingsService.SetSelectedProvider(value);
        UpdateConnectionStatus();
        
        if (value == LLMProvider.Ollama)
        {
            _ = LoadOllamaModelsAsync();
        }
    }

    partial void OnIsDarkModeChanged(bool value)
    {
        _settingsService.SetIsDarkMode(value);
    }

    partial void OnSelectedGroqModelChanged(string value)
    {
        if (!string.IsNullOrEmpty(value))
        {
            _settingsService.SetSelectedGroqModel(value);
        }
    }

    partial void OnSelectedOllamaModelChanged(string value)
    {
        if (!string.IsNullOrEmpty(value))
        {
            _settingsService.SetSelectedOllamaModel(value);
        }
    }

    [RelayCommand]
    private void SelectProvider(LLMProvider provider)
    {
        SelectedProvider = provider;
    }

    [RelayCommand]
    private async Task OpenDirectoryAsync(string directoryType)
    {
        var path = directoryType switch
        {
            "Downloads" => FileHelper.GetDownloadsPath(),
            "Desktop" => FileHelper.GetDesktopPath(),
            "Documents" => FileHelper.GetDocumentsPath(),
            "Pictures" => FileHelper.GetPicturesPath(),
            _ => null
        };

        if (!string.IsNullOrEmpty(path))
        {
            await FileHelper.OpenDirectoryAsync(path);
        }
    }

    public void SetApiKey(string apiKey)
    {
        _settingsService.SetApiKey(SelectedProvider, apiKey);
        UpdateConnectionStatus();
    }

    public string GetApiKey()
    {
        return _settingsService.GetApiKey(SelectedProvider);
    }

    public void UpdateConnectionStatus()
    {
        IsConnected = _llmService.IsConnected(SelectedProvider);
    }

    private void LoadAvailableModels()
    {
        // Load Groq models
        AvailableGroqModels.Clear();
        foreach (var model in _llmService.GetAvailableGroqModels())
        {
            AvailableGroqModels.Add(model);
        }

        // Load Ollama models if provider is Ollama
        if (SelectedProvider == LLMProvider.Ollama)
        {
            _ = LoadOllamaModelsAsync();
        }
    }

    private async Task LoadOllamaModelsAsync()
    {
        try
        {
            var models = await _llmService.FetchAvailableOllamaModelsAsync();
            AvailableOllamaModels.Clear();
            foreach (var model in models)
            {
                AvailableOllamaModels.Add(model);
            }

            // If current selected model is not available, use the first one
            if (models.Count > 0 && !models.Contains(SelectedOllamaModel))
            {
                SelectedOllamaModel = models[0];
            }
        }
        catch
        {
            // Failed to load Ollama models
        }
    }

    public string GetProviderIcon(LLMProvider provider) => ProviderHelper.GetIcon(provider);
    
    public Microsoft.UI.Xaml.Media.SolidColorBrush GetProviderColor(LLMProvider provider) => ProviderHelper.GetColorBrush(provider);
}
