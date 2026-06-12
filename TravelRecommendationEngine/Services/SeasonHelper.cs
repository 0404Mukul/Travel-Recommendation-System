using System.Globalization;

namespace TravelRecommendationEngine.Services;

internal static class SeasonHelper
{
    private static readonly string[] MonthAbbreviations =
        ["Jan", "Feb", "Mar", "Apr", "May", "Jun", "Jul", "Aug", "Sep", "Oct", "Nov", "Dec"];

    public static bool IsInBestSeason(string bestTimeToVisit, DateOnly? referenceDate = null)
    {
        if (string.IsNullOrWhiteSpace(bestTimeToVisit))
        {
            return true;
        }

        var currentMonth = (referenceDate ?? DateOnly.FromDateTime(DateTime.UtcNow)).Month;
        var ranges = bestTimeToVisit.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

        foreach (var range in ranges)
        {
            if (IsMonthInRange(currentMonth, range))
            {
                return true;
            }
        }

        return false;
    }

    private static bool IsMonthInRange(int month, string range)
    {
        var parts = range.Split('-', StringSplitOptions.TrimEntries);
        if (parts.Length != 2)
        {
            return false;
        }

        if (!TryParseMonth(parts[0], out var startMonth) || !TryParseMonth(parts[1], out var endMonth))
        {
            return false;
        }

        return startMonth <= endMonth
            ? month >= startMonth && month <= endMonth
            : month >= startMonth || month <= endMonth;
    }

    private static bool TryParseMonth(string value, out int month)
    {
        month = 0;
        for (var index = 0; index < MonthAbbreviations.Length; index++)
        {
            if (MonthAbbreviations[index].Equals(value, StringComparison.OrdinalIgnoreCase))
            {
                month = index + 1;
                return true;
            }
        }

        return int.TryParse(value, NumberStyles.Integer, CultureInfo.InvariantCulture, out month)
            && month is >= 1 and <= 12;
    }
}
