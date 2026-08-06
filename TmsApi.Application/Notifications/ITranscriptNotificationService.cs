namespace TmsApi.Application.Notifications;

public interface ITranscriptNotificationService
{
    Task NotifyTranscriptReadyAsync(int studentId, string reportId, string downloadUrl);
}