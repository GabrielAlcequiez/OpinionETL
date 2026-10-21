namespace OpinionETL.Data.Helpers;

public static class OpinionClassifier
{
    public static string Classify(
        string? classification,
        int? score)
    {
        if (!string.IsNullOrWhiteSpace(classification))
        {
            string value = classification.Trim();

            if (value.Equals("Positiva", StringComparison.OrdinalIgnoreCase))
                return "Positiva";

            if (value.Equals("Negativa", StringComparison.OrdinalIgnoreCase))
                return "Negativa";

            if (value.Equals("Neutra", StringComparison.OrdinalIgnoreCase))
                return "Neutra";
        }

        return score switch
        {
            >= 4 => "Positiva",
            3 => "Neutra",
            <= 2 => "Negativa",
            _ => "Neutra"
        };
    }
}