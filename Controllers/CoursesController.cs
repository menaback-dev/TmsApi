using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("api/courses")]
public class CoursesController(ICourseService courseService) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetAll() 
        => Ok(await courseService.GetAllAsync());

    [HttpGet("{code}")]
    public async Task<IActionResult> GetByCode(string code)
    {
        var course = await courseService.GetByCodeAsync(code);
        return course is not null ? Ok(course) : NotFound();
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateCourseRequest request)
    {
        var course = await courseService.CreateAsync(request);
        return CreatedAtAction(nameof(GetByCode), new { code = course.Code }, course);
    }

    [HttpDelete("{code}")]
    public async Task<IActionResult> Delete(string code)
    {
        var deleted = await courseService.DeleteAsync(code);
        return deleted ? NoContent() : NotFound();
    }
}