using Microsoft.UI;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using ComputerController.ViewModels;
using ComputerController.Models;
using Windows.UI;

namespace ComputerController.Views;

public sealed partial class ComputerControlView : Page
{
    private readonly ComputerControlViewModel _viewModel;
    private bool _isDialogOpen = false;

    public ComputerControlView()
    {
        this.InitializeComponent();
        this.NavigationCacheMode = Microsoft.UI.Xaml.Navigation.NavigationCacheMode.Required;

        _viewModel = App.Services.GetService(typeof(ComputerControlViewModel)) as ComputerControlViewModel
            ?? throw new InvalidOperationException("ComputerControlViewModel not found");

        this.Loaded += OnLoaded;

        // Subscribe to property changes
        _viewModel.PropertyChanged += ViewModel_PropertyChanged;
    }

    private void OnLoaded(object sender, RoutedEventArgs e)
    {
        UpdateUI();
    }

    private void ViewModel_PropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
    {
        DispatcherQueue.TryEnqueue(() =>
        {
            switch (e.PropertyName)
            {
                case nameof(_viewModel.IsProcessing):
                    UpdateProcessingState();
                    break;
                case nameof(_viewModel.GeneratedCode):
                    UpdateGeneratedCode();
                    break;
                case nameof(_viewModel.ExecutionResult):
                    UpdateExecutionResult();
                    break;
                case nameof(_viewModel.IsConnected):
                case nameof(_viewModel.SelectedProvider):
                    UpdateUI();
                    break;
                case nameof(_viewModel.ShowConfirmation):
                    if (_viewModel.ShowConfirmation && !_isDialogOpen)
                    {
                        _viewModel.ShowConfirmation = false; // Reset immediately to prevent re-triggering
                        ShowConfirmationDialog();
                    }
                    break;
            }
        });
    }

    private void UpdateUI()
    {
        // Update provider info
        // Update provider info
        // Provider info removed from main view
        // ProviderIcon.Glyph = _viewModel.GetProviderIcon();
        // ProviderIcon.Foreground = _viewModel.GetProviderColor();
        // ProviderText.Text = $"AI: {_viewModel.SelectedProvider.GetDisplayName()}";
        // ProviderText.Foreground = _viewModel.GetProviderColor();

        // Update connection status
        StatusIndicator.Fill = new SolidColorBrush(_viewModel.IsConnected ? Colors.Green : Colors.Red);
        StatusText.Text = _viewModel.IsConnected ? "Connected" : "Disconnected";

        // Update empty state message
        EmptyStateMessage.Text = _viewModel.IsConnected
            ? "Describe what you want to do in natural language, and I'll generate the appropriate Python code to execute it."
            : $"Please set your {_viewModel.SelectedProvider.GetDisplayName()} API key in the Settings tab to start using the app.";

        // Update history counter
        var historyCount = _viewModel.ConversationHistory.Count;
        if (historyCount > 0)
        {
            HistoryPanel.Visibility = Visibility.Visible;
            HistoryCount.Text = $"{historyCount} previous command{(historyCount > 1 ? "s" : "")}";
        }
        else
        {
            HistoryPanel.Visibility = Visibility.Collapsed;
        }
    }

    private void UpdateProcessingState()
    {
        ExecuteButton.IsEnabled = !_viewModel.IsProcessing && _viewModel.IsConnected && !string.IsNullOrWhiteSpace(UserInputBox.Text);

        if (_viewModel.IsProcessing)
        {
            LoadingRing.IsActive = true;
            LoadingRing.Visibility = Visibility.Visible;
            ExecuteIcon.Visibility = Visibility.Collapsed;
            ExecuteText.Text = "Processing...";
        }
        else
        {
            LoadingRing.IsActive = false;
            LoadingRing.Visibility = Visibility.Collapsed;
            ExecuteIcon.Visibility = Visibility.Visible;
            ExecuteText.Text = "Execute";
        }
    }



    private void UpdateExecutionResult()
    {
        if (!string.IsNullOrWhiteSpace(_viewModel.ExecutionResult))
        {
            ResultPanel.Visibility = Visibility.Visible;
            ResultText.Text = _viewModel.ExecutionResult;

            // Color the result based on success/error
            var isError = _viewModel.ExecutionResult.StartsWith("Error", StringComparison.OrdinalIgnoreCase);
            ResultBorder.Background = new SolidColorBrush(isError
                ? Color.FromArgb(25, 220, 20, 60)
                : Color.FromArgb(25, 16, 163, 127));
        }
        else
        {
            ResultPanel.Visibility = Visibility.Collapsed;
        }

        UpdateUI(); // Update history counter
    }

    private async void Execute_Click(object sender, RoutedEventArgs e)
    {
        _viewModel.UserInput = UserInputBox.Text;
        await _viewModel.ExecuteCommand.ExecuteAsync(null);
    }

    private void Settings_Click(object sender, RoutedEventArgs e)
    {
        Frame.Navigate(typeof(SettingsView));
    }

    private void Clear_Click(object sender, RoutedEventArgs e)
    {
        _viewModel.ClearCommand.Execute(null);
        UserInputBox.Text = string.Empty;
        UpdateGeneratedCode();
        UpdateExecutionResult();
    }

