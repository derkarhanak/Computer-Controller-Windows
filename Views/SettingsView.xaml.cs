using Microsoft.UI;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using ComputerController.ViewModels;
using ComputerController.Models;
using ComputerController.Helpers;
using Windows.UI;
using System.Linq;
using ComputerController;

namespace ComputerController.Views;

public sealed partial class SettingsView : Page
{
    private readonly SettingsViewModel _viewModel;
    private readonly MainViewModel _mainViewModel;

    public SettingsView()
    {
        this.InitializeComponent();
        _viewModel = App.Services.GetService(typeof(SettingsViewModel)) as SettingsViewModel 
            ?? throw new InvalidOperationException("SettingsViewModel not found");
        _mainViewModel = App.Services.GetService(typeof(MainViewModel)) as MainViewModel 
            ?? throw new InvalidOperationException("MainViewModel not found");
        
        this.Loaded += OnLoaded;
        _viewModel.PropertyChanged += ViewModel_PropertyChanged;
    }

    private async void OnLoaded(object sender, RoutedEventArgs e)
    {
        LoadProviders();
        UpdateUI();
        ThemeToggle.IsOn = _viewModel.IsDarkMode;
        UpdateTheme(); // Apply theme on initial load
        
        // Delay to ensure provider buttons are rendered before styling them
        await Task.Delay(100);
        UpdateProviderButtons();
    }

    private void LoadProviders()
    {
        var providers = Enum.GetValues<LLMProvider>().ToList();
        ProviderGrid.ItemsSource = providers;
    }

    private void Back_Click(object sender, RoutedEventArgs e)
    {
        if (Frame.CanGoBack)
        {
            Frame.GoBack();
        }
    }

    private void ViewModel_PropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
    {
        DispatcherQueue.TryEnqueue(() =>
        {
            switch (e.PropertyName)
            {
                case nameof(_viewModel.SelectedProvider):
                case nameof(_viewModel.IsConnected):
                    UpdateUI();
                    _mainViewModel.OnProviderChanged();
                    break;
                case nameof(_viewModel.IsDarkMode):
                    UpdateTheme();
                    break;
            }
        });
    }

    private void UpdateUI()
    {
        // Update current provider text
        CurrentProviderText.Text = $"Using: {_viewModel.SelectedProvider.GetDisplayName()}";

        // Update API key status
        if (_viewModel.SelectedProvider.RequiresApiKey())
        {
            SetApiKeyButton.Visibility = Visibility.Visible;
            ApiKeyStatusIndicator.Fill = new SolidColorBrush(_viewModel.IsConnected ? Colors.Green : Colors.Red);
            ApiKeyStatusText.Text = _viewModel.IsConnected ? "Connected" : "Not Connected";
            ApiKeyStatusText.Foreground = new SolidColorBrush(_viewModel.IsConnected ? Colors.Green : Colors.Red);
            ApiKeyButtonText.Text = _viewModel.IsConnected ? "Change Key" : "Set Key";

            if (_viewModel.IsConnected)
            {
                ApiKeyInfoBar.Message = $"✓ Your {_viewModel.SelectedProvider.GetDisplayName()} API key is configured and working";
                ApiKeyInfoBar.Severity = InfoBarSeverity.Success;
            }
            else
            {
                ApiKeyInfoBar.Message = $"⚠ Please set your {_viewModel.SelectedProvider.GetDisplayName()} API key to use the app";
                ApiKeyInfoBar.Severity = InfoBarSeverity.Warning;
            }
        }
        else
        {
            SetApiKeyButton.Visibility = Visibility.Collapsed;
            ApiKeyStatusIndicator.Fill = new SolidColorBrush(Colors.Green);
            ApiKeyStatusText.Text = "Ready";
            ApiKeyStatusText.Foreground = new SolidColorBrush(Colors.Green);
            ApiKeyInfoBar.Message = $"✓ {_viewModel.SelectedProvider.GetDisplayName()} is ready to use (local AI)";
            ApiKeyInfoBar.Severity = InfoBarSeverity.Success;
        }

        // Update model selection visibility
        if (_viewModel.SelectedProvider == LLMProvider.Groq && _viewModel.AvailableGroqModels.Count > 0)
        {
            ModelSelectionPanel.Visibility = Visibility.Visible;
            ModelComboBox.ItemsSource = _viewModel.AvailableGroqModels;
            ModelComboBox.SelectedItem = _viewModel.SelectedGroqModel;
        }
        else if (_viewModel.SelectedProvider == LLMProvider.Ollama && _viewModel.AvailableOllamaModels.Count > 0)
        {
            ModelSelectionPanel.Visibility = Visibility.Visible;
            ModelComboBox.ItemsSource = _viewModel.AvailableOllamaModels;
            ModelComboBox.SelectedItem = _viewModel.SelectedOllamaModel;
        }
        else
        {
            ModelSelectionPanel.Visibility = Visibility.Collapsed;
        }

        // Update provider buttons styling
        UpdateProviderButtons();
    }

