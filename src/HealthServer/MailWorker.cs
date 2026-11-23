namespace HealthServer;

internal sealed class MailWorker
{
    private readonly MailQueue _queue;
    private readonly IEmailSender _sender;

    public MailWorker(MailQueue queue, IEmailSender sender)
    {
        _queue = queue;
        _sender = sender;
    }

    public async Task RunAsync(CancellationToken token)
    {
        try
        {
            await foreach (var job in _queue.ReadAllAsync(token))
            {
                try
                {
                    await _sender.SendPasswordResetOtpAsync(job.ToEmail, job.FullName, job.OtpCode);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[MailWorker Error] Không gửi được email tới {job.ToEmail}: {ex.Message}");
                }
            }
        }
        catch (OperationCanceledException) { }
    }
}
