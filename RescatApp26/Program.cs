using RescatApp26.Data;
using RescatApp26.Models.DTOs;
using RescatApp26.Services;
using RescatApp26.Services.Interfaz_service;
using RescatApp26.Services.Repository;
using RescatApp26.View.FormUI;

namespace RescatApp26;

internal static class Program
{
    [STAThread]
    private static void Main()
    {
        ApplicationConfiguration.Initialize();

        try
        {
            while (true)
            {
                using var context = new RescatAppDbContext();
                IPasswordHasher passwordHasher = new PasswordHasher();
                IUserRepository userRepository = new UserRepository(context);
                IAuthenticationService authService = new AuthenticationService(userRepository, passwordHasher);

                AuthenticatedUserDto? user;
                using (var login = new LoginForm(authService))
                {
                    if (login.ShowDialog() != DialogResult.OK) return;
                    user = login.AuthenticatedUser;
                }
                if (user is null) return;

                var main = new MainContainerForm(user, context, authService);
                Application.Run(main);

                // Si cerró sesión se vuelve al login; si cerró la ventana, termina la aplicación.
                if (!main.LogoutRequested) return;
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Error inesperado:\n{ex.Message}", "RescatApp",
                MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }
}