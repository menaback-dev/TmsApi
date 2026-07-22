using Asp.Versioning;
using Microsoft.AspNetCore.Mvc;
using TmsApi.Application.Dtos;
using TmsApi.Application.Interfaces;

namespace TmsApi.Api.Controllers.V2;

[ApiController]
[Route("api/v{version:apiVersion}/courses")]
[ApiVersion("2.0")]
public class CoursesController(ICachedCourseService cachedCourseService) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetCourses(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        [FromQuery] string? search = null,
        [FromQuery] string orderBy = "Title",
        [FromQuery] bool descending = false,
        CancellationToken ct = default)
    {
        // Build the same PagedRequest your service expects
        var request = new PagedRequest
        {
            Page = page,
            PageSize = pageSize,
            Search = search,
            OrderBy = orderBy,
            Descending = descending
        };

        // This call is now cached + stampede-protected
        var result = await cachedCourseService.GetCoursesAsync(request, ct);

        // Map to the V2 envelope shape (data / meta / links)
        return Ok(new
        {
            data = result.Items,
            meta = new
            {
                totalCount = result.TotalCount,
                page = result.Page,
                pageSize = result.PageSize,
                totalPages = result.TotalPages,
                hasNext = result.HasNext,
                hasPrevious = result.HasPrevious
            },
            links = new
            {
                self = $"/api/v2/courses?page={result.Page}&pageSize={result.PageSize}",
                next = result.HasNext
                    ? $"/api/v2/courses?page={result.Page + 1}&pageSize={result.PageSize}"
                    : (string?)null,
                prev = result.HasPrevious
                    ? $"/api/v2/courses?page={result.Page - 1}&pageSize={result.PageSize}"
                    : (string?)null,
                enroll = "/api/v2/enrollments"
            }
        });
    }
}