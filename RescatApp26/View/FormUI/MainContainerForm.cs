using RescatApp26.Data;
using RescatApp26.Models.DTOs;
using RescatApp26.Models.Enums;
using RescatApp26.Services;
using RescatApp26.Services.Interfaz_service;
using RescatApp26.Services.Repository;
using RescatApp26.View.Interfaz;

namespace RescatApp26.View.FormUI;

public class MainContainerForm : Form
{
    private readonly AuthenticatedUserDto _user;
    private readonly RescatAppDbContext _context;
    private readonly IAuthenticationService _authService;
    private readonly IConnectivityService _connectivity;
    private readonly ISyncQueue _queue;
    private readonly SyncService _sync;
    private readonly Panel _pnlContent;
    private readonly Label _lblStatus;
    private readonly System.Windows.Forms.Timer _statusTimer;
    private Form? _currentForm;

    public bool LogoutRequested { get; private set; }

    public MainContainerForm(AuthenticatedUserDto user, RescatAppDbContext context, IAuthenticationService authService,
        IConnectivityService connectivity, ISyncQueue queue, SyncService sync)
    {
        _user = user ?? throw new ArgumentNullException(nameof(user));
        _context = context ?? throw new ArgumentNullException(nameof(context));
        _authService = authService ?? throw new ArgumentNullException(nameof(authService));
        _connectivity = connectivity ?? throw new ArgumentNullException(nameof(connectivity));
        _queue = queue ?? throw new ArgumentNullException(nameof(queue));
        _sync = sync ?? throw new ArgumentNullException(nameof(sync));

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
        var pnlStatus = new Panel { Dock = DockStyle.Bottom, Height = 60, BackColor = UiBuilder.Cream };
        _lblStatus = new Label
        {
            Text = "● Comprobando...",
            Left = 10,
            Top = 8,
            Width = 200,
            Height = 44,
            Font = new Font("Segoe UI", 9.5F, FontStyle.Bold),
            ForeColor = Color.Gray
        };
        pnlStatus.Controls.Add(_lblStatus);
        pnlMenu.Controls.Add(pnlStatus);

        var btnLogout = AddMenuButton(pnlMenu, "Cerrar sesión", 0, (s, e) =>
        {
            LogoutRequested = true;
            Close();
        });
        btnLogout.Dock = DockStyle.Bottom;
        btnLogout.Height = 50;

        // Todo es automático: los cambios de conexión y de la cola actualizan la pantalla solos.
        _connectivity.StatusChanged += OnConnectivityChanged;
        _sync.QueueChanged += OnQueueChanged;

        _statusTimer = new System.Windows.Forms.Timer { Interval = 2000 };
        _statusTimer.Tick += (s, e) => UpdateStatus();
        _statusTimer.Start();
        UpdateStatus();

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

    private void UpdateStatus()
    {
        if (IsDisposed) return;

        bool online = _connectivity.IsOnline;
        int pending;
        try
        {
            pending = _queue.PendingCount();
        }
        catch
        {
            pending = 0;
        }

        _lblStatus.ForeColor = online ? Color.SeaGreen : UiBuilder.Danger;
        _lblStatus.Text = (online ? "● En línea" : "● Sin conexión") + $"\nPendientes: {pending}";
    }

    // Estos eventos llegan desde hilos de segundo plano, así que se pasan al hilo de la pantalla.
    private void OnConnectivityChanged(object? sender, bool online) => RunOnUi(UpdateStatus);

    private void OnQueueChanged(object? sender, EventArgs e) => RunOnUi(() =>
    {
        UpdateStatus();
        if (_currentForm is IRefreshable refreshable) refreshable.RefreshData();
    });

    private void RunOnUi(Action action)
    {
        if (IsDisposed || !IsHandleCreated) return;
        try
        {
            BeginInvoke(action);
        }
        catch (InvalidOperationException)
        {
            // La ventana se está cerrando.
        }
    }

    protected override void OnFormClosed(FormClosedEventArgs e)
    {
        _connectivity.StatusChanged -= OnConnectivityChanged;
        _sync.QueueChanged -= OnQueueChanged;
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