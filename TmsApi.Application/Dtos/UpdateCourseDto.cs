namespace TmsApi.Application.Dtos;

public record UpdateCourseDto(
    string Title
// add fields you allow to change, e.g.:
// string? Code,
// int MaxCapacity
);