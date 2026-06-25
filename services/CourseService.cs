using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Linq;

public interface ICourseService
{
    Task<CourseRecord> CreateAsync(CreateCourseRequest request);
    Task<CourseRecord?> GetByCodeAsync(string code);
    Task<IReadOnlyList<CourseRecord>> GetAllAsync();
    Task<bool> DeleteAsync(string code);
}

public class CourseService : ICourseService
{
    private static readonly Dictionary<string, CourseRecord> _store = new();
    private readonly ILogger<CourseService> _logger;

    public CourseService(ILogger<CourseService> logger)
    {
        _logger = logger;
    }

    public Task<CourseRecord> CreateAsync(CreateCourseRequest request)
    {
        if (_store.ContainsKey(request.Code))
        {
            _logger.LogWarning("Course with code {CourseCode} already exists", request.Code);
            return Task.FromResult(_store[request.Code]);
        }

        var course = new CourseRecord(request.Code, request.Title, request.Description, DateTime.UtcNow);
        _store[request.Code] = course;

        _logger.LogInformation("Created course {CourseCode} - {Title}", request.Code, request.Title);
        return Task.FromResult(course);
    }

    public Task<CourseRecord?> GetByCodeAsync(string code)
    {
        _store.TryGetValue(code, out var course);
        if (course is null)
        {
            _logger.LogWarning("Course {CourseCode} not found", code);
        }
        return Task.FromResult(course);
    }

    public Task<IReadOnlyList<CourseRecord>> GetAllAsync()
    {
        return Task.FromResult<IReadOnlyList<CourseRecord>>(_store.Values.ToList());
    }

    public Task<bool> DeleteAsync(string code)
    {
        var removed = _store.Remove(code);
        if (removed)
            _logger.LogInformation("Deleted course {CourseCode}", code);
        else
            _logger.LogWarning("Delete failed - course {CourseCode} not found", code);

        return Task.FromResult(removed);
    }
}

public record CreateCourseRequest(string Code, string Title, string Description);

public record CourseRecord(
    string Code,
    string Title,
    string Description,
    DateTime CreatedAt
);