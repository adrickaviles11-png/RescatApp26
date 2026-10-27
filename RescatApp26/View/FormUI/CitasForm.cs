using RescatApp26.Models.DTOs;
using RescatApp26.Models.Enums;
using RescatApp26.Presenter;
using RescatApp26.Services.Interfaz_service;
using RescatApp26.View.Interfaz;

namespace RescatApp26.View.FormUI;

public class CitasForm : Form, ICitaView
{
    private readonly CitaPresenter _presenter;
    private readonly DataGridView _grid;
    private readonly ComboBox _cboType;
    private readonly ComboBox _cboStatus;
    private readonly ComboBox _cboPet;
    private readonly ComboBox _cboVeterinary;
    private readonly ComboBox _cboAdopter;
    private readonly Label _lblVeterinary;
    private readonly Label _lblAdopter;
    private readonly DateTimePicker _dtpWhen;
    private readonly TextBox _txtReason;
    private readonly TextBox _txtNotes;
    private int? _currentCitaId;

    public CitasForm(ICitaRepository citaRepository, IAdopterRepository adopterRepository)
    {
        _presenter = new CitaPresenter(this, citaRepository, adopterRepository);

        Text = "Citas";
        ClientSize = new Size(780, 580);
        StartPosition = FormStartPosition.CenterScreen;
        BackColor = Color.White;
        UiBuilder.AddTitle(this, "Citas", 15, 10);

        _grid = UiBuilder.AddGrid(this, 15, 55, 740, 200);
        UiBuilder.AddColumn(_grid, "Id", "ID", 35);
        UiBuilder.AddColumn(_grid, "TypeText", "Tipo", 105);
        UiBuilder.AddColumn(_grid, "PetName", "Mascota", 90);
        UiBuilder.AddColumn(_grid, "DestinationName", "Veterinaria / Adoptante", 165);
        UiBuilder.AddColumn(_grid, "ScheduledAt", "Fecha y hora", 135, "dd/MM/yyyy HH:mm");
        UiBuilder.AddColumn(_grid, "Reason", "Motivo", 110);
        UiBuilder.AddColumn(_grid, "StatusText", "Estado", 90);
        _grid.CellClick += (s, e) =>
        {
            if (e.RowIndex >= 0 && _grid.Rows[e.RowIndex].DataBoundItem is CitaDto cita)
                _presenter.SelectCita(cita.Id);
        };

        int y1 = 275, y2 = 310, y3 = 345, y4 = 380;

        UiBuilder.AddLabel(this, "Tipo", 15, y1);
        _cboType = UiBuilder.AddOptionCombo(this, 120, y1, 200, CitaOptions.Types);
        UiBuilder.AddLabel(this, "Estado", 350, y1);
        _cboStatus = UiBuilder.AddOptionCombo(this, 460, y1, 200, CitaOptions.Statuses);

        UiBuilder.AddLabel(this, "Mascota", 15, y2);
        _cboPet = new ComboBox
        {
            Left = 120,
            Top = y2,
            Width = 200,
            DropDownStyle = ComboBoxStyle.DropDownList,
            DisplayMember = "Text",
            ValueMember = "Id"
        };
        Controls.Add(_cboPet);
        UiBuilder.AddLabel(this, "Fecha y hora", 350, y2);
        _dtpWhen = new DateTimePicker
        {
            Left = 460,
            Top = y2,
            Width = 200,
            Format = DateTimePickerFormat.Custom,
            CustomFormat = "dd/MM/yyyy HH:mm",
            Value = NextHour()
        };
        Controls.Add(_dtpWhen);

        // Estos dos combos ocupan el mismo lugar; según el tipo se muestra uno u otro.
        _lblVeterinary = UiBuilder.AddLabel(this, "Veterinaria", 15, y3);
        _cboVeterinary = new ComboBox
        {
            Left = 120,
            Top = y3,
            Width = 200,
            DropDownStyle = ComboBoxStyle.DropDownList,
            DisplayMember = "Text",
            ValueMember = "Id"
        };
        Controls.Add(_cboVeterinary);
        _lblAdopter = UiBuilder.AddLabel(this, "Adoptante", 15, y3);
        _cboAdopter = new ComboBox
        {
            Left = 120,
            Top = y3,
            Width = 200,
            DropDownStyle = ComboBoxStyle.DropDownList,
            DisplayMember = "FullName",
            ValueMember = "Id"
        };
        Controls.Add(_cboAdopter);

        UiBuilder.AddLabel(this, "Motivo", 350, y3);
        _txtReason = UiBuilder.AddTextBox(this, 460, y3, 300);
        _txtReason.MaxLength = 200;

        UiBuilder.AddLabel(this, "Notas", 15, y4);
        _txtNotes = UiBuilder.AddTextBox(this, 120, y4, 620);
        _txtNotes.Multiline = true;
        _txtNotes.Height = 55;
        _txtNotes.MaxLength = 500;

        var btnNew = UiBuilder.AddButton(this, "Nuevo", 15, 460);
        btnNew.Click += (s, e) => _presenter.NewCita();
        var btnSave = UiBuilder.AddButton(this, "Guardar", 135, 460);
        btnSave.Click += (s, e) => _presenter.SaveCita();
        var btnCancel = UiBuilder.AddButton(this, "Cancelar cita", 255, 460, 130, danger: true);
        btnCancel.Click += (s, e) => _presenter.CancelCita();

        _cboType.SelectedIndexChanged += (s, e) => UpdateTypeVisibility();
        UpdateTypeVisibility();

        Load += (s, e) => _presenter.LoadData();
    }

