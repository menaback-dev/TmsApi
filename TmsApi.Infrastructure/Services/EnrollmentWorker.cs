using Microsoft.Extensions.DependencyInjection;
using TmsApi.Application.Interfaces;

namespace TmsApi.Infrastructure.Services;

public class EnrollmentWorker
{
    private readonly IServiceScopeFactory _scopeFactory;

    public EnrollmentWorker(IServiceScopeFactory scopeFactory)
    {
        _scopeFactory = scopeFactory;
    }

    public void ProcessBatch()
    {
        using var scope =
            _scopeFactory.CreateScope();

        var service =
            scope.ServiceProvider
                 .GetRequiredService<IEnrollmentService>();

    }
}