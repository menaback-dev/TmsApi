using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TmsApi.Data;
using TmsApi.Entities;

namespace TmsApi.Controllers;

[ApiController]
[Route("api/reports")]
public class ReportController(TmsDbContext context) : ControllerBase
{
   
    [HttpGet("students")]
    public async Task<IActionResult> GetStudents(int page = 1, int pageSize = 20)
    {
        var students = await context.Students
            .OrderBy(s => s.Name)                    // Must have stable sort
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return Ok(students);
    }


    [HttpGet("top-courses")]
    public async Task<IActionResult> TopCourses()
    {
        var topCourses = await context.Courses
            .Select(c => new
            {
                c.Title,
                EnrollmentCount = c.Enrollments.Count
            })
            .OrderByDescending(x => x.EnrollmentCount)
            .Take(5)
            .ToListAsync();

        return Ok(topCourses);
    }
}