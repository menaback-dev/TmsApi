using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Linq;

public interface IStudentService
{
    Task<StudentRecord> CreateAsync(CreateStudentRequest request);
    Task<StudentRecord?> GetByIdAsync(string id);
    Task<IReadOnlyList<StudentRecord>> GetAllAsync();
    Task<bool> DeleteAsync(string id);
}

public class StudentService : IStudentService
{
    private static readonly Dictionary<string, StudentRecord> _store = new();
    private readonly ILogger<StudentService> _logger;

    public StudentService(ILogger<StudentService> logger)
    {
        _logger = logger;
    }

    public Task<StudentRecord> CreateAsync(CreateStudentRequest request)
    {
        var existing = _store.Values.FirstOrDefault(s => s.Email == request.Email);
        if (existing is not null)
        {
            _logger.LogWarning("Student with email {Email} already exists (ID: {StudentId})", 
                request.Email, existing.Id);
            return Task.FromResult(existing);
        }

        var id = Guid.NewGuid().ToString("N")[..8];
        var student = new StudentRecord(id, request.Name, request.Email, DateTime.UtcNow);

        _store[id] = student;

        _logger.LogInformation("Created student {StudentId} - {Name}", id, request.Name);
        return Task.FromResult(student);
    }

    public Task<StudentRecord?> GetByIdAsync(string id)
    {
        _store.TryGetValue(id, out var student);
        if (student is null)
        {
            _logger.LogWarning("Student {StudentId} not found", id);
        }
        return Task.FromResult(student);
    }

    public Task<IReadOnlyList<StudentRecord>> GetAllAsync()
    {
        return Task.FromResult<IReadOnlyList<StudentRecord>>(_store.Values.ToList());
    }

    public Task<bool> DeleteAsync(string id)
    {
        var removed = _store.Remove(id);
        if (removed)
            _logger.LogInformation("Deleted student {StudentId}", id);
        else
            _logger.LogWarning("Delete failed - student {StudentId} not found", id);

        return Task.FromResult(removed);
    }
}

public record CreateStudentRequest(string Name, string Email);

public record StudentRecord(
    string Id,
    string Name,
    string Email,
    DateTime CreatedAt
);