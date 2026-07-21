using Microsoft.EntityFrameworkCore;
using TmsApi.Data;
using TmsApi.Dtos;
using TmsApi.Entities;

namespace TmsApi.Services;

public class CourseService(TmsDbContext context, ILogger<CourseService> logger) : ICourseService
{
    public async Task<CourseResponseDto?> GetByIdAsync(int id, CancellationToken ct = default)
    {
        return await context.Courses
            .AsNoTracking()
            .Where(c => c.Id == id)
            .Select(c => new CourseResponseDto(
                c.Id,
                c.Code,
                c.Title,
                c.MaxCapacity,
                c.Enrollments.Count))
            .FirstOrDefaultAsync(ct);
    }

    public async Task<CourseResponseDto> CreateAsync(CreateCourseRequest request, CancellationToken ct = default)
    {
    if (await CodeExistsAsync(request.Code, ct))
    {
        throw new InvalidOperationException($"A course with code '{request.Code}' already exists.");
    }

    var course = new Course
    {
        Code = request.Code,
        Title = request.Title,
        MaxCapacity = request.MaxCapacity
    };

    context.Courses.Add(course);
    await context.SaveChangesAsync(ct);

    logger.LogInformation("Created course {Id} with code {Code}", course.Id, course.Code);

    return (await GetByIdAsync(course.Id, ct))!;
    }

    public Task<bool> CodeExistsAsync(string code, CancellationToken ct = default)
    {
        return context.Courses.AsNoTracking()
            .AnyAsync(c => c.Code == code, ct);
    }

    public async Task<PagedResponse<CourseResponseDto>> GetCoursesAsync(PagedRequest request, CancellationToken ct = default)
{
    IQueryable<Course> query = context.Courses.AsNoTracking();

    
    if (!string.IsNullOrWhiteSpace(request.Search))
    {
        var searchTerm = $"%{request.Search}%";
        query = query.Where(c => EF.Functions.ILike(c.Title, searchTerm) ||
                                 EF.Functions.ILike(c.Code, searchTerm));
    }

    
    var totalCount = await query.CountAsync(ct);

    
    query = request.OrderBy?.ToLower() switch
    {
        "code" => request.Descending 
            ? query.OrderByDescending(c => c.Code) 
            : query.OrderBy(c => c.Code),
        "maxcapacity" => request.Descending 
            ? query.OrderByDescending(c => c.MaxCapacity) 
            : query.OrderBy(c => c.MaxCapacity),
        _ => request.Descending 
            ? query.OrderByDescending(c => c.Title) 
            : query.OrderBy(c => c.Title)
    };

    
    var items = await query
        .Skip((request.Page - 1) * request.PageSize)
        .Take(request.PageSize)
        .Select(c => new CourseResponseDto(
            c.Id,
            c.Code,
            c.Title,
            c.MaxCapacity,
            c.Enrollments.Count))
        .ToListAsync(ct);

    return new PagedResponse<CourseResponseDto>
    {
        Items = items,
        TotalCount = totalCount,
        Page = request.Page,
        PageSize = request.PageSize
    };
    }
}