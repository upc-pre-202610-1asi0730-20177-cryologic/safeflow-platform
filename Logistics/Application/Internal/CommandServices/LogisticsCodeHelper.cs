using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;
using SafeFlow.API.Logistics.Domain.Model.Aggregates;
using SafeFlow.API.Shared.Domain.Model;

namespace SafeFlow.API.Logistics.Application.Internal.CommandServices;

/// <summary>
/// Provides code generation, validation, and text normalization utilities for logistics entities.
/// Handles slug generation, unique code assignment, and multilingual text parsing.
/// </summary>
internal static partial class LogisticsCodeHelper
{
    /// <summary>
    /// Validates codigo format: lowercase letter start, followed by 1–47 alphanumeric or underscore characters.
    /// </summary>
    private static readonly Regex CodigoRegex = new(@"^[a-z][a-z0-9_]{1,47}$", RegexOptions.Compiled);

    /// <summary>
    /// Normalizes a raw string to valid codigo format: lowercase, non-alphanumeric → underscores, collapse multiples, trim underscores.
    /// </summary>
    /// <param name="raw">Input string; null or whitespace returns empty string.</param>
    /// <returns>Normalized codigo.</returns>
    public static string NormalizeCodigo(string? raw)
    {
        if (string.IsNullOrWhiteSpace(raw)) return string.Empty;
        var s = raw.Trim().ToLowerInvariant();
        s = NonAlnumRegex().Replace(s, "_");
        s = MultiUnderscoreRegex().Replace(s, "_").Trim('_');
        return s;
    }

    /// <summary>
    /// Checks if codigo matches the valid format pattern.
    /// </summary>
    /// <param name="codigo">Code to validate.</param>
    /// <returns>True if valid; false otherwise.</returns>
    public static bool IsValidCodigo(string codigo) => CodigoRegex.IsMatch(codigo);

    /// <summary>
    /// Parses multilingual text from string or JSON sources, defaulting to "—" if absent.
    /// </summary>
    /// <param name="nombre">Primary source (string or JsonElement with "en"/"es" properties).</param>
    /// <param name="nombreEn">Fallback English text.</param>
    /// <param name="nombreEs">Fallback Spanish text.</param>
    /// <returns>Localized text with English and Spanish variants.</returns>
    public static LocalizedText ParseNombre(object? nombre, string? nombreEn, string? nombreEs)
    {
        var raw = PickTrimmed(nombre as string, nombreEn, nombreEs);
        if (string.IsNullOrWhiteSpace(raw) && nombre is System.Text.Json.JsonElement je)
        {
            var en = je.TryGetProperty("en", out var e) ? e.GetString() : null;
            var es = je.TryGetProperty("es", out var s) ? s.GetString() : null;
            return new LocalizedText(
                PickTrimmed(en, es) ?? "—",
                PickTrimmed(es, en) ?? "—");
        }

        if (string.IsNullOrWhiteSpace(raw))
            return new LocalizedText("—", "—");

        return new LocalizedText(raw, raw);
    }

    /// <summary>
    /// Converts a name to lowercase alphanumeric slug, removing accents and non-alphanumeric characters.
    /// </summary>
    /// <param name="nombreRaw">Name to slugify.</param>
    /// <returns>Normalized slug.</returns>
    public static string SlugFromNombre(string nombreRaw)
    {
        var normalized = nombreRaw.Normalize(NormalizationForm.FormD);
        var sb = new StringBuilder();
        foreach (var ch in normalized)
        {
            if (CharUnicodeInfo.GetUnicodeCategory(ch) != UnicodeCategory.NonSpacingMark)
                sb.Append(ch);
        }

        return NormalizeCodigo(sb.ToString());
    }

    /// <summary>
    /// Generates a unique destination slug, appending numeric suffixes if the base slug is already taken by routes or destinations.
    /// Falls back to prefixed or GUID-based slugs if generation fails.
    /// </summary>
    /// <param name="nombreRaw">Destination name to base slug on.</param>
    /// <param name="routes">Existing routes whose slugs cannot be reused.</param>
    /// <param name="destinos">Existing destinations whose slugs cannot be reused.</param>
    /// <returns>Unique, valid codigo-format slug.</returns>
    public static string UniqueDestinoSlug(
        string nombreRaw,
        IEnumerable<LogisticsRoute> routes,
        IEnumerable<LogisticsDestination> destinos)
    {
        var baseSlug = SlugFromNombre(nombreRaw);
        if (baseSlug.Length < 2) baseSlug = NormalizeCodigo($"d_{baseSlug}");
        if (!IsValidCodigo(baseSlug)) baseSlug = "destino_x";

        var taken = routes.Select(r => r.Slug)
            .Concat(destinos.Select(d => d.Slug))
            .ToHashSet(StringComparer.Ordinal);

        var candidate = baseSlug;
        for (var n = 0; n < 100; n++)
        {
            if (!taken.Contains(candidate) && IsValidCodigo(candidate))
                return candidate;
            var suffix = $"_{n + 2}";
            var cut = Math.Max(2, 47 - suffix.Length);
            candidate = NormalizeCodigo(baseSlug[..Math.Min(baseSlug.Length, cut)] + suffix);
        }

        return NormalizeCodigo($"dest_{Guid.NewGuid():N}"[..12]);
    }

