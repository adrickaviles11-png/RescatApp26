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
            LocalDbContext.EnsureReady();   // crea rescatapp_local.db la primera vez

            using var connectivity = new ConnectivityService();
            connectivity.Start();           // empieza a vigilar la conexión con SQL Server

            ISyncQueue queue = new SqliteSyncQueue();
            using var sync = new SyncService(queue, connectivity);
            sync.Start();                   // envía la cola apenas vuelve la conexión

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

                var main = new MainContainerForm(user, context, authService, connectivity, queue, sync);
                Application.Run(main);

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