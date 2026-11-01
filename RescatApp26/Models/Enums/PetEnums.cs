namespace RescatApp26.Models.Enums;

// Los números son los que guarda tu base de datos.
public enum PetSex { Macho = 0, Hembra = 1 }
public enum PetSize { Pequeno = 0, Mediano = 1, Grande = 2 }
public enum PetStatus { Disponible = 0, EnProceso = 1, Adoptada = 2 }

/// <summary>Un par número/texto para llenar los combos de la pantalla.</summary>
public record OptionItem(int Value, string Text);

public static class PetOptions
{
    public static readonly IReadOnlyList<OptionItem> Sexes = new[]
    {
        new OptionItem((int)PetSex.Macho, "Macho"),
        new OptionItem((int)PetSex.Hembra, "Hembra")
    };

    public static readonly IReadOnlyList<OptionItem> Sizes = new[]
    {
        new OptionItem((int)PetSize.Pequeno, "Pequeño"),
        new OptionItem((int)PetSize.Mediano, "Mediano"),
        new OptionItem((int)PetSize.Grande, "Grande")
    };

    public static readonly IReadOnlyList<OptionItem> Statuses = new[]
    {
        new OptionItem((int)PetStatus.Disponible, "Disponible"),
        new OptionItem((int)PetStatus.EnProceso, "En proceso"),
        new OptionItem((int)PetStatus.Adoptada, "Adoptada")
    };

    public static string SexText(int value) => Sexes.FirstOrDefault(o => o.Value == value)?.Text ?? "Desconocido";
    public static string SizeText(int value) => Sizes.FirstOrDefault(o => o.Value == value)?.Text ?? "Desconocido";
    public static string StatusText(int value) => Statuses.FirstOrDefault(o => o.Value == value)?.Text ?? "Desconocido";
}