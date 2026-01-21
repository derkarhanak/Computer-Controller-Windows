using ComputerController.Models;
using Microsoft.UI;
using Microsoft.UI.Xaml.Media;
using Windows.UI;

namespace ComputerController.Helpers;

public static class ProviderHelper
{
    public static string GetIcon(LLMProvider provider) => provider switch
    {
        LLMProvider.DeepSeek => "\uE950", // Brain icon
        LLMProvider.OpenAI => "\uE950", // CPU/Chip icon
        LLMProvider.Claude => "\uE735", // Sparkles icon
        LLMProvider.Groq => "\uE945", // Lightning bolt icon
        LLMProvider.Ollama => "\uE80F", // Home icon
        _ => "\uE946"
    };

    public static SolidColorBrush GetColorBrush(LLMProvider provider) => provider switch
    {
        LLMProvider.DeepSeek => new SolidColorBrush(Color.FromArgb(255, 255, 140, 0)), // Orange
        LLMProvider.OpenAI => new SolidColorBrush(Color.FromArgb(255, 16, 163, 127)), // Green
        LLMProvider.Claude => new SolidColorBrush(Color.FromArgb(255, 138, 43, 226)), // Purple
        LLMProvider.Groq => new SolidColorBrush(Color.FromArgb(255, 220, 20, 60)), // Red
        LLMProvider.Ollama => new SolidColorBrush(Color.FromArgb(255, 30, 144, 255)), // Blue
        _ => new SolidColorBrush(Colors.Gray)
    };

    public static Color GetColor(LLMProvider provider) => provider switch
    {
        LLMProvider.DeepSeek => Color.FromArgb(255, 255, 140, 0), // Orange
        LLMProvider.OpenAI => Color.FromArgb(255, 16, 163, 127), // Green
        LLMProvider.Claude => Color.FromArgb(255, 138, 43, 226), // Purple
        LLMProvider.Groq => Color.FromArgb(255, 220, 20, 60), // Red
        LLMProvider.Ollama => Color.FromArgb(255, 30, 144, 255), // Blue
        _ => Colors.Gray
    };
}
