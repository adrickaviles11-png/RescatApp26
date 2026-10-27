using RescatApp26.Models.Enums;

namespace RescatApp26.View.FormUI;


/// <summary>Ayudas para construir los formularios por código (sin diseñador).</summary>
internal static class UiBuilder
{
    public static readonly Color Primary = Color.FromArgb(38, 70, 83);
    public static readonly Color Accent = Color.FromArgb(42, 157, 143);
    public static readonly Color Danger = Color.FromArgb(193, 18, 31);

    public static Label AddLabel(Control parent, string text, int x, int y)
    {
        var label = new Label { Text = text, Left = x, Top = y + 4, AutoSize = true };
        parent.Controls.Add(label);
        return label;
    }

    public static Label AddTitle(Control parent, string text, int x, int y)
    {
        var label = new Label
        {
            Text = text,
            Left = x,
            Top = y,
            AutoSize = true,
            Font = new Font("Segoe UI", 15F, FontStyle.Bold),
            ForeColor = Primary
        };
        parent.Controls.Add(label);
        return label;
    }

    public static TextBox AddTextBox(Control parent, int x, int y, int width = 220, bool password = false)
    {
        var textBox = new TextBox { Left = x, Top = y, Width = width, UseSystemPasswordChar = password };
        parent.Controls.Add(textBox);
        return textBox;
    }

    public static ComboBox AddCombo(Control parent, int x, int y, int width, params string[] items)
    {
        var combo = new ComboBox { Left = x, Top = y, Width = width, DropDownStyle = ComboBoxStyle.DropDownList };
        combo.Items.AddRange(items);
        if (items.Length > 0) combo.SelectedIndex = 0;
        parent.Controls.Add(combo);
        return combo;
    }
    public static ComboBox AddOptionCombo(Control parent, int x, int y, int width, IEnumerable<OptionItem> items)
    {
        var combo = new ComboBox
        {
            Left = x,
            Top = y,
            Width = width,
            DropDownStyle = ComboBoxStyle.DropDownList,
            DisplayMember = nameof(OptionItem.Text),
            ValueMember = nameof(OptionItem.Value)
        };
        combo.DataSource = items.ToList();
        parent.Controls.Add(combo);
        return combo;
    }
    public static Button AddButton(Control parent, string text, int x, int y, int width = 110, bool danger = false)
    {
        var button = new Button
        {
            Text = text,
            Left = x,
            Top = y,
            Width = width,
            Height = 34,
            FlatStyle = FlatStyle.Flat,
            ForeColor = Color.White,
            Cursor = Cursors.Hand,
            BackColor = danger ? Danger : Accent
        };
        button.FlatAppearance.BorderSize = 0;
        parent.Controls.Add(button);
        return button;
    }

    public static DataGridView AddGrid(Control parent, int x, int y, int width, int height)
    {
        var grid = new DataGridView
        {
            Left = x,
            Top = y,
            Width = width,
            Height = height,
            ReadOnly = true,
            AllowUserToAddRows = false,
            AllowUserToDeleteRows = false,
            AutoGenerateColumns = false,
            MultiSelect = false,
            RowHeadersVisible = false,
            SelectionMode = DataGridViewSelectionMode.FullRowSelect,
            BackgroundColor = Color.White,
            BorderStyle = BorderStyle.FixedSingle,
            Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right
        };
        grid.ColumnHeadersDefaultCellStyle.BackColor = Primary;
        grid.ColumnHeadersDefaultCellStyle.ForeColor = Color.White;
        grid.EnableHeadersVisualStyles = false;
        parent.Controls.Add(grid);
        return grid;
    }

    public static void AddColumn(DataGridView grid, string property, string header, int width, string? format = null)
    {
        var column = new DataGridViewTextBoxColumn
        {
            Name = property,
            DataPropertyName = property,
            HeaderText = header,
            Width = width
        };
        if (format is not null) column.DefaultCellStyle.Format = format;
        grid.Columns.Add(column);
    }

    public static void Info(string message) =>
        MessageBox.Show(message, "RescatApp", MessageBoxButtons.OK, MessageBoxIcon.Information);

    public static void Error(string message) =>
        MessageBox.Show(message, "RescatApp", MessageBoxButtons.OK, MessageBoxIcon.Error);

    public static bool Ask(string message) =>
        MessageBox.Show(message, "Confirmar", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes;
}