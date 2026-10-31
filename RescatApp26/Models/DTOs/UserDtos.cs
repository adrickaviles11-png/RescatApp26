using RescatApp26.Models.Enums;

namespace RescatApp26.Models.DTOs;

public class UserRegisterDto
{
    public string UserName { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public string ConfirmPassword { get; set; } = string.Empty;
    public int Role { get; set; } = (int)UserRole.Adoptante;
}

public class UserCreateDto
{
    public string UserName { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public int Role { get; set; }
}

public class OperationResultDto
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;

    public static OperationResultDto Ok(string message) => new() { Success = true, Message = message };
    public static OperationResultDto Fail(string message) => new() { Success = false, Message = message };
}


