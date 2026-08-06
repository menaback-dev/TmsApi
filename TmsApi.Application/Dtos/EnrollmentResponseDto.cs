namespace TmsApi.Application.Dtos;

public record EnrollmentResponseDto(
    int Id,
    string StudentId,      // RegistrationNumber
    string StudentName,
    string CourseCode,
    string CourseName,
    DateTime EnrolledAt,
    string Status
);