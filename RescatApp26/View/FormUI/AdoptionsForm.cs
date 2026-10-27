using RescatApp26.Models.DTOs;
using RescatApp26.Presenter;
using RescatApp26.Services.Interfaz_service;
using RescatApp26.View.Interfaz;

namespace RescatApp26.View.FormUI;

public class AdoptionsForm : Form, IAdoptionView
{
    private readonly AdoptionPresenter _presenter;
    private readonly ComboBox _cboPet;
    private readonly ComboBox _cboAdopter;
    private readonly NumericUpDown _nudCost;
    private readonly TextBox _txtNotes;
    private readonly DataGridView _grid;

    public AdoptionsForm(IAdoptionRepository adoptionRepository, IAdopterRepository adopterRepository)
    {
        _presenter = new AdoptionPresenter(this, adoptionRepository, adopterRepository);

        Text = "Adopciones";
        ClientSize = new Size(780, 580);
        StartPosition = FormStartPosition.CenterScreen;
        BackColor = Color.White;
        UiBuilder.AddTitle(this, "Registro de adopciones", 15, 10);

        UiBuilder.AddLabel(this, "Mascota", 15, 60);
        _cboPet = new ComboBox
        {
            Left = 110,
            Top = 60,
            Width = 260,
            DropDownStyle = ComboBoxStyle.DropDownList,
            DisplayMember = "Text",
            ValueMember = "Id"
        };
        Controls.Add(_cboPet);

        UiBuilder.AddLabel(this, "Adoptante", 400, 60);
        _cboAdopter = new ComboBox
        {
            Left = 480,
            Top = 60,
            Width = 275,
            DropDownStyle = ComboBoxStyle.DropDownList,
            DisplayMember = "FullName",
            ValueMember = "Id"
        };
        Controls.Add(_cboAdopter);

        UiBuilder.AddLabel(this, "Costo", 15, 100);
        _nudCost = new NumericUpDown
        {
            Left = 110,
            Top = 100,
            Width = 120,
            DecimalPlaces = 2,
            Minimum = 0,
            Maximum = 1000000,
            ThousandsSeparator = true
        };
        Controls.Add(_nudCost);

        UiBuilder.AddLabel(this, "Notas", 260, 100);
        _txtNotes = UiBuilder.AddTextBox(this, 360, 100, 395);
        _txtNotes.MaxLength = 500;

        var btnRegister = UiBuilder.AddButton(this, "Registrar adopción", 110, 140, 190);
        btnRegister.Click += (s, e) => _presenter.RegisterAdoption();

        _grid = UiBuilder.AddGrid(this, 15, 195, 740, 340);
        UiBuilder.AddColumn(_grid, "Code", "Código", 65);
        UiBuilder.AddColumn(_grid, "PetName", "Mascota", 110);
        UiBuilder.AddColumn(_grid, "AdopterName", "Adoptante", 170);
        UiBuilder.AddColumn(_grid, "Cost", "Costo", 80, "N2");
        UiBuilder.AddColumn(_grid, "Date", "Fecha", 135, "dd/MM/yyyy HH:mm");
        UiBuilder.AddColumn(_grid, "Notes", "Observaciones", 140);

        Load += (s, e) => _presenter.LoadData();
    }

    public int? SelectedPetId => _cboPet.SelectedValue is int id ? id : null;
    public int? SelectedAdopterId => _cboAdopter.SelectedValue is int id ? id : null;
    public decimal Cost => _nudCost.Value;
    public string? Notes => string.IsNullOrWhiteSpace(_txtNotes.Text) ? null : _txtNotes.Text.Trim();

    public void ShowAdoptablePets(IEnumerable<LookupItemDto> pets) => _cboPet.DataSource = pets.ToList();
    public void ShowAdopters(IEnumerable<AdopterDto> adopters) => _cboAdopter.DataSource = adopters.ToList();
    public void ShowAdoptions(IEnumerable<AdoptionListDto> adoptions) => _grid.DataSource = adoptions.ToList();

    public void ShowMessage(string message) => UiBuilder.Info(message);
    public void ShowError(string message) => UiBuilder.Error(message);

    public void ClearForm()
    {
        _nudCost.Value = 0;
        _txtNotes.Clear();
    }
}