namespace ReportCards.Web.Extensions;

public static class StringExtensions
{
    /// <summary>Returns null if the string is null or whitespace, otherwise trims and returns it.</summary>
    public static string? NullIfEmpty(this string? value)
        => string.IsNullOrWhiteSpace(value) ? null : value.Trim();
}
