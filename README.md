# Computer Controller for Windows

A powerful, AI-driven application that allows you to control your Windows computer using natural language commands. Built with WinUI 3 and .NET 8, this app leverages Large Language Models (LLMs) like Groq and Ollama to generate and execute secure Python scripts for automating tasks.

![App Logo](Assets/app_logo.png)

## 🚀 Features

-   **Natural Language Control**: Describe what you want to do in plain English (e.g., "Organize my downloads folder by file type", "Create a backup of my documents").
-   **AI-Powered Code Generation**: Uses state-of-the-art LLMs to translate your requests into executable Python code.
-   **Secure Execution**: All generated code runs in a sandboxed environment with strict safety checks.
    -   Prevention of dangerous commands (e.g., system formatting).
    -   Explicit permission required for file system modifications.
    -   "View Code" feature to inspect generated scripts before execution.
-   **Multi-Provider Support**: Choose between different LLM providers:
    -   **Groq**: Fast, cloud-based inference (Requires API Key).
    -   **Ollama**: Local, private inference (Requires Ollama installation).
-   **Modern WinUI 3 Interface**:
    -   Sleek, responsive design that looks great on Windows 11.
    -   Dark and Light mode support.
    -   History tracking of previous commands.
    -   "Favorites" system for quick access to frequent tasks.

## 🛠️ Tech Stack

-   **Framework**: WinUI 3 (Windows App SDK)
-   **Language**: C# (.NET 8)
-   **Architecture**: MVVM (Model-View-ViewModel) using CommunityToolkit.Mvvm
-   **Scripting Engine**: Python (Standard Library)
-   **AI Integration**: Custom services for Groq and Ollama APIs

## 📦 Installation & Setup

### Prerequisites

1.  **Windows 10 (version 1809 or later) or Windows 11**.
2.  **Python**: Ensure Python is installed and added to your system PATH.
    -   Open Command Prompt and type `python --version` to verify.
3.  **App Dependencies**: The app is self-contained, but ensure you have the [Windows App SDK Runtime](https://learn.microsoft.com/en-us/windows/apps/windows-app-sdk/downloads) if you encounter issues.

### Running the App

1.  Download the latest release from the [Releases](https://github.com/derkarhanak/Computer-Controller-Windows/releases) page.
2.  Extract the ZIP file to a folder of your choice.
3.  Run `ComputerController.exe`.

### Configuration

1.  **Select AI Provider**:
    -   Go to the **Settings** tab.
    -   Choose between **Groq** (Cloud) or **Ollama** (Local).
2.  **Groq Setup**:
    -   Get a free API key from [console.groq.com](https://console.groq.com).
    -   Enter the API key in the app settings.
    -   Select a model (e.g., `llama3-70b-8192` or `mixtral-8x7b-32768`).
3.  **Ollama Setup**:
    -   Install [Ollama](https://ollama.com/) on your machine.
    -   Pull a model (e.g., `ollama pull llama3`).
    -   Enter your local endpoint (default: `http://localhost:11434`) and model name in the app settings.

## 💡 Usage Examples

Type these commands into the "Operation Plan" box:

-   **File Management**:
    -   "Move all screenshots from Desktop to the Pictures folder."
    -   "Find all duplicate files in Documents/ProjectX."
    -   "Rename all .txt files to .md in the current folder."
-   **System Info**:
    -   "List the top 5 largest files in my Downloads folder."
    -   "Show me the free space on C: drive."
-   **Productivity**:
    -   "Create a folder named 'Report_2024' and add a text file 'notes.txt' inside it."

## 🔒 Security

This application executes Python code on your machine. While we implement strict prompt engineering and validation to ensure the AI generates safe code, **always review the generated code** using the "View Code" button before execution, especially for destructive operations like deleting files.

## 📄 License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.
