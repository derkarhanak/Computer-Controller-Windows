using Windows.System;

namespace ComputerController.Helpers;

public static class FileHelper
{
    public static async Task OpenDirectoryAsync(string path)
    {
        try
        {
            await Launcher.LaunchFolderPathAsync(path);
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Failed to open directory: {ex.Message}");
        }
    }

    public static string GetDownloadsPath() => 
        Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), "Downloads");

    public static string GetDesktopPath() => 
        Environment.GetFolderPath(Environment.SpecialFolder.Desktop);

    public static string GetDocumentsPath() => 
        Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);

    public static string GetPicturesPath() => 
        Environment.GetFolderPath(Environment.SpecialFolder.MyPictures);
}
