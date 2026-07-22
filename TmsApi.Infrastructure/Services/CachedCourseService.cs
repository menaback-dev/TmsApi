using Microsoft.Extensions.Caching.Hybrid;
using Microsoft.Extensions.Logging;
using TmsApi.Application.Dtos;
using TmsApi.Application.Interfaces;
using TmsApi.Infrastructure.Caching;

namespace TmsApi.Infrastructure.Services;

public class CachedCourseService(
    HybridCache cache,
    ICourseService courseService,
    ILogger<CachedCourseService> logger) : ICachedCourseService
{
    public async Task<PagedResponse<CourseResponseDto>> GetCoursesAsync(
        PagedRequest request,
        CancellationToken ct = default)
    {
        var key = $"{CacheKeys.CoursesAll}:p{request.Page}:s{request.PageSize}:q{request.Search ?? ""}:o{request.OrderBy}:d{request.Descending}";
        var dbHit = false;

        var result = await cache.GetOrCreateAsync(
            key,
            (courseService, request),
            async (state, token) =>
            {
                dbHit = true;
                logger.LogInformation("Cache MISS for {Key} fetching from DB", key);
                return await state.courseService.GetCoursesAsync(state.request, token);
            },
            tags: [CacheKeys.CoursesTag],
            cancellationToken: ct);

        if (!dbHit)
            logger.LogInformation("Cache HIT for {Key}", key);

        return result;
    }

    public async Task InvalidateCourseCacheAsync(CancellationToken ct = default)
    {
        logger.LogInformation("Invalidating cache tag {Tag}", CacheKeys.CoursesTag);
        await cache.RemoveByTagAsync(CacheKeys.CoursesTag, ct);
    }
}