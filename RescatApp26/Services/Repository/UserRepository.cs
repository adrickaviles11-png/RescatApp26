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
                    PasswordHash = u.ContrasenaHash
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
                Activo = true
            };
            _context.Usuarios.Add(entity);
            _context.SaveChanges();
            return entity.Id;
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException("Error al registrar el usuario.", ex);
        }
        finally
        {
            _context.ChangeTracker.Clear();
        }
    }
}