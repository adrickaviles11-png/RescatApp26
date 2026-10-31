using RescatApp26.Models.DTOs;
using RescatApp26.Presenter;
using RescatApp26.Services.Interfaz_service;
using RescatApp26.View.Interfaz;

namespace RescatApp26.View.FormUI;

public class QueueForm : Form, IQueueView, IRefreshable
{
    private readonly QueuePresenter _presenter;
    private readonly DataGridView _grid;
    private readonly Label _lblSummary;

    public void RefreshData() => _presenter.Load();

    public QueueForm(ISyncQueue queue)
    {
        _presenter = new QueuePresenter(this, queue);

        Text = "Cola de sincronización";
        ClientSize = new Size(780, 580);
        StartPosition = FormStartPosition.CenterScreen;
        BackColor = Color.White;

        UiBuilder.AddTitle(this, "Cola de sincronización", 15, 10);

        _lblSummary = new Label { Left = 15, Top = 48, Width = 740, Height = 22, ForeColor = UiBuilder.Primary };
        Controls.Add(_lblSummary);

        _grid = UiBuilder.AddGrid(this, 15, 78, 740, 350);
        UiBuilder.AddColumn(_grid, "Id", "N°", 40);
        UiBuilder.AddColumn(_grid, "Entity", "Entidad", 75);
        UiBuilder.AddColumn(_grid, "Operation", "Operación", 80);
        UiBuilder.AddColumn(_grid, "Status", "Estado", 80);
        UiBuilder.AddColumn(_grid, "Attempts", "Intentos", 60);
        UiBuilder.AddColumn(_grid, "CreatedAt", "Creado", 130, "dd/MM/yyyy HH:mm:ss");
        UiBuilder.AddColumn(_grid, "SentAt", "Enviado", 130, "dd/MM/yyyy HH:mm:ss");
        UiBuilder.AddColumn(_grid, "LastError", "Último error", 150);

        var btnRefresh = UiBuilder.AddButton(this, "Actualizar", 15, 445);
        btnRefresh.Click += (s, e) => _presenter.Load();
        var btnAdd = UiBuilder.AddButton(this, "Encolar prueba", 135, 445, 140);
        btnAdd.Click += (s, e) => _presenter.EnqueueTest();
        var btnNext = UiBuilder.AddButton(this, "Atender el primero", 285, 445, 160);
        btnNext.Click += (s, e) => _presenter.ProcessNextTest();

        Load += (s, e) => _presenter.Load();
    }

    public void ShowItems(IEnumerable<QueueItemDto> items) => _grid.DataSource = items.ToList();
    public void ShowSummary(string text) => _lblSummary.Text = text;
    public void ShowMessage(string message) => UiBuilder.Info(message);
    public void ShowError(string message) => UiBuilder.Error(message);
}