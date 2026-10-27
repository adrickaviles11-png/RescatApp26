using RescatApp26.Models.DTOs;
using ScottPlot.WinForms;
using System.ComponentModel;

namespace RescatApp26.View.FormUI;

/// <summary>Gráfico de barras basado en ScottPlot 5. Mantiene la misma interfaz que la versión dibujada a mano.</summary>
public class BarChartControl : UserControl
{
    private static readonly string[] Palette = { "#2970AD", "#F8D053", "#1F4B74", "#C1121F" };
    private readonly FormsPlot _plot = new() { Dock = DockStyle.Fill };

    [Browsable(false)]
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public string Title { get; set; } = string.Empty;

    public BarChartControl()
    {
        BorderStyle = BorderStyle.FixedSingle;
        Controls.Add(_plot);
    }

    public void SetData(IEnumerable<ChartItemDto> items)
    {
        var list = items.ToList();
        var plot = _plot.Plot;
        plot.Clear();

        var bars = new List<ScottPlot.Bar>();
        var ticks = new List<ScottPlot.Tick>();
        for (int i = 0; i < list.Count; i++)
        {
            bars.Add(new ScottPlot.Bar
            {
                Position = i,
                Value = list[i].Value,
                FillColor = ScottPlot.Color.FromHex(Palette[i % Palette.Length]),
                Label = list[i].Value.ToString()   // número sobre la barra
            });
            ticks.Add(new ScottPlot.Tick(i, list[i].Label));
        }

        plot.Add.Bars(bars);
        plot.Axes.Bottom.TickGenerator = new ScottPlot.TickGenerators.NumericManual(ticks.ToArray());
        plot.Axes.Left.TickGenerator = new ScottPlot.TickGenerators.NumericAutomatic { IntegerTicksOnly = true };
        plot.Axes.Margins(bottom: 0);
        plot.Title(Title);

        int max = list.Count == 0 ? 1 : Math.Max(1, list.Max(x => x.Value));
        plot.Axes.SetLimitsY(0, max * 1.25);
        _plot.Refresh();
    }
}