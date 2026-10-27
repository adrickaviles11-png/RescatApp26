using RescatApp26.Models.DTOs;
using RescatApp26.Models.Enums;
using RescatApp26.Presenter;
using RescatApp26.Services.Interfaz_service;
using RescatApp26.View.Interfaz;

namespace RescatApp26.View.FormUI;

public class PetsForm : Form, IPetView
{
    private readonly PetPresenter _presenter;
    private readonly DataGridView _grid;
    private readonly TextBox _txtName;
    private readonly ComboBox _cboSpecies;
    private readonly TextBox _txtBreed;
    private readonly NumericUpDown _nudAgeMonths;
    private readonly ComboBox _cboSex;
    private readonly ComboBox _cboSize;
    private readonly ComboBox _cboStatus;
    private readonly TextBox _txtDescription;
    private int? _currentPetId;

    public PetsForm(IPetRepository repository)
    {
        _presenter = new PetPresenter(this, repository);

        Text = "Mascotas";
        ClientSize = new Size(780, 580);
        StartPosition = FormStartPosition.CenterScreen;
        BackColor = Color.White;
        UiBuilder.AddTitle(this, "Mascotas rescatadas", 15, 10);

        _grid = UiBuilder.AddGrid(this, 15, 55, 740, 230);
        UiBuilder.AddColumn(_grid, "Id", "ID", 40);
        UiBuilder.AddColumn(_grid, "Name", "Nombre", 120);
        UiBuilder.AddColumn(_grid, "Species", "Especie", 70);
        UiBuilder.AddColumn(_grid, "Breed", "Raza", 95);
        UiBuilder.AddColumn(_grid, "AgeMonths", "Edad", 55);
        UiBuilder.AddColumn(_grid, "SexText", "Sexo", 65);
        UiBuilder.AddColumn(_grid, "SizeText", "Tamaño", 75);
        UiBuilder.AddColumn(_grid, "StatusText", "Estado", 85);
        UiBuilder.AddColumn(_grid, "EntryDate", "Ingreso", 95, "dd/MM/yyyy");
        _grid.CellClick += (s, e) =>
        {
            if (e.RowIndex >= 0 && _grid.Rows[e.RowIndex].DataBoundItem is PetDto pet)
                _presenter.SelectPet(pet.Id);
        };

        int y1 = 305, y2 = 340, y3 = 375, y4 = 410, y5 = 445;

        UiBuilder.AddLabel(this, "Nombre", 15, y1);
        _txtName = UiBuilder.AddTextBox(this, 120, y1, 200);
        _txtName.MaxLength = 100;
        UiBuilder.AddLabel(this, "Especie", 350, y1);
        _cboSpecies = new ComboBox { Left = 460, Top = y1, Width = 200, DropDownStyle = ComboBoxStyle.DropDown, MaxLength = 50 };
        _cboSpecies.Items.AddRange(new object[] { "Perro", "Gato" });
        Controls.Add(_cboSpecies);

        UiBuilder.AddLabel(this, "Raza", 15, y2);
        _txtBreed = UiBuilder.AddTextBox(this, 120, y2, 200);
        _txtBreed.MaxLength = 50;
        UiBuilder.AddLabel(this, "Edad (meses)", 350, y2);
        _nudAgeMonths = new NumericUpDown { Left = 460, Top = y2, Width = 80, Minimum = 0, Maximum = 360 };
        Controls.Add(_nudAgeMonths);

        UiBuilder.AddLabel(this, "Sexo", 15, y3);
        _cboSex = UiBuilder.AddOptionCombo(this, 120, y3, 200, PetOptions.Sexes);
        UiBuilder.AddLabel(this, "Tamaño", 350, y3);
        _cboSize = UiBuilder.AddOptionCombo(this, 460, y3, 200, PetOptions.Sizes);

        UiBuilder.AddLabel(this, "Estado", 15, y4);
        _cboStatus = UiBuilder.AddOptionCombo(this, 120, y4, 200, PetOptions.Statuses);
        SetStatusOptions(includeAdopted: false);

        UiBuilder.AddLabel(this, "Descripción", 15, y5);
        _txtDescription = UiBuilder.AddTextBox(this, 120, y5, 520);
        _txtDescription.Multiline = true;
        _txtDescription.Height = 50;
        _txtDescription.MaxLength = 500;

        var btnNew = UiBuilder.AddButton(this, "Nuevo", 15, 515);
        btnNew.Click += (s, e) => _presenter.NewPet();
        var btnSave = UiBuilder.AddButton(this, "Guardar", 135, 515);
        btnSave.Click += (s, e) => _presenter.SavePet();
        var btnDelete = UiBuilder.AddButton(this, "Eliminar", 255, 515, danger: true);
        btnDelete.Click += (s, e) => _presenter.DeletePet();

        Load += (s, e) => _presenter.LoadPets();
    }

    // Estado "Adoptada" solo aparece para mascotas ya adoptadas; no se puede asignar a mano.
    private void SetStatusOptions(bool includeAdopted)
    {
        _cboStatus.DataSource = PetOptions.Statuses
            .Where(o => includeAdopted || o.Value != (int)PetStatus.Adoptada)
            .ToList();
    }

    public int? SelectedPetId => _currentPetId;
    public string PetName => _txtName.Text.Trim();
    public string Species => _cboSpecies.Text.Trim();
    public string? Breed => string.IsNullOrWhiteSpace(_txtBreed.Text) ? null : _txtBreed.Text.Trim();
    public int AgeMonths => (int)_nudAgeMonths.Value;
    public int SexCode => _cboSex.SelectedValue is int v ? v : 0;
    public int SizeCode => _cboSize.SelectedValue is int v ? v : 0;
    public string? Description => string.IsNullOrWhiteSpace(_txtDescription.Text) ? null : _txtDescription.Text.Trim();
    public int StatusCode => _cboStatus.SelectedValue is int v ? v : 0;

    public void ShowPets(IEnumerable<PetDto> pets) => _grid.DataSource = pets.ToList();

    public void ShowPet(PetDto pet)
    {
        _currentPetId = pet.Id;
        _txtName.Text = pet.Name;
        _cboSpecies.Text = pet.Species;
        _txtBreed.Text = pet.Breed ?? string.Empty;
        _nudAgeMonths.Value = Math.Clamp(pet.AgeMonths, (int)_nudAgeMonths.Minimum, (int)_nudAgeMonths.Maximum);
        _cboSex.SelectedValue = pet.Sex;
        _cboSize.SelectedValue = pet.Size;
        _txtDescription.Text = pet.Description ?? string.Empty;

        bool adopted = pet.Status == (int)PetStatus.Adoptada;
        SetStatusOptions(includeAdopted: adopted);
        _cboStatus.SelectedValue = pet.Status;
        _cboStatus.Enabled = !adopted;
    }

    public void ShowMessage(string message) => UiBuilder.Info(message);
    public void ShowError(string message) => UiBuilder.Error(message);
    public bool Confirm(string message) => UiBuilder.Ask(message);

    public void ClearForm()
    {
        _currentPetId = null;
        _txtName.Clear();
        _cboSpecies.Text = string.Empty;
        _txtBreed.Clear();
        _nudAgeMonths.Value = 0;
        _cboSex.SelectedIndex = 0;
        _cboSize.SelectedIndex = 0;
        _txtDescription.Clear();
        SetStatusOptions(includeAdopted: false);
        _cboStatus.SelectedIndex = 0;
        _cboStatus.Enabled = true;
        _grid.ClearSelection();
        _txtName.Focus();
    }
}