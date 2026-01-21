using System.Diagnostics;
using System.Text;

namespace ComputerController.Services;

public class PythonExecutor
{
    private string? _pythonPath;

    public PythonExecutor()
    {
        _pythonPath = FindPythonPath();
    }

    public async Task<string> ExecuteAsync(string code, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrEmpty(_pythonPath))
        {
            return "Error: Python is not installed or could not be found. Please install Python 3 from python.org";
        }

        // Create a temporary Python file
        var tempDir = Path.GetTempPath();
        var tempFile = Path.Combine(tempDir, $"temp_script_{Guid.NewGuid()}.py");

        try
        {
            // Write the code to the temporary file
            await File.WriteAllTextAsync(tempFile, code, Encoding.UTF8, cancellationToken);

            // Create the process
            var processStartInfo = new ProcessStartInfo
            {
                FileName = _pythonPath,
                Arguments = $"\"{tempFile}\"",
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true,
                WorkingDirectory = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile)
            };

            using var process = new Process { StartInfo = processStartInfo };
            
            var outputBuilder = new StringBuilder();
            var errorBuilder = new StringBuilder();

            process.OutputDataReceived += (sender, e) =>
            {
                if (!string.IsNullOrEmpty(e.Data))
                {
                    outputBuilder.AppendLine(e.Data);
                }
            };

            process.ErrorDataReceived += (sender, e) =>
            {
                if (!string.IsNullOrEmpty(e.Data))
                {
                    errorBuilder.AppendLine(e.Data);
                }
            };

            process.Start();
            process.BeginOutputReadLine();
            process.BeginErrorReadLine();

            // Wait for the process to exit with timeout
            var timeout = TimeSpan.FromSeconds(120);
            var completed = await Task.Run(() => process.WaitForExit((int)timeout.TotalMilliseconds), cancellationToken);

            if (!completed)
            {
                try
                {
                    process.Kill();
                }
                catch { }
                return "Error: Python script execution timed out (120 seconds)";
            }

            var output = outputBuilder.ToString().Trim();
            var error = errorBuilder.ToString().Trim();

            if (process.ExitCode == 0)
            {
                return string.IsNullOrEmpty(output) ? "Operation completed successfully" : output;
            }
            else
            {
                return $"Error: {(string.IsNullOrEmpty(error) ? $"Process failed with exit code {process.ExitCode}" : error)}";
            }
        }
        catch (Exception ex)
        {
            return $"Error: {ex.Message}";
        }
        finally
        {
            // Clean up the temporary file
            try
            {
                if (File.Exists(tempFile))
                {
                    File.Delete(tempFile);
                }
            }
            catch { }
        }
    }

    public bool ValidateCode(string code)
    {
        // Basic validation - check for potentially dangerous operations
        var dangerousPatterns = new[]
        {
            "import subprocess",
            "import os.system",
            "eval(",
            "exec(",
            "__import__",
            "globals(",
            "locals(",
            "compile("
        };

        var lowercasedCode = code.ToLowerInvariant();

        foreach (var pattern in dangerousPatterns)
        {
            if (lowercasedCode.Contains(pattern.ToLowerInvariant()))
            {
                return false;
            }
        }

        // Check if it's a valid Python file operation
        var safePatterns = new[]
        {
            "import os",
            "import shutil",
            "import pathlib",
            "import glob",
            "os.path",
            "shutil.",
            "pathlib.",
            "glob.glob",
            "os.makedirs",
            "os.remove",
            "os.rename",
            "shutil.move",
            "shutil.copy",
            "shutil.rmtree"
        };

        foreach (var pattern in safePatterns)
        {
            if (lowercasedCode.Contains(pattern.ToLowerInvariant()))
            {
                return true;
            }
        }

        return false;
    }

    private string? FindPythonPath()
    {
        // Try common Python executable names
        var pythonCommands = new[] { "python", "python3", "py" };

        foreach (var command in pythonCommands)
        {
            try
            {
                var processStartInfo = new ProcessStartInfo
                {
                    FileName = command,
                    Arguments = "--version",
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                };

                using var process = Process.Start(processStartInfo);
                if (process != null)
                {
                    process.WaitForExit(3000);
                    if (process.ExitCode == 0)
                    {
                        return command;
                    }
                }
            }
            catch
            {
                // Continue to next command
            }
        }

        return null;
    }

    public string? GetPythonPath() => _pythonPath;
}
