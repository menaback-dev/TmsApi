using Microsoft.AspNetCore.Mvc;
using TmsApi.Application.Dtos;
using TmsApi.Application.Interfaces;
using Microsoft.AspNetCore.SignalR;
using TmsApi.Api.Hubs;
using TmsApi.Application.Hubs;

namespace TmsApi.Api.Controllers;

[ApiController]
[Route("api/enrollments")]
[Tags("Enrollments")]
[Produces("application/json")]
[ProducesResponseType(typeof(ProblemDetails),StatusCodes.Status500InternalServerError)]
public class EnrollmentController(
    IEnrollmentService enrollmentService,
    IHubContext<TmsHub, ITmsHubClient> hubContext) : ControllerBase
{
    [HttpGet(Name = "ListCourseEnrollments")]
    [ProducesResponseType(typeof(IReadOnlyList<EnrollmentResponseDto>),StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [EndpointSummary("List enrollmemnts for a course")]

    public async Task<IActionResult> GetAll()
    {
        var enrollments = await enrollmentService.GetAllAsync();
        return Ok(enrollments);
    }

    [HttpGet("{id}", Name = nameof(GetById))]
    [ProducesResponseType(typeof(EnrollmentResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [EndpointSummary("Get one enrolment for a course")]
    public async Task<IActionResult> GetById(string id)
    {
        var record = await enrollmentService.GetByIdAsync(id);
        return record is not null? Ok(record) : NotFound();
    }

    [HttpPost]
    [ProducesResponseType(typeof(EnrollmentResponseDto), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    [EndpointSummary("Enroll a student in a course")]
    [EndpointDescription("Returns 404 if the course does not exist, 409if the course has reached MaxCapacity.")]
    public async Task<IActionResult> Create([FromBody] CreateEnrollmentRequest request)
    {
        var record =  await enrollmentService.EnrollAsync(request.StudentId, request.CourseCode);
        return CreatedAtAction(nameof(GetById), new {id= record.Id}, record);
    }

    [HttpDelete("{id}", Name = nameof(Delete))]
    [ProducesResponseType(typeof(EnrollmentResponseDto), StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [EndpointSummary("Delete one enrollment for a course")]
    public async Task<IActionResult> Delete(string id)
    {
        var deleted = await enrollmentService.DeleteAsync(id);
        return deleted ? NoContent() : NotFound();
    }

    [HttpPost("{id}/approve")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [EndpointSummary("Approve an enrollment")]
    public async Task<IActionResult> Approve(string id, CancellationToken ct)
    {
        var ok = await enrollmentService.ApproveAsync(id, ct);
        if (!ok) return NotFound();

        await hubContext.Clients.All
            .ReceiveEnrollmentStatusUpdated(id, "Approved");

        return NoContent();
    }
}

public record CreateEnrollmentRequest(string StudentId, string CourseCode);