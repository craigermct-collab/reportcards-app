namespace ReportCards.Web.Data;

public class Teacher
{
    public int Id { get; set; }
    public required string DisplayName { get; set; }
    public required string Email { get; set; }

    public List<Class> Classes { get; set; } = new();
}

public class Student
{
    public int Id { get; set; }
    public required string FirstName { get; set; }
    public required string LastName { get; set; }
    public DateOnly? DateOfBirth { get; set; }

    public List<Enrollment> Enrollments { get; set; } = new();
}

public class Class
{
    public int Id { get; set; }
    public required string Name { get; set; }
    public string? SchoolYear { get; set; }

    public int TeacherId { get; set; }
    public Teacher? Teacher { get; set; }

    public List<Enrollment> Enrollments { get; set; } = new();
}

public class Enrollment
{
    public int Id { get; set; }

    public int StudentId { get; set; }
    public Student? Student { get; set; }

    public int ClassId { get; set; }
    public Class? Class { get; set; }

    public DateTimeOffset EnrolledOn { get; set; } = DateTimeOffset.UtcNow;
}