using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;
using TmsApi.Application.Dtos;
using TmsApi.Infrastructure.Persistence;

namespace TmsApi.Api.Controllers.V1;

[ApiController]
[Route("api/v{version:apiVersion}/courses")]
[ApiVersion("1.0")]

public class CoursesController : ControllerBase
{
    private readonly TmsDbContext _context;
    private readonly IAuthorizationService _authorizationService;

    public CoursesController(TmsDbContext context, IAuthorizationService authorizationService)
    {
        _context = context;
        _authorizationService = authorizationService;
    }

    [HttpGet]
    public async Task<IActionResult> GetCourses(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken ct = default
    )
    {
        page = Math.Max(1, page = 1);
        pageSize = Math.Clamp(pageSize, 1, 50);

        var baseQuery = _context.Courses.AsNoTracking();
        var totalCount = await baseQuery.CountAsync(ct);
        var items = await baseQuery
            .OrderBy(c => c.Title)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(c => new
            {
                c.Id,
                c.Code,
                c.Title,
                c.MaxCapacity,
                c.InstructorId,
                EnrollmentCount = c.Enrollments.Count
            })
            .ToListAsync(ct);
        var totalPages = (int)Math.Ceiling(totalCount / (double)pageSize);

        return Ok(new
        {
            items,
            totalCount,
            page,
            pageSize,
            totalPages,
            hasNext = page < totalPages,
            hasPrevious = page > 1
        });
    }

    [Authorize(Roles = "Instructor,Admin")]
    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateCourse(int id, [FromBody] UpdateCourseDto dto)
    {
        var course = await _context.Courses.FindAsync(id);
        if (course is null) return NotFound();

        var authResult = await _authorizationService.AuthorizeAsync(User, course, "CanEditCourse");
        if (!authResult.Succeeded)
            return Forbid();

        course.Title = dto.Title;
        await _context.SaveChangesAsync();
        return NoContent();
    }

    [ApiController]
    [Route("api/v2/transcripts")]
    public class TranscriptsController : ControllerBase
    {
        [HttpPost]
        [EnableRateLimiting("transcripts")]
        public IActionResult RequestTranscript([FromBody] object? _)
        {
            return Ok();
        }
    }
}
