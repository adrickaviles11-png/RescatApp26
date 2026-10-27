using RescatApp26.Models.DTOs;
using RescatApp26.Presenter;
using RescatApp26.Services.Interfaz_service;
using RescatApp26.View.Interfaz;

namespace RescatApp26.View.FormUI;

public class AdoptersForm : Form, IAdopterView
{
    private readonly AdopterPresenter _presenter;
    private readonly DataGridView _grid;
    private readonly TextBox _txtFirstNames;
    private readonly TextBox _txtLastNames;
    private readonly TextBox _txtDocumentId;
    private readonly TextBox _txtPhone;
    private readonly TextBox _txtEmail;
    private readonly TextBox _txtCountry;
    private readonly TextBox _txtAddress;
    private int? _currentAdopterId;

    public AdoptersForm(IAdopterRepository repository)
    {
        _presenter = new AdopterPresenter(this, repository);

        Text = "Adoptantes";
        ClientSize = new Size(780, 580);
        StartPosition = FormStartPosition.CenterScreen;
        BackColor = Color.White;
        UiBuilder.AddTitle(this, "Adoptantes", 15, 10);

        _grid = UiBuilder.AddGrid(this, 15, 55, 740, 230);
        UiBuilder.AddColumn(_grid, "Id", "ID", 45);
        UiBuilder.AddColumn(_grid, "FullName", "Nombre", 160);
        UiBuilder.AddColumn(_grid, "DocumentId", "Documento", 105);
        UiBuilder.AddColumn(_grid, "Phone", "Teléfono", 90);
        UiBuilder.AddColumn(_grid, "Email", "Correo", 140);
        UiBuilder.AddColumn(_grid, "Country", "País", 80);
        UiBuilder.AddColumn(_grid, "Address", "Dirección", 150);
        _grid.CellClick += (s, e) =>
        {
            if (e.RowIndex >= 0 && _grid.Rows[e.RowIndex].DataBoundItem is AdopterDto adopter)
                _presenter.SelectAdopter(adopter.Id);
        };

        int y1 = 305, y2 = 340, y3 = 375, y4 = 410;

        UiBuilder.AddLabel(this, "Nombres", 15, y1);
        _txtFirstNames = UiBuilder.AddTextBox(this, 120, y1, 220);
        _txtFirstNames.MaxLength = 100;
        UiBuilder.AddLabel(this, "Apellidos", 380, y1);
        _txtLastNames = UiBuilder.AddTextBox(this, 500, y1, 220);
        _txtLastNames.MaxLength = 100;

        UiBuilder.AddLabel(this, "Documento", 15, y2);
        _txtDocumentId = UiBuilder.AddTextBox(this, 120, y2, 220);
        _txtDocumentId.MaxLength = 30;
        UiBuilder.AddLabel(this, "Teléfono", 380, y2);
        _txtPhone = UiBuilder.AddTextBox(this, 500, y2, 220);
        _txtPhone.MaxLength = 30;

        UiBuilder.AddLabel(this, "Correo", 15, y3);
        _txtEmail = UiBuilder.AddTextBox(this, 120, y3, 220);
        _txtEmail.MaxLength = 100;
        UiBuilder.AddLabel(this, "País de origen", 380, y3);
        _txtCountry = UiBuilder.AddTextBox(this, 500, y3, 220);
        _txtCountry.MaxLength = 50;

        UiBuilder.AddLabel(this, "Dirección", 15, y4);
        _txtAddress = UiBuilder.AddTextBox(this, 120, y4, 600);
        _txtAddress.MaxLength = 200;

        var btnNew = UiBuilder.AddButton(this, "Nuevo", 15, 465);
        btnNew.Click += (s, e) => _presenter.NewAdopter();
        var btnSave = UiBuilder.AddButton(this, "Guardar", 135, 465);
        btnSave.Click += (s, e) => _presenter.SaveAdopter();
        var btnDelete = UiBuilder.AddButton(this, "Eliminar", 255, 465, danger: true);
        btnDelete.Click += (s, e) => _presenter.DeleteAdopter();

        Load += (s, e) => _presenter.LoadAdopters();
    }

    public int? SelectedAdopterId => _currentAdopterId;
    public string FirstNames => _txtFirstNames.Text.Trim();
    public string LastNames => _txtLastNames.Text.Trim();
    public string DocumentId => _txtDocumentId.Text.Trim();
    public string? Phone => NullIfEmpty(_txtPhone.Text);
    public string? Email => NullIfEmpty(_txtEmail.Text);
    public string? Address => NullIfEmpty(_txtAddress.Text);
    public string? Country => NullIfEmpty(_txtCountry.Text);

    private static string? NullIfEmpty(string text) =>
        string.IsNullOrWhiteSpace(text) ? null : text.Trim();

    public void ShowAdopters(IEnumerable<AdopterDto> adopters) => _grid.DataSource = adopters.ToList();

    public void ShowAdopter(AdopterDto adopter)
    {
        _currentAdopterId = adopter.Id;
        _txtFirstNames.Text = adopter.FirstNames;
        _txtLastNames.Text = adopter.LastNames;
        _txtDocumentId.Text = adopter.DocumentId;
        _txtPhone.Text = adopter.Phone ?? string.Empty;
        _txtEmail.Text = adopter.Email ?? string.Empty;
        _txtCountry.Text = adopter.Country ?? string.Empty;
        _txtAddress.Text = adopter.Address ?? string.Empty;
    }

    public void ShowMessage(string message) => UiBuilder.Info(message);
    public void ShowError(string message) => UiBuilder.Error(message);
    public bool Confirm(string message) => UiBuilder.Ask(message);

    public void ClearForm()
    {
        _currentAdopterId = null;
        _txtFirstNames.Clear();
        _txtLastNames.Clear();
        _txtDocumentId.Clear();
        _txtPhone.Clear();
        _txtEmail.Clear();
        _txtCountry.Clear();
        _txtAddress.Clear();
        _grid.ClearSelection();
        _txtFirstNames.Focus();
    }
}