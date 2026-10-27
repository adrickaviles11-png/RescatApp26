using RescatApp26.Data;
using RescatApp26.Models.DTOs;
using RescatApp26.Services.Interfaz_service;
using RescatApp26.Services.Repository;

namespace RescatApp26.View.FormUI;

public class MainContainerForm : Form
{
    private readonly AuthenticatedUserDto _user;
    private readonly RescatAppDbContext _context;
    private readonly IAuthenticationService _authService;
    private readonly Panel _pnlContent;
    private Form? _currentForm;

    public bool LogoutRequested { get; private set; }

    public MainContainerForm(AuthenticatedUserDto user, RescatAppDbContext context, IAuthenticationService authService)
    {
        _user = user ?? throw new ArgumentNullException(nameof(user));
        _context = context ?? throw new ArgumentNullException(nameof(context));
        _authService = authService ?? throw new ArgumentNullException(nameof(authService));

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

        UiBuilder.AddLogo(pnlMenu, 50, 15, 120);
        var lblBrand = new Label
        {
            Text = "RescatApp",
            Left = 0,
            Top = 140,
            Width = 220,
            Height = 34,
            TextAlign = ContentAlignment.MiddleCenter,
            ForeColor = UiBuilder.Primary,
            Font = new Font("Segoe UI", 16F, FontStyle.Bold)
        };
        var lblUser = new Label
        {
            Text = $"Sesión: {_user.UserName}",
            Left = 10,
            Top = 176,
            Width = 200,
            Height = 26,
            TextAlign = ContentAlignment.MiddleCenter,
            ForeColor = UiBuilder.Primary
        };
        pnlMenu.Controls.Add(lblBrand);
        pnlMenu.Controls.Add(lblUser);

        int y = 225;
        AddMenuButton(pnlMenu, "Resumen", y, (s, e) =>
            OpenModule(() => new DashboardForm(new DashboardRepository(_context))));
        y += 50;
        AddMenuButton(pnlMenu, "Mascotas", y, (s, e) =>
            OpenModule(() => new PetsForm(new PetRepository(_context))));
        y += 50;
        AddMenuButton(pnlMenu, "Adoptantes", y, (s, e) =>
            OpenModule(() => new AdoptersForm(new AdopterRepository(_context))));
        y += 50;
        AddMenuButton(pnlMenu, "Adopciones", y, (s, e) =>
           OpenModule(() => new AdoptionsForm(new AdoptionRepository(_context), new AdopterRepository(_context))));
        y += 50;
        AddMenuButton(pnlMenu, "Citas", y, (s, e) =>
           OpenModule(() => new CitasForm(new CitaRepository(_context), new AdopterRepository(_context))));
        y += 50;
        AddMenuButton(pnlMenu, "Nuevo usuario", y, (s, e) =>
        {
            using var registerForm = new UserRegisterForm(_authService);
            registerForm.ShowDialog(this);
        });

        var btnLogout = AddMenuButton(pnlMenu, "Cerrar sesión", 0, (s, e) =>
        {
            LogoutRequested = true;
            Close();
        });
        btnLogout.Dock = DockStyle.Bottom;
        btnLogout.Height = 50;

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
            Height = 44,
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