using System.ComponentModel.DataAnnotations;

namespace TmsApi.Dtos;

public record EnrollStudentRequest
{
    [Range(1, int.MaxValue)]
    public required int StudentId { get; init; }
};