namespace TmsApi.Application.Common;

public static class StudentRegistrationNumbers
{
    public static string Next(IEnumerable<string> existing, int? year = null)
    {
        year ??= DateTime.UtcNow.Year;
        var prefix = $"TMS-{year}-";

        var max = existing
            .Where(r => r.StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
            .Select(r => int.TryParse(r.AsSpan(prefix.Length), out var n) ? n : 0)
            .DefaultIfEmpty(0)
            .Max();

        return $"{prefix}{(max + 1).ToString("D4")}";
    }
}