    private void UpdateProviderButtons()
    {
        // This would ideally be done with data binding, but for simplicity we'll update in code
        for (int i = 0; i < ProviderGrid.Items.Count; i++)
        {
            var container = ProviderGrid.ContainerFromIndex(i) as GridViewItem;
            if (container != null)
            {
                var provider = (LLMProvider)ProviderGrid.Items[i];
                var border = FindChild<Border>(container, "ProviderButton");
                var icon = FindChild<FontIcon>(container, "ProviderIcon");
                var name = FindChild<TextBlock>(container, "ProviderName");
                var badge = FindChild<Border>(container, "LocalBadge");

                if (border != null && icon != null && name != null)
                {
                    var isSelected = provider == _viewModel.SelectedProvider;
                    var color = ProviderHelper.GetColor(provider);
                    var iconGlyph = ProviderHelper.GetIcon(provider);

                    icon.Glyph = iconGlyph;
                    icon.Foreground = new SolidColorBrush(isSelected ? color : Colors.Gray);
                    name.Text = provider.GetDisplayName();
                    name.Foreground = new SolidColorBrush(isSelected ? color : Colors.Gray);
                    border.Background = new SolidColorBrush(isSelected ? Color.FromArgb(25, color.R, color.G, color.B) : Colors.Transparent);
                    border.BorderBrush = new SolidColorBrush(isSelected ? color : Colors.Gray);
                    border.BorderThickness = new Thickness(isSelected ? 2 : 1);

                    if (badge != null)
                    {
                        badge.Visibility = !provider.RequiresApiKey() ? Visibility.Visible : Visibility.Collapsed;
                    }
                }
            }
        }
    }

    private void UpdateTheme()
    {
        // Update the app's theme
        if (App.MainWindow.Content is FrameworkElement rootElement)
        {
            rootElement.RequestedTheme = _viewModel.IsDarkMode ? ElementTheme.Dark : ElementTheme.Light;
        }
        
        // Update theme toggle appearance
        var isDark = _viewModel.IsDarkMode;
        LightIcon.Foreground = new SolidColorBrush(isDark ? Colors.Gray : Color.FromArgb(255, 255, 165, 0));
        LightText.Foreground = new SolidColorBrush(isDark ? Colors.Gray : Colors.Black);
        DarkIcon.Foreground = new SolidColorBrush(isDark ? Color.FromArgb(255, 30, 144, 255) : Colors.Gray);
        DarkText.Foreground = new SolidColorBrush(isDark ? Colors.White : Colors.Gray);
    }

    private void ProviderGrid_ItemClick(object sender, ItemClickEventArgs e)
    {
        if (e.ClickedItem is LLMProvider provider)
        {
            _viewModel.SelectProviderCommand.Execute(provider);
        }
    }

