using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TmsApi.Data;
using TmsApi.Entities;

namespace TmsApi.Controllers;

[ApiController]
[Route("api/test")]
public class TestController(TmsDbContext context) : ControllerBase
{
    // 1. Deferred Execution Demo
    [HttpGet("deferred")]
    public IActionResult TestDeferred()
    {
        Console.WriteLine("\n>>> STEP 1: Building the query object (no DB hit yet)...");
        var query = context.Students.Where(s => s.GPA >= 3.0m);

        Console.WriteLine(">>> STEP 2: Adding ordering...");
        var orderedQuery = query.OrderBy(s => s.Name);

        Console.WriteLine(">>> STEP 3: Materializing (ToList) → DB query runs now...");
        var results = orderedQuery.ToList();

        Console.WriteLine($">>> STEP 4: Materialization finished. Returned {results.Count} students.\n");

        return Ok(results);
    }

    // 2. Translation Failure Demo
    private static bool IsHonorRoll(decimal gpa) => gpa >= 3.5m;

    [HttpGet("translation-fail")]
    public IActionResult TestTranslationFail()
    {
        Console.WriteLine("\n>>> Testing non-translatable query...");
        try
        {
            var students = context.Students
                .Where(s => IsHonorRoll(s.GPA))   // This will fail translation
                .ToList();

            return Ok(students);
        }
        catch (Exception ex)
        {
            Console.WriteLine($">>> EXCEPTION: {ex.Message}");
            return BadRequest(new { Message = ex.Message });
        }
    }

    // 3. Registrar Business Queries
    [HttpGet("active-high-gpa")]
    public async Task<IActionResult> ActiveHighGPAStudents()
    {
        var count = await context.Students
            .Where(s => s.IsActive && s.GPA >= 3.0m)
            .CountAsync();

        return Ok(new { Count = count });
    }

    [HttpGet("popular-courses")]
    public async Task<IActionResult> PopularCourses()
    {
        var courses = await context.Courses
            .Select(c => new
            {
                c.Title,
                EnrollmentCount = c.Enrollments.Count
            })
            .OrderByDescending(x => x.EnrollmentCount)
            .ToListAsync();

        return Ok(courses);
    }

    [HttpGet("avg-gpa-per-course")]
    public async Task<IActionResult> AvgGpaPerCourse()
    {
        var result = await context.Enrollments
            .GroupBy(e => e.Course.Title)
            .Select(g => new
            {
                Course = g.Key,
                AverageGPA = g.Average(e => e.Student.GPA)
            })
            .ToListAsync();

        return Ok(result);
    }

    [HttpGet("students-no-enrollment")]
    public async Task<IActionResult> StudentsWithNoEnrollment()
    {
        var students = await context.Students
            .Where(s => !s.Enrollments.Any())
            .Select(s => s.Name)
            .ToListAsync();

        return Ok(students);
    }

    [HttpGet("nplus1")]
    public async Task<IActionResult> ShowNPlusOne()
    {
        var students = await context.Students.AsNoTracking().ToListAsync();

        foreach (var s in students)
        {
            var count = await context.Enrollments
                .AsNoTracking()
                .CountAsync(e => e.StudentId == s.Id);

            Console.WriteLine($"{s.Name} has {count} enrollments");
        }

        return Ok("Check console for N+1 queries");
    }

    [HttpGet("nplus1-fixed")]
    public async Task<IActionResult> FixedNPlusOne()
    {
        var report = await context.Students
            .AsNoTracking()
            .Select(s => new
            {
                s.Name,
                EnrollmentCount = s.Enrollments.Count
            })
            .ToListAsync();

        foreach (var r in report)
        {
            Console.WriteLine($"{r.Name} has {r.EnrollmentCount} enrollments");
        }

        return Ok(report);
    }

    [HttpPost("archive-old")]
    public async Task<IActionResult> ArchiveOldEnrollments()
    {
        var cutoff = DateTime.UtcNow.AddYears(-1);

        var rowsAffected = await context.Enrollments
            .Where(e => e.EnrolledAt < cutoff)
            .ExecuteUpdateAsync(s => s.SetProperty(e => e.IsArchived, true));

        return Ok(new { ArchivedCount = rowsAffected });
    }
    }