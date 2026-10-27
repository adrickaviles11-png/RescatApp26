using RescatApp26.Models.DTOs;
using RescatApp26.Presenter;
using RescatApp26.Services.Interfaz_service;
using RescatApp26.View.Interfaz;

namespace RescatApp26.View.FormUI;

public class DashboardForm : Form, IDashboardView
{
    private readonly DashboardPresenter _presenter;
    private readonly Label _lblAvailable;
    private readonly Label _lblAdopted;
    private readonly Label _lblAdopters;
    private readonly Label _lblAdoptionsMonth;
    private readonly Label _lblWeek;
    private readonly Label _lblHalf;
    private readonly Label _lblMonth;
    private readonly BarChartControl _chartStatus;
    private readonly BarChartControl _chartType;

    public DashboardForm(IDashboardRepository repository)
    {
        _presenter = new DashboardPresenter(this, repository);

        Text = "Resumen";
        ClientSize = new Size(780, 580);
        StartPosition = FormStartPosition.CenterScreen;
        BackColor = Color.White;

        UiBuilder.AddTitle(this, "Resumen general", 15, 10);
        var btnRefresh = UiBuilder.AddButton(this, "Actualizar", 645, 12, 110);
        btnRefresh.Click += (s, e) => _presenter.LoadSummary();

        _lblAvailable = AddCard("Mascotas disponibles", UiBuilder.Accent, Color.White, 15);
        _lblAdopted = AddCard("Mascotas adoptadas", UiBuilder.Primary, Color.White, 203);
        _lblAdopters = AddCard("Adoptantes registrados", UiBuilder.Highlight, UiBuilder.Primary, 391);
        _lblAdoptionsMonth = AddCard("Adopciones este mes", UiBuilder.Cream, UiBuilder.Primary, 579);

        var pnlMonthly = new Panel
        {
            Left = 15,
            Top = 150,
            Width = 360,
            Height = 170,
            BorderStyle = BorderStyle.FixedSingle,
            BackColor = Color.White
        };
        Controls.Add(pnlMonthly);
        pnlMonthly.Controls.Add(new Label
        {
            Text = "Rendimiento mensual",
            Left = 0,
            Top = 10,
            Width = 358,
            Height = 26,
            TextAlign = ContentAlignment.MiddleCenter,
            Font = new Font("Segoe UI", 11F, FontStyle.Bold),
            ForeColor = UiBuilder.Primary
        });
        _lblWeek = AddStatRow(pnlMonthly, "Citas de la semana actual", 50);
        _lblHalf = AddStatRow(pnlMonthly, "Citas del 1 al 15 del mes", 85);
        _lblMonth = AddStatRow(pnlMonthly, "Citas del mes", 120);

        _chartStatus = new BarChartControl { Left = 15, Top = 335, Width = 360, Height = 230, Title = "Citas por estado" };
        _chartType = new BarChartControl { Left = 390, Top = 150, Width = 365, Height = 415, Title = "Citas por tipo" };
        Controls.Add(_chartStatus);
        Controls.Add(_chartType);

        Load += (s, e) => _presenter.LoadSummary();
    }

    private Label AddCard(string caption, Color back, Color fore, int x)
    {
        var card = new Panel { Left = x, Top = 55, Width = 175, Height = 80, BackColor = back };
        var value = new Label
        {
            Text = "0",
            Left = 0,
            Top = 8,
            Width = 175,
            Height = 40,
            TextAlign = ContentAlignment.MiddleCenter,
            Font = new Font("Segoe UI", 22F, FontStyle.Bold),
            ForeColor = fore
        };
        var text = new Label
        {
            Text = caption,
            Left = 0,
            Top = 50,
            Width = 175,
            Height = 22,
            TextAlign = ContentAlignment.MiddleCenter,
            ForeColor = fore
        };
        card.Controls.Add(value);
        card.Controls.Add(text);
        Controls.Add(card);
        return value;
    }

    private static Label AddStatRow(Panel parent, string caption, int y)
    {
        parent.Controls.Add(new Label
        {
            Text = caption,
            Left = 15,
            Top = y,
            Width = 250,
            Height = 24,
            TextAlign = ContentAlignment.MiddleLeft
        });
        var value = new Label
        {
            Text = "0",
            Left = 270,
            Top = y,
            Width = 70,
            Height = 24,
            TextAlign = ContentAlignment.MiddleRight,
            Font = new Font("Segoe UI", 11F, FontStyle.Bold),
            ForeColor = UiBuilder.Primary
        };
        parent.Controls.Add(value);
        return value;
    }

    public void ShowSummary(DashboardDto summary)
    {
        _lblAvailable.Text = summary.AvailablePets.ToString();
        _lblAdopted.Text = summary.AdoptedPets.ToString();
        _lblAdopters.Text = summary.Adopters.ToString();
        _lblAdoptionsMonth.Text = summary.AdoptionsThisMonth.ToString();
        _lblWeek.Text = summary.CitasThisWeek.ToString();
        _lblHalf.Text = summary.CitasFirstHalf.ToString();
        _lblMonth.Text = summary.CitasThisMonth.ToString();
        _chartStatus.SetData(summary.CitasByStatus);
        _chartType.SetData(summary.CitasByType);
    }

    public void ShowError(string message) => UiBuilder.Error(message);
}