    private async void SetApiKey_Click(object sender, RoutedEventArgs e)
    {
        var dialog = new ContentDialog
        {
            Title = $"{_viewModel.SelectedProvider.GetDisplayName()} API Key",
            PrimaryButtonText = "Save",
            CloseButtonText = "Cancel",
            DefaultButton = ContentDialogButton.Primary,
            XamlRoot = this.XamlRoot
        };

        var stackPanel = new StackPanel { Spacing = 12 };
        
        var infoText = new TextBlock
        {
            Text = $"Enter your {_viewModel.SelectedProvider.GetDisplayName()} API key to enable AI-powered file operations.",
            TextWrapping = TextWrapping.Wrap
        };
        stackPanel.Children.Add(infoText);

        var apiKeyUrl = _viewModel.SelectedProvider.GetApiKeyUrl();
        if (!string.IsNullOrEmpty(apiKeyUrl))
        {
            var linkText = new HyperlinkButton
            {
                Content = $"Get your API key from {apiKeyUrl}",
                NavigateUri = new Uri(apiKeyUrl)
            };
            stackPanel.Children.Add(linkText);
        }

        var passwordBox = new PasswordBox
        {
            PlaceholderText = "API Key",
            Password = _viewModel.GetApiKey()
        };
        stackPanel.Children.Add(passwordBox);

        dialog.Content = stackPanel;

        var result = await dialog.ShowAsync();
        if (result == ContentDialogResult.Primary && !string.IsNullOrWhiteSpace(passwordBox.Password))
        {
            _viewModel.SetApiKey(passwordBox.Password);
        }
    }

    private async void OpenDirectory_Click(object sender, RoutedEventArgs e)
    {
        if (sender is Button button && button.Tag is string directoryType)
        {
            await _viewModel.OpenDirectoryCommand.ExecuteAsync(directoryType);
        }
    }

    private void ThemeToggle_Toggled(object sender, RoutedEventArgs e)
    {
        _viewModel.IsDarkMode = ThemeToggle.IsOn;
    }

    private void ModelComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (ModelComboBox.SelectedItem is string model)
        {
            if (_viewModel.SelectedProvider == LLMProvider.Groq)
            {
                _viewModel.SelectedGroqModel = model;
            }
            else if (_viewModel.SelectedProvider == LLMProvider.Ollama)
            {
                _viewModel.SelectedOllamaModel = model;
            }
        }
    }

    private void ExampleCommand_Click(object sender, RoutedEventArgs e)
    {
        if (sender is Button button && button.Tag is string commandType)
        {
            string command = commandType switch
            {
                "Move PDFs" => "Move all .pdf files from Downloads to Documents",
                "Rename Images" => "Rename all image files in Pictures folder with date prefix",
                "Backup Files" => "Create a backup of Documents folder to Desktop",
                "Clean Downloads" => "Delete empty folders in Downloads",
                _ => string.Empty
            };

            if (!string.IsNullOrEmpty(command))
            {
                // Get the ComputerControlViewModel
                var computerControlViewModel = App.Services.GetService(typeof(ComputerControlViewModel)) as ComputerControlViewModel;
                if (computerControlViewModel != null)
                {
                    computerControlViewModel.UserInput = command;
                }

                // Navigate to ComputerControlView
                // Check if we can change selection in MainWindow
                if (App.MainWindow is MainWindow mainWindow && mainWindow.Content is Grid rootGrid)
                {
                    // Find NavigationView to update selection
                    // Since specific access is hard without public properties, we'll try just Frame navigation first
                    // But to update the NavView selection, we might need a reference.
                    // Ideally MainWindow should expose a method. 
                    // For now, let's just use the Frame if available.
                }

                this.Frame.Navigate(typeof(ComputerControlView));
            }
        }
    }

    private T? FindChild<T>(DependencyObject parent, string childName) where T : DependencyObject
    {
        if (parent == null) return null;

        int childrenCount = VisualTreeHelper.GetChildrenCount(parent);
        for (int i = 0; i < childrenCount; i++)
        {
            var child = VisualTreeHelper.GetChild(parent, i);
            
            if (child is T typedChild && (child as FrameworkElement)?.Name == childName)
            {
                return typedChild;
            }

            var foundChild = FindChild<T>(child, childName);
            if (foundChild != null) return foundChild;
        }

        return null;
    }
}
