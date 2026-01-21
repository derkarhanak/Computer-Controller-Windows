using ComputerController.Models;
using System.Text.Json;

namespace ComputerController.Services;

public class SettingsService
{
    private readonly string _settingsFilePath;
    private Dictionary<string, string> _settings;

    private const string SelectedProviderKey = "SelectedProvider";
    private const string IsDarkModeKey = "IsDarkMode";
    private const string SelectedGroqModelKey = "SelectedGroqModel";
    private const string SelectedOllamaModelKey = "SelectedOllamaModel";

    public SettingsService()
    {
        var appDataPath = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
        var appFolder = Path.Combine(appDataPath, "ComputerController");
        Directory.CreateDirectory(appFolder);
        _settingsFilePath = Path.Combine(appFolder, "settings.json");
        
        LoadSettings();
    }

    private void LoadSettings()
    {
        try
        {
            if (File.Exists(_settingsFilePath))
            {
                var json = File.ReadAllText(_settingsFilePath);
                _settings = JsonSerializer.Deserialize<Dictionary<string, string>>(json) ?? new Dictionary<string, string>();
            }
            else
            {
                _settings = new Dictionary<string, string>();
            }
        }
        catch
        {
            _settings = new Dictionary<string, string>();
        }
    }

    private void SaveSettings()
    {
        try
        {
            var json = JsonSerializer.Serialize(_settings, new JsonSerializerOptions { WriteIndented = true });
            File.WriteAllText(_settingsFilePath, json);
        }
        catch
        {
            // Ignore save errors
        }
    }

    public LLMProvider GetSelectedProvider()
    {
        if (_settings.TryGetValue(SelectedProviderKey, out var value))
        {
            if (Enum.TryParse<LLMProvider>(value, out var provider))
            {
                return provider;
            }
        }
        return LLMProvider.DeepSeek; // Default
    }

    public void SetSelectedProvider(LLMProvider provider)
    {
        _settings[SelectedProviderKey] = provider.ToString();
        SaveSettings();
    }

    public string GetApiKey(LLMProvider provider)
    {
        var key = $"ApiKey_{provider}";
        if (_settings.TryGetValue(key, out var value))
        {
            return value;
        }
        return string.Empty;
    }

    public void SetApiKey(LLMProvider provider, string apiKey)
    {
        var key = $"ApiKey_{provider}";
        _settings[key] = apiKey;
        SaveSettings();
    }

    public bool GetIsDarkMode()
    {
        if (_settings.TryGetValue(IsDarkModeKey, out var value))
        {
            return bool.TryParse(value, out var isDark) && isDark;
        }
        return false; // Default to light mode
    }

    public void SetIsDarkMode(bool isDark)
    {
        _settings[IsDarkModeKey] = isDark.ToString();
        SaveSettings();
    }

    public string GetSelectedGroqModel()
    {
        if (_settings.TryGetValue(SelectedGroqModelKey, out var value))
        {
            return value;
        }
        return "openai/gpt-oss-120b"; // Default
    }

    public void SetSelectedGroqModel(string model)
    {
        _settings[SelectedGroqModelKey] = model;
        SaveSettings();
    }

    public string GetSelectedOllamaModel()
    {
        if (_settings.TryGetValue(SelectedOllamaModelKey, out var value))
        {
            return value;
        }
        return "llama3.2:3b"; // Default
    }

    public void SetSelectedOllamaModel(string model)
    {
        _settings[SelectedOllamaModelKey] = model;
        SaveSettings();
    }

    // Favorites
    private const string FavoriteCommandsKey = "FavoriteCommands";

    public List<string> GetFavoriteCommands()
    {
        if (_settings.TryGetValue(FavoriteCommandsKey, out var value))
        {
            try 
            {
                return JsonSerializer.Deserialize<List<string>>(value) ?? new List<string>();
            }
            catch 
            { 
                return new List<string>(); 
            }
        }
        return new List<string>();
    }

    public void AddFavoriteCommand(string command)
    {
        var favorites = GetFavoriteCommands();
        if (!favorites.Contains(command))
        {
            favorites.Add(command);
            SaveFavorites(favorites);
        }
    }

    public void RemoveFavoriteCommand(string command)
    {
        var favorites = GetFavoriteCommands();
        if (favorites.Contains(command))
        {
            favorites.Remove(command);
            SaveFavorites(favorites);
        }
    }

    private void SaveFavorites(List<string> favorites)
    {
        _settings[FavoriteCommandsKey] = JsonSerializer.Serialize(favorites);
        SaveSettings();
    }
}
