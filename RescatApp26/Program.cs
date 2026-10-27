using RescatApp26.Data;
using RescatApp26.Services;
using RescatApp26.Services.Interfaz_service;
using RescatApp26.Services.Repository;
using RescatApp26.View.FormUI;
using RescatApp26.Services.Repository;

namespace RescatApp26;

internal static class Program
{
    [STAThread]
    private static void Main()
    {
        ApplicationConfiguration.Initialize();

        try
        {
            using var context = new RescatAppDbContext();
            IPasswordHasher passwordHasher = new PasswordHasher();
            IUserRepository userRepository = new UserRepository(context);
            IAuthenticationService authService = new AuthenticationService(userRepository, passwordHasher);

            using var login = new LoginForm(authService);
            if (login.ShowDialog() != DialogResult.OK) return;
            MessageBox.Show($"Bienvenido/a {login.AuthenticatedUser?.UserName}");
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Error inesperado:\n{ex.Message}", "RescatApp",
                MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }
}