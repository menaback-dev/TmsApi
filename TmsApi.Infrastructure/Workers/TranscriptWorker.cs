using System.Threading.Channels;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using TmsApi.Application.Notifications;
using TmsApi.Application.Transcripts;
using TmsApi.Infrastructure.Transcripts;

namespace TmsApi.Infrastructure.Workers;

public class TranscriptWorker(
    Channel<TranscriptRequest> channel,
    ITranscriptStatusStore statusStore,
    ITranscriptNotificationService notificationService,
    ILogger<TranscriptWorker> logger) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken ct)
    {
        logger.LogInformation("Transcript worker started.");

        await foreach (var request in channel.Reader.ReadAllAsync(ct))
        {
            var reportId = request.ReportId!;

            try
            {
                await statusStore.MarkProcessingAsync(reportId, ct);
                await Task.Delay(TimeSpan.FromSeconds(5), ct);

                var downloadUrl = $"/api/v2/transcripts/{reportId}/download";
                await statusStore.MarkReadyAsync(reportId, downloadUrl, ct);

                await notificationService.NotifyTranscriptReadyAsync(
                    request.StudentId, reportId, downloadUrl);

                logger.LogInformation(
                    "Transcript ready, notification sent: {ReportId} for student {StudentId}",
                    reportId, request.StudentId);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Transcript generation failed: {ReportId}", reportId);
                await statusStore.MarkFailedAsync(reportId, ex.Message, CancellationToken.None);
            }
        }
    }
}