using System.Text.RegularExpressions;
using RescatApp26.Models.DTOs;
using RescatApp26.Models.Enums;
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
            UserName = data.UserName,
            Role = data.Role
        });
    }

    public IReadOnlyList<OptionItem> GetAvailableRoles(bool requestedByAdmin)
    {
        // Si todavía no hay ningún usuario, el primero debe ser Administrador.
        if (!_userRepository.AnyUsers())
            return UserRoleOptions.All.Where(o => o.Value == (int)UserRole.Administrador).ToList();

        // Desde el inicio de sesión solo se pueden crear cuentas de Adoptante.
        if (!requestedByAdmin)
            return UserRoleOptions.All.Where(o => o.Value == (int)UserRole.Adoptante).ToList();

        return UserRoleOptions.All;
    }

    public OperationResultDto Register(UserRegisterDto request, bool requestedByAdmin)
    {
        if (request is null) throw new ArgumentNullException(nameof(request));

        string userName = request.UserName?.Trim() ?? string.Empty;
        string answer = NormalizeAnswer(request.SecurityAnswer);

        if (!UserNameRegex.IsMatch(userName))
            return OperationResultDto.Fail("El usuario debe tener entre 3 y 50 caracteres (letras, números, punto o guion bajo).");
        if ((request.Password ?? string.Empty).Length < MinPasswordLength)
            return OperationResultDto.Fail($"La contraseña debe tener al menos {MinPasswordLength} caracteres.");
        if (request.Password != request.ConfirmPassword)
            return OperationResultDto.Fail("Las contraseñas no coinciden.");
        if (!GetAvailableRoles(requestedByAdmin).Any(o => o.Value == request.Role))
            return OperationResultDto.Fail("El rol seleccionado no está permitido.");
        if (!SecurityQuestions.All.Contains(request.SecurityQuestion))
            return OperationResultDto.Fail("Seleccione una pregunta de seguridad.");
        if (answer.Length < 2)
            return OperationResultDto.Fail("Escriba la respuesta de seguridad (mínimo 2 caracteres).");
        if (_userRepository.ExistsUserName(userName))
            return OperationResultDto.Fail("Ese nombre de usuario ya está en uso.");

        _userRepository.CreateUser(new UserCreateDto
        {
            UserName = userName,
            PasswordHash = _passwordHasher.HashPassword(request.Password!),
            Role = request.Role,
            SecurityQuestion = request.SecurityQuestion,
            AnswerHash = _passwordHasher.HashPassword(answer)
        });

        return OperationResultDto.Ok("Usuario registrado correctamente.");
    }

    public string? GetSecurityQuestion(string userName)
    {
        var data = _userRepository.GetRecoveryData(userName?.Trim() ?? string.Empty);
        return string.IsNullOrEmpty(data?.Question) || string.IsNullOrEmpty(data.AnswerHash) ? null : data.Question;
    }

    public OperationResultDto ResetPassword(string userName, string answer, string newPassword, string confirmPassword)
    {
        var data = _userRepository.GetRecoveryData(userName?.Trim() ?? string.Empty);
        if (data is null || string.IsNullOrEmpty(data.AnswerHash))
            return OperationResultDto.Fail("No fue posible restablecer la contraseña de esta cuenta.");

        // Primero se comprueba la respuesta; solo si es correcta se acepta la contraseña nueva.
        if (!_passwordHasher.VerifyPassword(NormalizeAnswer(answer), data.AnswerHash))
            return OperationResultDto.Fail("La respuesta de seguridad no es correcta.");
        if ((newPassword ?? string.Empty).Length < MinPasswordLength)
            return OperationResultDto.Fail($"La contraseña debe tener al menos {MinPasswordLength} caracteres.");
        if (newPassword != confirmPassword)
            return OperationResultDto.Fail("Las contraseñas no coinciden.");

        _userRepository.UpdatePasswordHash(data.UserId, _passwordHasher.HashPassword(newPassword));
        return OperationResultDto.Ok("Contraseña restablecida. Ya puede iniciar sesión con la nueva.");
    }

    // Las respuestas no distinguen mayúsculas ni espacios al inicio y al final.
    private static string NormalizeAnswer(string? answer) => (answer ?? string.Empty).Trim().ToLowerInvariant();
}