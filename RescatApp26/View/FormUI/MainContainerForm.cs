using RescatApp26.Data;
using RescatApp26.Models.DTOs;
using RescatApp26.Models.Enums;
using RescatApp26.Services.Interfaz_service;
using RescatApp26.Services.Repository;

namespace RescatApp26.View.FormUI;

public class MainContainerForm : Form
{
    private readonly AuthenticatedUserDto _user;
    private readonly RescatAppDbContext _context;
    private readonly IAuthenticationService _authService;
    private readonly IConnectivityService _connectivity;
    private readonly ISyncQueue _queue;
    private readonly Panel _pnlContent;
    private readonly Label _lblStatus;
    private readonly System.Windows.Forms.Timer _statusTimer;
    private Form? _currentForm;
    private bool _checking;

    public bool LogoutRequested { get; private set; }

    public MainContainerForm(AuthenticatedUserDto user, RescatAppDbContext context, IAuthenticationService authService,
        IConnectivityService connectivity, ISyncQueue queue)
    {
        _user = user ?? throw new ArgumentNullException(nameof(user));
        _context = context ?? throw new ArgumentNullException(nameof(context));
        _authService = authService ?? throw new ArgumentNullException(nameof(authService));
        _connectivity = connectivity ?? throw new ArgumentNullException(nameof(connectivity));
        _queue = queue ?? throw new ArgumentNullException(nameof(queue));

        Text = "RescatApp - Rescate y adopción de mascotas";
        ClientSize = new Size(1020, 640);
        MinimumSize = new Size(1020, 640);
        StartPosition = FormStartPosition.CenterScreen;
        BackColor = Color.White;
        UiBuilder.ApplyIcon(this);

        var pnlMenu = new Panel { Dock = DockStyle.Left, Width = 220, BackColor = UiBuilder.Cream };
        _pnlContent = new Panel { Dock = DockStyle.Fill, BackColor = Color.White };
        Controls.Add(_pnlContent);
        Controls.Add(pnlMenu);

        UiBuilder.AddLogo(pnlMenu, 60, 10, 100);
        pnlMenu.Controls.Add(new Label
        {
            Text = "RescatApp",
            Left = 0,
            Top = 112,
            Width = 220,
            Height = 30,
            TextAlign = ContentAlignment.MiddleCenter,
            ForeColor = UiBuilder.Primary,
            Font = new Font("Segoe UI", 15F, FontStyle.Bold)
        });
        pnlMenu.Controls.Add(new Label
        {
            Text = $"Sesión: {_user.UserName}\n({_user.RoleText})",
            Left = 10,
            Top = 144,
            Width = 200,
            Height = 40,
            TextAlign = ContentAlignment.MiddleCenter,
            ForeColor = UiBuilder.Primary
        });

        // Qué ve cada rol (los permisos están en AppPermissions)
        int role = _user.Role;
        bool staff = AppPermissions.CanUseStaffModules(role);
        int y = 192;

        if (staff)
        {
            AddMenuButton(pnlMenu, "Resumen", y, (s, e) =>
                OpenModule(() => new DashboardForm(new DashboardRepository(_context))));
            y += 44;
        }

        AddMenuButton(pnlMenu, "Mascotas", y, (s, e) =>
            OpenModule(() => new PetsForm(
                new OfflinePetRepository(new PetRepository(_context), _connectivity, _queue),
                AppPermissions.CanEdit(role), AppPermissions.CanDelete(role))));
        y += 44;

        if (staff)
        {
            AddMenuButton(pnlMenu, "Adoptantes", y, (s, e) =>
                OpenModule(() => new AdoptersForm(new AdopterRepository(_context), AppPermissions.CanDelete(role))));
            y += 44;
            AddMenuButton(pnlMenu, "Adopciones", y, (s, e) =>
                OpenModule(() => new AdoptionsForm(new AdoptionRepository(_context), new AdopterRepository(_context))));
            y += 44;
            AddMenuButton(pnlMenu, "Citas", y, (s, e) =>
                OpenModule(() => new CitasForm(new CitaRepository(_context), new AdopterRepository(_context))));
            y += 44;
            AddMenuButton(pnlMenu, "Cola de sincronización", y, (s, e) =>
                OpenModule(() => new QueueForm(_queue)));
            y += 44;
        }

        if (AppPermissions.CanManageUsers(role))
        {
            AddMenuButton(pnlMenu, "Nuevo usuario", y, (s, e) =>
            {
                try
                {
                    using var registerForm = new UserRegisterForm(_authService, requestedByAdmin: true);
                    registerForm.ShowDialog(this);
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.ToString(), "Error al abrir el registro");
                }
            });
            y += 44;
        }