    private void ClearHistory_Click(object sender, RoutedEventArgs e)
    {
        _viewModel.ClearHistoryCommand.Execute(null);
        UpdateUI();
    }

    private void AddToFavorites_Click(object sender, RoutedEventArgs e)
    {
        if (!string.IsNullOrWhiteSpace(UserInputBox.Text))
        {
            _viewModel.AddToFavoritesCommand.Execute(UserInputBox.Text);
        }
    }

    private void Favorite_Click(object sender, RoutedEventArgs e)
    {
        if (sender is Button button && button.Content is string command)
        {
            _viewModel.UseCommandCommand.Execute(command);
            UserInputBox.Text = command; // Update visually as well
        }
    }

    private void HistoryItem_Click(object sender, ItemClickEventArgs e)
    {
        if (e.ClickedItem is ConversationEntry entry)
        {
            _viewModel.UseCommandCommand.Execute(entry.UserRequest);
            UserInputBox.Text = entry.UserRequest;
        }
    }

    private void HistoryFavorite_Click(object sender, RoutedEventArgs e)
    {
        if (sender is Button button && button.DataContext is ConversationEntry entry)
        {
            _viewModel.AddToFavoritesCommand.Execute(entry.UserRequest);
        }
    }

    private void UpdateGeneratedCode()
    {
        if (!string.IsNullOrWhiteSpace(_viewModel.GeneratedCode))
        {
            EmptyState.Visibility = Visibility.Collapsed;
            OperationPanel.Visibility = Visibility.Visible;
            // CodePanel.Visibility = Visibility.Collapsed; // Keep hidden as requested
            
            OperationText.Text = !string.IsNullOrWhiteSpace(_viewModel.OperationDescription) 
                ? _viewModel.OperationDescription 
                : "Generated Python code ready for execution.";
            CodeText.Text = _viewModel.GeneratedCode;
        }
        else
        {
            EmptyState.Visibility = Visibility.Visible;
            OperationPanel.Visibility = Visibility.Collapsed;
            CodePanel.Visibility = Visibility.Collapsed;
            ResultPanel.Visibility = Visibility.Collapsed;
        }
    }
    private async void ShowConfirmationDialog()
    {
        // Prevent multiple dialogs from opening
        if (_isDialogOpen)
        {
            return;
        }

        // Get XamlRoot from the main window's content
        var xamlRoot = (App.MainWindow.Content as FrameworkElement)?.XamlRoot;
        if (xamlRoot == null)
        {
            _viewModel.ExecutionResult = "Error: Unable to show confirmation dialog";
            return;
        }

        _isDialogOpen = true;

        // Create dialog programmatically to ensure XamlRoot is set correctly
        var dialog = new ContentDialog
        {
            Title = "Confirm Operation",
            PrimaryButtonText = "Execute",
            CloseButtonText = "Cancel",
            DefaultButton = ContentDialogButton.Primary,
            XamlRoot = xamlRoot
        };

        var content = new StackPanel { Spacing = 12 };
        content.Children.Add(new TextBlock
        {
            Text = "Are you sure you want to execute this operation?",
            TextWrapping = TextWrapping.Wrap
        });
        content.Children.Add(new TextBlock
        {
            Text = _viewModel.OperationDescription,
            FontWeight = Microsoft.UI.Text.FontWeights.SemiBold,
            TextWrapping = TextWrapping.Wrap
        });
        dialog.Content = content;

        try
        {
            var result = await dialog.ShowAsync();
            if (result == ContentDialogResult.Primary)
            {
                await _viewModel.ConfirmExecutionCommand.ExecuteAsync(null);
            }
        }
        finally
        {
            _isDialogOpen = false;
        }
    }
    private async void ViewCode_Click(object sender, RoutedEventArgs e)
    {
        if (string.IsNullOrEmpty(_viewModel.GeneratedCode)) return;

        var xamlRoot = (App.MainWindow.Content as FrameworkElement)?.XamlRoot;
        if (xamlRoot == null) return;

        var scrollViewer = new ScrollViewer
        {
            HorizontalScrollBarVisibility = ScrollBarVisibility.Auto,
            VerticalScrollBarVisibility = ScrollBarVisibility.Auto,
            HorizontalAlignment = HorizontalAlignment.Stretch,
            VerticalAlignment = VerticalAlignment.Stretch,
            MinHeight = 350,
            MaxHeight = 500,
            MinWidth = 600
        };

        var textBlock = new TextBlock
        {
            Text = _viewModel.GeneratedCode,
            FontFamily = new FontFamily("Consolas"),
            FontSize = 13,
            IsTextSelectionEnabled = true,
            TextWrapping = TextWrapping.NoWrap
        };

        scrollViewer.Content = textBlock;

        var dialog = new ContentDialog
        {
            Title = $"Generated Python Code ({_viewModel.GeneratedCode.Length} chars)",
            CloseButtonText = "Close",
            XamlRoot = xamlRoot,
            DefaultButton = ContentDialogButton.Close,
            Content = scrollViewer
        };

        await dialog.ShowAsync();
    }
}
