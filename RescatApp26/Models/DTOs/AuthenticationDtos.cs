namespace RescatApp26.Models.DTOs;

public class AuthenticationRequestDto
{
    public string UserName { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
}

public class AuthenticatedUserDto
{
    public int UserId { get; set; }
    public string UserName { get; set; } = string.Empty;
}

public class AuthenticationResultDto
{
    public bool IsAuthenticated { get; set; }
    public string Message { get; set; } = string.Empty;
    public AuthenticatedUserDto? User { get; set; }

    public static AuthenticationResultDto Success(AuthenticatedUserDto user) =>
        new() { IsAuthenticated = true, User = user, Message = "Autenticación correcta." };

    public static AuthenticationResultDto Failure(string message) =>
        new() { IsAuthenticated = false, Message = message };
}

public class UserAuthenticationDataDto
{
    public int UserId { get; set; }
    public string UserName { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
}