        // Estado de la conexión (abajo, encima de "Cerrar sesión")
        var pnlStatus = new Panel { Dock = DockStyle.Bottom, Height = 85, BackColor = UiBuilder.Cream };
        _lblStatus = new Label
        {
            Text = "● Comprobando...",
            Left = 10,
            Top = 6,
            Width = 200,
            Height = 42,
            Font = new Font("Segoe UI", 9.5F, FontStyle.Bold),
            ForeColor = Color.Gray
        };
        var chkOffline = new CheckBox
        {
            Text = "Simular sin conexión",
            Left = 10,
            Top = 54,
            Width = 200,
            ForeColor = UiBuilder.Primary,
            Cursor = Cursors.Hand
        };
        chkOffline.CheckedChanged += (s, e) =>
        {
            _connectivity.SimulateOffline = chkOffline.Checked;
            UpdateStatusAsync();
        };
        pnlStatus.Controls.Add(_lblStatus);
        pnlStatus.Controls.Add(chkOffline);
        pnlMenu.Controls.Add(pnlStatus);

        var btnLogout = AddMenuButton(pnlMenu, "Cerrar sesión", 0, (s, e) =>
        {
            LogoutRequested = true;
            Close();
        });
        btnLogout.Dock = DockStyle.Bottom;
        btnLogout.Height = 50;

        _statusTimer = new System.Windows.Forms.Timer { Interval = 4000 };
        _statusTimer.Tick += (s, e) => UpdateStatusAsync();
        _statusTimer.Start();
        UpdateStatusAsync();

        ShowWelcome();
    }

    private static Button AddMenuButton(Panel parent, string text, int y, EventHandler onClick)
    {
        var button = new Button
        {
            Text = text,
            Left = 0,
            Top = y,
            Width = 220,
            Height = 40,
            FlatStyle = FlatStyle.Flat,
            ForeColor = UiBuilder.Primary,
            BackColor = UiBuilder.Cream,
            Font = new Font("Segoe UI", 11F, FontStyle.Bold),
            Cursor = Cursors.Hand
        };
        button.FlatAppearance.BorderSize = 0;
        button.FlatAppearance.MouseOverBackColor = UiBuilder.Highlight;
        button.Click += onClick;
        parent.Controls.Add(button);
        return button;
    }

    // La comprobación de conexión se hace en segundo plano para que la ventana nunca se congele.
    private async void UpdateStatusAsync()
    {
        if (_checking || IsDisposed) return;
        _checking = true;
        try
        {
            bool online = await Task.Run(() => _connectivity.IsOnline());
            int pending = await Task.Run(() => _queue.PendingCount());
            if (IsDisposed) return;

            string state = online ? "● En línea" : _connectivity.SimulateOffline ? "● Sin conexión (simulada)" : "● Sin conexión";
            _lblStatus.ForeColor = online ? Color.SeaGreen : UiBuilder.Danger;
            _lblStatus.Text = $"{state}\nPendientes: {pending}";
        }
        catch
        {
            // El indicador nunca debe tumbar la aplicación.
        }
        finally
        {
            _checking = false;
        }
    }

    protected override void OnFormClosed(FormClosedEventArgs e)
    {
        _statusTimer.Stop();
        _statusTimer.Dispose();
        base.OnFormClosed(e);
    }

    private void ShowWelcome()
    {
        ClearCurrentForm();
        var lblWelcome = new Label
        {
            Text = $"Bienvenido/a, {_user.UserName}\n\nSeleccione una opción del menú para comenzar.",
            Dock = DockStyle.Fill,
            TextAlign = ContentAlignment.TopCenter,
            Font = new Font("Segoe UI", 14F),
            ForeColor = UiBuilder.Primary
        };
        _pnlContent.Controls.Add(lblWelcome);

        var logo = new PictureBox
        {
            Dock = DockStyle.Top,
            Height = 300,
            SizeMode = PictureBoxSizeMode.Zoom,
            Image = UiBuilder.Logo
        };
        _pnlContent.Controls.Add(logo);
    }

    private void OpenModule(Func<Form> factory)
    {
        try
        {
            LoadFormIntoPanel(factory());
        }
        catch (Exception ex)
        {
            MessageBox.Show($"No fue posible abrir el módulo.\n{ex.Message}", "RescatApp",
                MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private void ClearCurrentForm()
    {
        if (_currentForm is { IsDisposed: false })
        {
            _currentForm.Close();
            _currentForm.Dispose();
        }
        _currentForm = null;
        _pnlContent.Controls.Clear();
    }

    private void LoadFormIntoPanel(Form form)
    {
        ClearCurrentForm();
        form.TopLevel = false;
        form.FormBorderStyle = FormBorderStyle.None;
        form.Dock = DockStyle.Fill;
        _pnlContent.Controls.Add(form);
        _currentForm = form;
        form.Show();
    }
}