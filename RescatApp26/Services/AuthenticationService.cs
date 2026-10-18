using System.Text.RegularExpressions;
using RescatApp26.Models.DTOs;
using RescatApp26.Services.Interfaz_service;

namespace RescatApp26.Services;

public class AuthenticationService : IAuthenticationService
{
    private static readonly Regex UserNameRegex = new(@"^[A-Za-z0-9._]{3,50}$", RegexOptions.Compiled);
    private const int MinPasswordLength = 6;

    private readonly IUserRepository _userRepository;
    private readonly IPasswordHasher _passwordHasher;

    public AuthenticationService(IUserRepository userRepository, IPasswordHasher passwordHasher)
    {
        _userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
        _passwordHasher = passwordHasher ?? throw new ArgumentNullException(nameof(passwordHasher));
    }

    public AuthenticationResultDto Authenticate(AuthenticationRequestDto request)
    {
        if (request is null) throw new ArgumentNullException(nameof(request));

        string userName = request.UserName?.Trim() ?? string.Empty;
        if (userName.Length == 0 || string.IsNullOrEmpty(request.Password))
            return AuthenticationResultDto.Failure("Ingrese usuario y contraseña.");

        var data = _userRepository.GetUserAuthenticationData(userName);
        if (data is null || !_passwordHasher.VerifyPassword(request.Password, data.PasswordHash))
            return AuthenticationResultDto.Failure("Usuario o contraseña incorrectos.");

        return AuthenticationResultDto.Success(new AuthenticatedUserDto
        {
            UserId = data.UserId,
            UserName = data.UserName
        });
    }

    public OperationResultDto Register(UserRegisterDto request)
    {
        if (request is null) throw new ArgumentNullException(nameof(request));

        string userName = request.UserName?.Trim() ?? string.Empty;

        if (!UserNameRegex.IsMatch(userName))
            return OperationResultDto.Fail("El usuario debe tener entre 3 y 50 caracteres (letras, números, punto o guion bajo).");
        if ((request.Password ?? string.Empty).Length < MinPasswordLength)
            return OperationResultDto.Fail($"La contraseña debe tener al menos {MinPasswordLength} caracteres.");
        if (request.Password != request.ConfirmPassword)
            return OperationResultDto.Fail("Las contraseñas no coinciden.");
        if (_userRepository.ExistsUserName(userName))
            return OperationResultDto.Fail("Ese nombre de usuario ya está en uso.");

        _userRepository.CreateUser(new UserCreateDto
        {
            UserName = userName,
            PasswordHash = _passwordHasher.HashPassword(request.Password!)
        });

        return OperationResultDto.Ok("Usuario registrado correctamente.");
    }
}