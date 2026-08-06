using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using TmsApi.Application.Dtos;
using TmsApi.Application.Interfaces;
using TmsApi.Domain.Entities;
using TmsApi.Infrastructure.Persistence;

namespace TmsApi.Infrastructure.Services;

public class EnrollmentService(
    TmsDbContext context,
    ILogger<EnrollmentService> logger) : IEnrollmentService
{
    public async Task<IReadOnlyList<EnrollmentResponseDto>> GetAllAsync(CancellationToken ct = default)
    {
        return await context.Enrollments
            .AsNoTracking()
            .Select(e => new EnrollmentResponseDto(
                e.Id,
                e.Student.RegistrationNumber,
                e.Student.Name,
                e.Course.Code,
                e.Course.Title,
                e.EnrolledAt,
                e.Status))
            .ToListAsync(ct);
    }

    public async Task<EnrollmentResponseDto?> GetByIdAsync(string id, CancellationToken ct = default)
    {
        if (!int.TryParse(id, out var enrollmentId))
            return null;

        return await context.Enrollments
            .AsNoTracking()
            .Where(e => e.Id == enrollmentId)
            .Select(e => new EnrollmentResponseDto(
                e.Id,
                e.Student.RegistrationNumber,
                e.Student.Name,
                e.Course.Code,
                e.Course.Title,
                e.EnrolledAt,
                e.Status))
            .FirstOrDefaultAsync(ct);
    }

    public async Task<EnrollmentResponseDto> EnrollAsync(
        string studentId,
        string courseCode,
        CancellationToken ct = default)
    {
        var student = await context.Students
            .FirstOrDefaultAsync(s => s.RegistrationNumber == studentId, ct)
            ?? throw new InvalidOperationException($"Student '{studentId}' not found.");

        var course = await context.Courses
            .Include(c => c.Enrollments)
            .FirstOrDefaultAsync(c => c.Code == courseCode, ct)
            ?? throw new InvalidOperationException($"Course '{courseCode}' not found.");

        var alreadyExists = await context.Enrollments
            .AnyAsync(e => e.StudentId == student.Id && e.CourseId == course.Id, ct);

        if (alreadyExists)
        {
            logger.LogWarning(
                "Duplicate enrollment attempt {StudentId} already in {CourseCode}",
                studentId, courseCode);

            return await context.Enrollments
                .Where(e => e.StudentId == student.Id && e.CourseId == course.Id)
                .Select(e => new EnrollmentResponseDto(
                    e.Id,
                    student.RegistrationNumber,
                    student.Name,
                    course.Code,
                    course.Title,
                    e.EnrolledAt,
                    e.Status))
                .FirstAsync(ct);
        }

        if (course.Enrollments.Count >= course.MaxCapacity)
            throw new InvalidOperationException($"Course '{course.Title}' is full.");

        var enrollment = new Enrollment
        {
            StudentId = student.Id,
            CourseId = course.Id,
            EnrolledAt = DateTime.UtcNow,
            Status = "Pending"
        };

        context.Enrollments.Add(enrollment);
        await context.SaveChangesAsync(ct);

        logger.LogInformation(
            "Enrolled {StudentId} in {CourseCode} record {EnrollmentId}",
            studentId, courseCode, enrollment.Id);

        return new EnrollmentResponseDto(
            enrollment.Id,
            student.RegistrationNumber,
            student.Name,
            course.Code,
            course.Title,
            enrollment.EnrolledAt,
            enrollment.Status);
    }

    public async Task<bool> DeleteAsync(string id, CancellationToken ct = default)
    {
        if (!int.TryParse(id, out var enrollmentId))
            return false;

        var enrollment = await context.Enrollments.FindAsync([enrollmentId], ct);
        if (enrollment is null)
            return false;

        context.Enrollments.Remove(enrollment);
        await context.SaveChangesAsync(ct);
        return true;
    }

    public async Task<bool> ApproveAsync(string id, CancellationToken ct = default)
    {
        if (!int.TryParse(id, out var enrollmentId))
            return false;

        var enrollment = await context.Enrollments.FindAsync([enrollmentId], ct);
        if (enrollment is null)
            return false;

        enrollment.Status = "Approved";
        await context.SaveChangesAsync(ct);
        return true;
    }

    // ---------- CQRS ----------

    public async Task<bool> ExistsAsync(int studentId, string courseCode, CancellationToken ct = default)
    {
        return await context.Enrollments
            .AnyAsync(e => e.StudentId == studentId && e.Course.Code == courseCode, ct);
    }

    public async Task AddAsync(Enrollment enrollment, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(enrollment.Status))
            enrollment.Status = "Pending";

        if (enrollment.EnrolledAt == default)
            enrollment.EnrolledAt = DateTime.UtcNow;

        context.Enrollments.Add(enrollment);
        await context.SaveChangesAsync(ct);
    }

    public async Task<IReadOnlyList<Enrollment>> GetByStudentIdAsync(int studentId, CancellationToken ct = default)
    {
        return await context.Enrollments
            .Include(e => e.Course)
            .AsNoTracking()
            .Where(e => e.StudentId == studentId)
            .ToListAsync(ct);
    }
}

public class TmsDatabaseException(string message) : Exception(message) { }