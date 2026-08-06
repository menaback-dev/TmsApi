using TmsApi.Application.Dtos;
using TmsApi.Domain.Entities;

namespace TmsApi.Application.Interfaces;

public interface IEnrollmentService
{
    // API methods
    Task<IReadOnlyList<EnrollmentResponseDto>> GetAllAsync(CancellationToken ct = default);
    Task<EnrollmentResponseDto?> GetByIdAsync(string id, CancellationToken ct = default);
    Task<EnrollmentResponseDto> EnrollAsync(string studentId, string courseCode, CancellationToken ct = default);
    Task<bool> DeleteAsync(string id, CancellationToken ct = default);
    Task<bool> ApproveAsync(string id, CancellationToken ct = default);

    // CQRS methods
    Task<bool> ExistsAsync(int studentId, string courseCode, CancellationToken ct = default);
    Task AddAsync(Enrollment enrollment, CancellationToken ct = default);
    Task<IReadOnlyList<Enrollment>> GetByStudentIdAsync(int studentId, CancellationToken ct = default);
}