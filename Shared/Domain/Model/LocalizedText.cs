using System.Text.Json;

namespace SafeFlow.API.Shared.Domain.Model;

/// <summary>Texto bilingüe compatible con el frontend SafeFlow.</summary>
public sealed record LocalizedText(string En, string Es)
{
    public static LocalizedText FromRaw(string? value)
    {
        if (string.IsNullOrWhiteSpace(value)) return new LocalizedText("—", "—");
        var trimmed = value.Trim();
        if (trimmed.StartsWith('{'))
        {
            try
            {
                using var doc = JsonDocument.Parse(trimmed);
                var root = doc.RootElement;
                var en = root.TryGetProperty("en", out var e) ? e.GetString() ?? trimmed : trimmed;
                var es = root.TryGetProperty("es", out var s) ? s.GetString() ?? trimmed : trimmed;
                return new LocalizedText(en, es);
            }
            catch
            {
                return new LocalizedText(trimmed, trimmed);
            }
        }

        return new LocalizedText(trimmed, trimmed);
    }

    public string ToStorageJson() => JsonSerializer.Serialize(new { en = En, es = Es });

    public object ToApiObject() => new { en = En, es = Es };
}
