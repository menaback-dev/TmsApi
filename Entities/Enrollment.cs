using System;

namespace TmsApi.Entities;

public class Enrollment
{
    public int Id  { get; set;}
    public int StudentId { get; set; }
    public int CourseId { get; set; }
    public decimal? Grade { get; set; }
    public DateTime EnrolledAt { get; set; } = DateTime.UtcNow;

    public Student Student { get; set; } = null!;
    public Course Course {get; set; } = null!;

    public bool IsArchived {get; set; } = false;
}

