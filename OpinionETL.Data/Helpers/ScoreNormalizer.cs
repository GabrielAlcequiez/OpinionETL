namespace OpinionETL.Data.Helpers;

public static class ScoreNormalizer
{
    public static int? Normalize(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return null;

        if (!int.TryParse(value, out int score))
            return null;

        return score is >= 1 and <= 5 ? score : null;
    }
}