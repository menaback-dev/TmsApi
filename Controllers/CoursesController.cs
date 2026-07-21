using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using TmsApi.Dtos;
using TmsApi.Services;

namespace TmsApi.Controllers;

[ApiController]
[Route("api/courses")]
[Tags("Courses")]
[Produces("application/json")]
[ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
public class CoursesController(ICourseService courseService, LinkGenerator linkGenerator) : ControllerBase
{
    [HttpGet("{id:int}", Name = nameof(GetCourseById))]
    [ProducesResponseType(typeof(CourseDetailDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [EndpointSummary("Get a course by ID")]
    [EndpointDescription("Returns course details with HATEOAS links. Returns 404 if the course does not exist.")]
    public async Task<IActionResult> GetCourseById(int id, CancellationToken ct = default)
    {
        var course = await courseService.GetByIdAsync(id, ct);
        if (course is null) 
            return NotFound();

        var links = new List<LinkDto>();

        links.Add(new LinkDto(
            linkGenerator.GetPathByName(HttpContext, nameof(GetCourseById), new { id })!,
            "self", "GET"));

        links.Add(new LinkDto(
            linkGenerator.GetPathByName(HttpContext, nameof(GetCourseById), new { id })!,
            "update", "PUT"));

        links.Add(new LinkDto(
            linkGenerator.GetPathByName(HttpContext, nameof(GetCourseById), new { id })!,
            "delete", "DELETE"));

    
        links.Add(new LinkDto(
            linkGenerator.GetPathByAction(HttpContext, "GetEnrollments", "Enrollments", new { courseId = id })!,
            "enrollments", "GET"));

    
        if (course.EnrollmentCount < course.MaxCapacity)
        {
            links.Add(new LinkDto(
                linkGenerator.GetPathByAction(HttpContext, "EnrollStudent", "Enrollments", new { courseId = id })!,
                "enroll", "POST"));
        }

        var detail = new CourseDetailDto
        {
            Id = course.Id,
            Code = course.Code,
            Title = course.Title,
            MaxCapacity = course.MaxCapacity,
            EnrollmentCount = course.EnrollmentCount,
            Links = links
        };

        return Ok(detail);
    }

    [HttpPost]
    [ProducesResponseType(typeof(CourseResponseDto), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.
    Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    [EndpointSummary("Create a new course")]
    [EndpointDescription("Creates a course with a unique code. Returns409 if the course code already exists.")]
    public async Task<IActionResult> CreateCourse(CreateCourseRequest request, CancellationToken ct = default)
    {
        var result = await courseService.CreateAsync(request, ct);
        return CreatedAtAction(nameof(GetCourseById), new { id = result.Id }, result);
    }

    [HttpGet]
    [ProducesResponseType(typeof(PagedResponse<CourseResponseDto>), StatusCodes.Status200OK)]
    [EndpointSummary("List courses with pagination")]
    [EndpointDescription("Returns a paginated, optionally filtered list of TMS courses. PageSize is capped at 50.")]
    public async Task<IActionResult> GetCourses([FromQuery] PagedRequest request, CancellationToken ct = default)
    {
        var result = await courseService.GetCoursesAsync(request, ct);
        return Ok(result);
    }

    
}