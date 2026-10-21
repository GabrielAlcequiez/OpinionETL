using System.Globalization;

namespace OpinionETL.Data.Helpers;

public static class DateNormalizer
{
    public static DateTime? Normalize(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return null;

        return DateTime.TryParse(
            value,
            CultureInfo.InvariantCulture,
            DateTimeStyles.None,
            out DateTime date)
            ? date
            : null;
    }
}