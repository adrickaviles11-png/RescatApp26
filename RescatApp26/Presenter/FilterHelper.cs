using System.Globalization;

namespace RescatApp26.Presenter;

internal static class FilterHelper
{
    /// <summary>Busca texto ignorando mayúsculas y acentos.</summary>
    public static bool Contains(string? value, string text) =>
        !string.IsNullOrEmpty(value) &&
        CultureInfo.CurrentCulture.CompareInfo.IndexOf(
            value, text, CompareOptions.IgnoreCase | CompareOptions.IgnoreNonSpace) >= 0;
}