    private static DateTime NextHour()
    {
        var now = DateTime.Now;
        return now.Date.AddHours(now.Hour + 1);
    }

    private void UpdateTypeVisibility()
    {
        bool isVet = TypeCode == (int)CitaTipo.Veterinaria;
        _lblVeterinary.Visible = isVet;
        _cboVeterinary.Visible = isVet;
        _lblAdopter.Visible = !isVet;
        _cboAdopter.Visible = !isVet;
    }

    private static string? NullIfEmpty(string text) =>
        string.IsNullOrWhiteSpace(text) ? null : text.Trim();

    public int? SelectedCitaId => _currentCitaId;
    public int TypeCode => _cboType.SelectedValue is int v ? v : 0;
    public int? SelectedPetId => _cboPet.SelectedValue is int id ? id : null;
    public int? SelectedVeterinaryId => _cboVeterinary.SelectedValue is int id ? id : null;
    public int? SelectedAdopterId => _cboAdopter.SelectedValue is int id ? id : null;
    public DateTime ScheduledAt => _dtpWhen.Value;
    public string? Reason => NullIfEmpty(_txtReason.Text);
    public int StatusCode => _cboStatus.SelectedValue is int v ? v : 0;
    public string? Notes => NullIfEmpty(_txtNotes.Text);

    public void ShowCitas(IEnumerable<CitaDto> citas) => _grid.DataSource = citas.ToList();
    public void ShowPets(IEnumerable<LookupItemDto> pets) => _cboPet.DataSource = pets.ToList();
    public void ShowVeterinarias(IEnumerable<LookupItemDto> veterinarias) => _cboVeterinary.DataSource = veterinarias.ToList();
    public void ShowAdopters(IEnumerable<AdopterDto> adopters) => _cboAdopter.DataSource = adopters.ToList();

    public void ShowCita(CitaDto cita)
    {
        _currentCitaId = cita.Id;
        _cboType.SelectedValue = cita.Type;
        UpdateTypeVisibility();

        // Una cita antigua puede ser de una mascota que ya fue adoptada y no está en la lista.
        EnsurePetInList(cita.PetId, cita.PetName);
        _cboPet.SelectedValue = cita.PetId;

        if (cita.VeterinaryId is int vetId) _cboVeterinary.SelectedValue = vetId;
        if (cita.AdopterId is int adopterId) _cboAdopter.SelectedValue = adopterId;

        _dtpWhen.Value = cita.ScheduledAt < _dtpWhen.MinDate ? DateTime.Now : cita.ScheduledAt;
        _txtReason.Text = cita.Reason ?? string.Empty;
        _txtNotes.Text = cita.Notes ?? string.Empty;
        _cboStatus.SelectedValue = cita.Status;
    }

    private void EnsurePetInList(int petId, string petName)
    {
        var pets = _cboPet.DataSource as List<LookupItemDto> ?? new List<LookupItemDto>();
        if (pets.Any(p => p.Id == petId)) return;

        var extended = new List<LookupItemDto>(pets) { new LookupItemDto { Id = petId, Text = petName } };
        _cboPet.DataSource = extended;
    }

    public void ShowMessage(string message) => UiBuilder.Info(message);
    public void ShowError(string message) => UiBuilder.Error(message);
    public bool Confirm(string message) => UiBuilder.Ask(message);

    public void ClearForm()
    {
        _currentCitaId = null;
        _cboType.SelectedIndex = 0;
        _cboStatus.SelectedIndex = 0;
        if (_cboPet.Items.Count > 0) _cboPet.SelectedIndex = 0;
        if (_cboVeterinary.Items.Count > 0) _cboVeterinary.SelectedIndex = 0;
        if (_cboAdopter.Items.Count > 0) _cboAdopter.SelectedIndex = 0;
        _dtpWhen.Value = NextHour();
        _txtReason.Clear();
        _txtNotes.Clear();
        UpdateTypeVisibility();
        _grid.ClearSelection();
    }
}