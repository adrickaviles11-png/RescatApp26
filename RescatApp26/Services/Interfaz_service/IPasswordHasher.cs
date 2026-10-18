namespace RescatApp26.Services.Interfaz_service;

public interface IPasswordHasher
{
    string HashPassword(string password);
    bool VerifyPassword(string password, string storedHash);
}