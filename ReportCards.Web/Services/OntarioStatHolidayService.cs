namespace ReportCards.Web.Services;

/// <summary>
/// Calculates Ontario statutory holiday dates for any given calendar year
/// using rule definitions rather than hardcoded dates.
///
/// Rule types supported:
///   ObservedFixed  — fixed month/day, shifted to Monday if it falls on a weekend
///   NthWeekday     — Nth occurrence of a weekday in a month (e.g. 1st Monday of September)
///   EasterOffset   — relative to Easter Sunday (e.g. Good Friday = Easter - 2 days)
///   VictoriaDay    — special case: last Monday before May 25
/// </summary>
public static class OntarioStatHolidayService
{
    public record HolidayResult(DateOnly Date, string Name);

    // ── Rule definitions ───────────────────────────────────────────────────────

    private abstract record HolidayRule(string Name)
    {
        public abstract DateOnly Calculate(int year);
    }

    /// <summary>Same month/day every year. If it falls on a weekend, observed on the nearest Monday.</summary>
    private record ObservedFixedRule(string Name, int Month, int Day) : HolidayRule(Name)
    {
        public override DateOnly Calculate(int year)
        {
            var date = new DateOnly(year, Month, Day);
            return date.DayOfWeek switch
            {
                DayOfWeek.Saturday => date.AddDays(2),  // Saturday → Monday
                DayOfWeek.Sunday   => date.AddDays(1),  // Sunday   → Monday
                _                  => date
            };
        }
    }

    /// <summary>Nth occurrence of a specific weekday in a given month.</summary>
    private record NthWeekdayRule(string Name, int Month, DayOfWeek Weekday, int Occurrence) : HolidayRule(Name)
    {
        public override DateOnly Calculate(int year)
        {
            var first = new DateOnly(year, Month, 1);
            var daysUntil = ((int)Weekday - (int)first.DayOfWeek + 7) % 7;
            return first.AddDays(daysUntil + (Occurrence - 1) * 7);
        }
    }

    /// <summary>Calculated relative to Easter Sunday using the Anonymous Gregorian algorithm.</summary>
    private record EasterOffsetRule(string Name, int DayOffset) : HolidayRule(Name)
    {
        public override DateOnly Calculate(int year)
        {
            return CalculateEaster(year).AddDays(DayOffset);
        }

        /// <summary>Anonymous Gregorian algorithm for Easter Sunday.</summary>
        private static DateOnly CalculateEaster(int year)
        {
            int a = year % 19;
            int b = year / 100;
            int c = year % 100;
            int d = b / 4;
            int e = b % 4;
            int f = (b + 8) / 25;
            int g = (b - f + 1) / 3;
            int h = (19 * a + b - d - g + 15) % 30;
            int i = c / 4;
            int k = c % 4;
            int l = (32 + 2 * e + 2 * i - h - k) % 7;
            int m = (a + 11 * h + 22 * l) / 451;
            int month = (h + l - 7 * m + 114) / 31;
            int day   = ((h + l - 7 * m + 114) % 31) + 1;
            return new DateOnly(year, month, day);
        }
    }

    /// <summary>Victoria Day: last Monday before May 25.</summary>
    private record VictoriaDayRule() : HolidayRule("Victoria Day")
    {
        public override DateOnly Calculate(int year)
        {
            var may25 = new DateOnly(year, 5, 25);
            var d = may25.AddDays(-1);
            while (d.DayOfWeek != DayOfWeek.Monday) d = d.AddDays(-1);
            return d;
        }
    }

    // ── Ontario holiday rules ──────────────────────────────────────────────────
    // Source: https://www.ontario.ca/document/your-guide-employment-standards-act-0/public-holidays

    private static readonly List<HolidayRule> Rules =
    [
        new ObservedFixedRule("New Year's Day",                            1,  1),
        new NthWeekdayRule   ("Family Day",                                2,  DayOfWeek.Monday, 3),
        new EasterOffsetRule ("Good Friday",                                   -2),
        new VictoriaDayRule  (),
        new ObservedFixedRule("Canada Day",                                7,  1),
        new NthWeekdayRule   ("Civic Holiday",                             8,  DayOfWeek.Monday, 1),
        new NthWeekdayRule   ("Labour Day",                                9,  DayOfWeek.Monday, 1),
        new ObservedFixedRule("National Day for Truth and Reconciliation", 9, 30),
        new NthWeekdayRule   ("Thanksgiving Day",                         10,  DayOfWeek.Monday, 2),
        new ObservedFixedRule("Remembrance Day",                          11, 11),
        new ObservedFixedRule("Christmas Day",                            12, 25),
        new ObservedFixedRule("Boxing Day",                               12, 26),
    ];

    // ── Public API ─────────────────────────────────────────────────────────────

    /// <summary>
    /// Returns all Ontario stat holidays for the given calendar year, sorted by date.
    /// Works for any year — no hardcoded dates.
    /// </summary>
    public static List<HolidayResult> GetHolidaysForYear(int year)
    {
        return Rules
            .Select(r => new HolidayResult(r.Calculate(year), r.Name))
            .Where(h => h.Date.Year == year) // guard: filter out dates that shifted into adjacent year
            .OrderBy(h => h.Date)
            .ToList();
    }

    /// <summary>
    /// Returns all Ontario stat holidays across the calendar years covered by a school year name
    /// like "2025-2026", deduplicating any that fall on the same date.
    /// </summary>
    public static List<HolidayResult> GetHolidaysForSchoolYear(string schoolYearName)
    {
        var calendarYears = ParseYears(schoolYearName);
        if (calendarYears.Count == 0) return [];

        return calendarYears
            .SelectMany(GetHolidaysForYear)
            .GroupBy(h => h.Date)
            .Select(g => g.First())
            .OrderBy(h => h.Date)
            .ToList();
    }

    // ── Private helpers ────────────────────────────────────────────────────────

    private static List<int> ParseYears(string name)
    {
        var years = new List<int>();
        foreach (var part in name.Split('-', '/', ' '))
            if (int.TryParse(part.Trim(), out var y) && y >= 2000 && y <= 2100)
                years.Add(y);
        return years.Distinct().Take(2).ToList();
    }
}
