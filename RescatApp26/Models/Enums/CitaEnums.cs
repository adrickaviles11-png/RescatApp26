namespace RescatApp26.Models.Enums;

// Los números son los que guarda la tabla Citas.
public enum CitaTipo { Veterinaria = 0, VisitaAdopcion = 1 }
public enum CitaEstado { Programada = 0, Confirmada = 1, Completada = 2, Cancelada = 3 }

public static class CitaOptions
{
    public static readonly IReadOnlyList<OptionItem> Types = new[]
    {
        new OptionItem((int)CitaTipo.Veterinaria, "Veterinaria"),
        new OptionItem((int)CitaTipo.VisitaAdopcion, "Visita de adopción")
    };

    public static readonly IReadOnlyList<OptionItem> Statuses = new[]
    {
        new OptionItem((int)CitaEstado.Programada, "Programada"),
        new OptionItem((int)CitaEstado.Confirmada, "Confirmada"),
        new OptionItem((int)CitaEstado.Completada, "Completada"),
        new OptionItem((int)CitaEstado.Cancelada, "Cancelada")
    };

    public static string TypeText(int value) => Types.FirstOrDefault(o => o.Value == value)?.Text ?? "Desconocido";
    public static string StatusText(int value) => Statuses.FirstOrDefault(o => o.Value == value)?.Text ?? "Desconocido";
}