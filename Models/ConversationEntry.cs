namespace ComputerController.Models;

public record ConversationEntry(
    string UserRequest,
    string GeneratedCode,
    string? ExecutionResult,
    DateTime Timestamp
);
