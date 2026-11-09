using S100FC.S101.ComplexAttributes;
using S100Framework.ImporterNIS.Core;

namespace S100Framework.Applications;

public static class DateHelper
{
    public static bool TryConvertSordat(string value, out string? result) =>
        S57DateParser.TryParseReportedDate(value, out result);

    public static bool TryConvertToDateOnly(string? dateString, out DateOnly dateOnly) =>
        S57DateParser.TryParseDate(dateString, out dateOnly);

    internal static bool TryGetPeriodicDateRange(string? start, string? end, out periodicDateRange[]? value) {
        value = null;
        if (!S57DateParser.TryParsePeriodicDate(start, out var parsedStart) ||
            !S57DateParser.TryParsePeriodicDate(end, out var parsedEnd)) return false;
        value = [new periodicDateRange { dateStart = parsedStart!, dateEnd = parsedEnd! }];
        return true;
    }

    internal static bool TryGetSurveyDateRange(string? start, string? end, out surveyDateRange? value) {
        value = null;
        if (!S57DateParser.TryParseCalendarDate(start, out var parsedStart)) return false;
        value = new surveyDateRange { dateStart = parsedStart! };
        // Preserve an unknown survey end rather than inventing a date.
        if (S57DateParser.TryParseCalendarDate(end, out var parsedEnd)) value.dateEnd = parsedEnd!;
        return true;
    }

    internal static bool TryGetFixedDateRange(string? start, string? end, out fixedDateRange? value) {
        value = null;
        if (!S57DateParser.TryParseCalendarDate(start, out var parsedStart) ||
            !S57DateParser.TryParseCalendarDate(end, out var parsedEnd)) return false;
        value = new fixedDateRange { dateStart = parsedStart!, dateEnd = parsedEnd! };
        return true;
    }
}
