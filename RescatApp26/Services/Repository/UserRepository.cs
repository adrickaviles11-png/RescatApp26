using Microsoft.EntityFrameworkCore;
using RescatApp26.Data;
using RescatApp26.Models.DTOs;
using RescatApp26.Models.Entities;
using RescatApp26.Services.Interfaz_service;

namespace RescatApp26.Services.Repository;

public class UserRepository : IUserRepository
{
    private const string ConnectionError = "No fue posible acceder a los usuarios. Verifique la conexión a la base de datos.";
    private readonly RescatAppDbContext _context;

    public UserRepository(RescatAppDbContext context)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
    }

    public UserAuthenticationDataDto? GetUserAuthenticationData(string userName)
    {
        try
        {
            return _context.Usuarios.AsNoTracking()
                .Where(u => u.NombreUsuario == userName && u.Activo)
                .Select(u => new UserAuthenticationDataDto
                {
                    UserId = u.Id,
                    UserName = u.NombreUsuario,
                    PasswordHash = u.ContrasenaHash,
                    Role = u.Rol
                })
                .FirstOrDefault();
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException(ConnectionError, ex);
        }
    }

    public bool ExistsUserName(string userName)
    {
        try
        {
            return _context.Usuarios.AsNoTracking().Any(u => u.NombreUsuario == userName);
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException(ConnectionError, ex);
        }
    }

    public int CreateUser(UserCreateDto user)
    {
        if (user is null) throw new ArgumentNullException(nameof(user));

        try
        {
            var entity = new Usuario
            {
                NombreUsuario = user.UserName,
                ContrasenaHash = user.PasswordHash,
                Activo = true,
                Rol = user.Role,
                PreguntaSeguridad = user.SecurityQuestion,
                RespuestaHash =user.AnswerHash
            };
            _context.Usuarios.Add(entity);
            _context.SaveChanges();
            return entity.Id;
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException("Error al registrar el usuario: " + (ex.InnerException?.Message ?? ex.Message), ex);
        }
        finally
        {
            _context.ChangeTracker.Clear();
        }
    }


    public bool AnyUsers()
    {
        try
        {
            return _context.Usuarios.AsNoTracking().Any();
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException(ConnectionError, ex);
        }
    }

    public UserRecoveryDataDto? GetRecoveryData(string userName)
    {
        try
        {
            return _context.Usuarios.AsNoTracking()
                .Where(u => u.NombreUsuario == userName && u.Activo)
                .Select(u => new UserRecoveryDataDto
                {
                    UserId = u.Id,
                    Question = u.PreguntaSeguridad,
                    AnswerHash = u.RespuestaHash
                })
                .FirstOrDefault();
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException(ConnectionError, ex);
        }
    }

    public void UpdatePasswordHash(int userId, string passwordHash)
    {
        try
        {
            _context.Usuarios.Where(u => u.Id == userId)
                .ExecuteUpdate(s => s.SetProperty(u => u.ContrasenaHash, passwordHash));
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException("No fue posible actualizar la contraseña.", ex);
        }
    }
}