using System.Text.RegularExpressions;

namespace OpinionETL.Data.Helpers;

public static class IdNormalizer
{
    public static int? NormalizeId(string? id)
    {
        if (string.IsNullOrWhiteSpace(id))
            return null;

        string value = id.Trim();

        // Permite valores como 123, C007, P016, T0001.
        if (!Regex.IsMatch(value, @"^(?:[A-Za-z]+)?\d+$"))
            return null;

        string numbers = Regex.Replace(value, @"^[A-Za-z]+", "");

        return int.TryParse(numbers, out int result) && result > 0
            ? result
            : null;
    }
}