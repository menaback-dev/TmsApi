using System.ComponentModel.DataAnnotations;

namespace TmsApi.Dtos;

public record CreateCourseRequest
{
    [Required(ErrorMessage = "Code is required")]
    [RegularExpression(@"^[A-Z]{3}-\d{3}$", ErrorMessage = "Code must follow format XXX-000 (e.g. CSE-101)")]
    public required string Code { get; init; }

    [Required(ErrorMessage = "Title is required")]
    [MaxLength(200, ErrorMessage = "Title cannot exceed 200 characters")]
    public required string Title { get; init; }

    [Range(1, 200, ErrorMessage = "MaxCapacity must be between 1 and 200")]
    public int MaxCapacity { get; init; }
}