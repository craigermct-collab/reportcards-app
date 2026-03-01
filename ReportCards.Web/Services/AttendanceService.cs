using Microsoft.EntityFrameworkCore;
using ReportCards.Web.Data;

namespace ReportCards.Web.Services;

public record AttendanceSummary(int EligibleDays, int Absences, int Lates);

public class AttendanceService(SchoolDbContext db)
{
    /// <summary>
    /// All weekdays within the school year's term date ranges, minus calendar exceptions.
    /// </summary>
    public async Task<List<DateOnly>> GetEligibleSchoolDaysAsync(int schoolYearId)
    {
        var terms = await db.TermInstances
            .Where(t => t.SchoolYearId == schoolYearId)
            .ToListAsync();

        var exceptions = (await db.SchoolCalendarExceptions
            .Where(e => e.SchoolYearId == schoolYearId)
            .Select(e => e.Date)
            .ToListAsync()).ToHashSet();

        var days = new List<DateOnly>();
        foreach (var term in terms)
        {
            var d = term.StartDate;
            while (d <= term.EndDate)
            {
                if (d.DayOfWeek is not DayOfWeek.Saturday and not DayOfWeek.Sunday
                    && !exceptions.Contains(d))
                    days.Add(d);
                d = d.AddDays(1);
            }
        }

        return days.Distinct().Order().ToList();
    }

    /// <summary>
    /// Attendance summary for one student across all terms of a school year.
    /// </summary>
    public async Task<AttendanceSummary> GetSummaryAsync(int studentId, int schoolYearId)
    {
        var eligibleDays = await GetEligibleSchoolDaysAsync(schoolYearId);

        var events = await db.AttendanceEvents
            .Where(a => a.StudentId == studentId && eligibleDays.Contains(a.Date))
            .ToListAsync();

        return new AttendanceSummary(
            EligibleDays: eligibleDays.Count,
            Absences:     events.Count(e => e.Type == AttendanceType.Absent),
            Lates:        events.Count(e => e.Type == AttendanceType.Late)
        );
    }

    /// <summary>
    /// Attendance summary for one student within a single term.
    /// </summary>
    public async Task<AttendanceSummary> GetTermSummaryAsync(int studentId, int termInstanceId)
    {
        var term = await db.TermInstances.FindAsync(termInstanceId);
        if (term is null) return new AttendanceSummary(0, 0, 0);

        var exceptions = (await db.SchoolCalendarExceptions
            .Where(e => e.SchoolYearId == term.SchoolYearId
                     && e.Date >= term.StartDate
                     && e.Date <= term.EndDate)
            .Select(e => e.Date)
            .ToListAsync()).ToHashSet();

        var eligibleDays = new List<DateOnly>();
        var d = term.StartDate;
        while (d <= term.EndDate)
        {
            if (d.DayOfWeek is not DayOfWeek.Saturday and not DayOfWeek.Sunday
                && !exceptions.Contains(d))
                eligibleDays.Add(d);
            d = d.AddDays(1);
        }

        var events = await db.AttendanceEvents
            .Where(a => a.StudentId == studentId && eligibleDays.Contains(a.Date))
            .ToListAsync();

        return new AttendanceSummary(
            EligibleDays: eligibleDays.Count,
            Absences:     events.Count(e => e.Type == AttendanceType.Absent),
            Lates:        events.Count(e => e.Type == AttendanceType.Late)
        );
    }
}
