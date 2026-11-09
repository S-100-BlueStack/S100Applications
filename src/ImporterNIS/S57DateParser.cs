using System.Globalization;

namespace S100Framework.ImporterNIS.Core;

public static class S57DateParser
{
    public static bool TryParseDate(string? value, out DateOnly date) =>
        DateOnly.TryParseExact(value, "yyyyMMdd", CultureInfo.InvariantCulture, DateTimeStyles.None, out date);

    public static bool TryParseReportedDate(string? value, out string? result) {
        result = null;
        value = value?.Trim();
        if (string.IsNullOrEmpty(value))
            return false;

        if (value?.Length == 4)
            result = value;
        else if (value?.Length < 8)
            value = $"--{value!}";

        result = value!;
        return true;

        ////if (value?.Length < 8)
        ////    value = $"--{value!}";

        ////result = null;
        ////if (value is null || value.Length is not (4 or 6 or 8) || value.Any(c => c is < '0' or > '9'))
        ////    return false;

        ////if (!int.TryParse(value.AsSpan(0, 4), out int year) || year is < 1 or > 9999)
        ////    return false;
        ////if (value.Length >= 6 && (!int.TryParse(value.AsSpan(4, 2), out int month) || month is < 1 or > 12))
        ////    return false;
        ////if (value.Length == 8 && !TryParseDate(value, out _))
        ////    return false;

        ////result = value.PadRight(8, '-');
        ////return true;
    }
    public static bool TryParseCalendarDate(string? value, out string? result) {
        if (value is { Length: 8 } && value.EndsWith("----", StringComparison.Ordinal))
            return TryParseReportedDate(value[..4], out result);
        if (value is { Length: 8 } && value.EndsWith("--", StringComparison.Ordinal))
            return TryParseReportedDate(value[..6], out result);
        return TryParseReportedDate(value, out result);
    }

    public static bool TryParsePeriodicDate(string? value, out string? result) {
        result = null;
        value = value?.Trim();
        if (string.IsNullOrEmpty(value))
            return false;

        if (value?.Length == 4)
            result = value;
        else if (value?.Length < 8)
            value = $"--{value!}";

        result = value!;
        return true;

        ////result = null;
        ////string? digits = value is { Length: 6 } && value.StartsWith("--", StringComparison.Ordinal)
        ////    ? value[2..] : value;
        ////// Use a leap year because a recurring February 29 is a valid month/day.
        ////if (digits is not { Length: 4 } || !TryParseDate("2000" + digits, out _))
        ////    return false;
        ////result = "--" + digits;
        ////return true;
    }
}
