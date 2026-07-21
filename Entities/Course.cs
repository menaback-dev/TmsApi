namespace TmsApi.Entities;

public class Course
{
    public int Id { get; set; }
    public required string Code { get; set;}
    public required string Title { get; set; }
    public int MaxCapacity { get; set;}

    public ICollection<Enrollment> Enrollments { get; set;} = new List<Enrollment>();
    public ICollection<Assessment> Assessments { get; set; } = new List<Assessment>();
    public ICollection<Certificate> Certificates { get; set; } = new List<Certificate>();
}