    /// <summary>
    /// Generates the next destination code: DST-### (zero-padded to 3 digits).
    /// </summary>
    /// <param name="destinos">Existing destinations to determine max code.</param>
    /// <returns>Next sequential destination code.</returns>
    public static string NextDestinationCode(IEnumerable<LogisticsDestination> destinos)
    {
        var max = 0;
        foreach (var d in destinos)
        {
            var m = DstCodeRegex().Match(d.DestinationCode);
            if (m.Success) max = Math.Max(max, int.Parse(m.Groups[1].Value));
        }

        return $"DST-{max + 1:D3}";
    }

    /// <summary>
    /// Generates the next route code: RT# (incrementing integer).
    /// </summary>
    /// <param name="routes">Existing routes to determine max code.</param>
    /// <returns>Next sequential route code.</returns>
    public static string NextRouteCode(IEnumerable<LogisticsRoute> routes)
    {
        var max = 0;
        foreach (var r in routes)
        {
            var m = RtCodeRegex().Match(r.RouteCode);
            if (m.Success) max = Math.Max(max, int.Parse(m.Groups[1].Value));
        }

        return $"RT{max + 1}";
    }

    /// <summary>
    /// Generates the next driver code: DRV-### (zero-padded to 3 digits).
    /// </summary>
    /// <param name="drivers">Existing drivers to determine max code.</param>
    /// <returns>Next sequential driver code.</returns>
    public static string NextDriverCode(IEnumerable<LogisticsDriver> drivers)
    {
        var max = 0;
        foreach (var d in drivers)
        {
            var m = DrvCodeRegex().Match(d.DriverCode);
            if (m.Success) max = Math.Max(max, int.Parse(m.Groups[1].Value));
        }

        return $"DRV-{max + 1:D3}";
    }

    /// <summary>
    /// Generates the next dispatch code: S### (zero-padded to 3 digits).
    /// </summary>
    /// <param name="dispatches">Existing dispatches to determine max code.</param>
    /// <returns>Next sequential dispatch code.</returns>
    public static string NextDispatchCode(IEnumerable<LogisticsDispatch> dispatches)
    {
        var max = 0;
        foreach (var d in dispatches)
        {
            var m = DispatchCodeRegex().Match(d.DispatchCode);
            if (m.Success) max = Math.Max(max, int.Parse(m.Groups[1].Value));
        }

        return $"S{max + 1:D3}";
    }

    /// <summary>
    /// Generates the next carrier code: T# (incrementing integer).
    /// </summary>
    /// <param name="carriers">Existing carriers to determine max code.</param>
    /// <returns>Next sequential carrier code.</returns>
    public static string NextCarrierCode(IEnumerable<LogisticsCarrier> carriers)
    {
        var max = 0;
        foreach (var c in carriers)
        {
            var m = CarrierCodeRegex().Match(c.CarrierCode);
            if (m.Success) max = Math.Max(max, int.Parse(m.Groups[1].Value));
        }

        return $"T{max + 1}";
    }

    /// <summary>
    /// Returns the first non-null, non-whitespace trimmed string from a list of candidates.
    /// </summary>
    /// <param name="values">Candidate strings.</param>
    /// <returns>First valid trimmed string, or null if all are empty.</returns>
    private static string? PickTrimmed(params string?[] values)
    {
        foreach (var v in values)
        {
            if (!string.IsNullOrWhiteSpace(v)) return v.Trim();
        }

        return null;
    }

    /// <summary>Regex: DST-### format.</summary>
    [GeneratedRegex(@"^DST-(\d+)$", RegexOptions.IgnoreCase)]
    private static partial Regex DstCodeRegex();

    /// <summary>Regex: RT# format.</summary>
    [GeneratedRegex(@"^RT(\d+)$", RegexOptions.IgnoreCase)]
    private static partial Regex RtCodeRegex();

    /// <summary>Regex: DRV-### format.</summary>
    [GeneratedRegex(@"^DRV-(\d+)$", RegexOptions.IgnoreCase)]
    private static partial Regex DrvCodeRegex();

    /// <summary>Regex: S### format.</summary>
    [GeneratedRegex(@"^S(\d+)$", RegexOptions.IgnoreCase)]
    private static partial Regex DispatchCodeRegex();

    /// <summary>Regex: T# format.</summary>
    [GeneratedRegex(@"^T(\d+)$", RegexOptions.IgnoreCase)]
    private static partial Regex CarrierCodeRegex();

    /// <summary>Regex: replaces non-alphanumeric characters with underscores.</summary>
    [GeneratedRegex(@"[^a-z0-9]+")]
    private static partial Regex NonAlnumRegex();

    /// <summary>Regex: collapses multiple consecutive underscores.</summary>
    [GeneratedRegex(@"_+")]
    private static partial Regex MultiUnderscoreRegex();
}
