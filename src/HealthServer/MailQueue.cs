using System.Threading.Channels;

namespace HealthServer;

internal sealed record MailJob(string ToEmail, string FullName, string OtpCode);

internal sealed class MailQueue
{
    private readonly Channel<MailJob> _kenh = Channel.CreateBounded<MailJob>(
        new BoundedChannelOptions(200)
        {
            FullMode = BoundedChannelFullMode.Wait,
            SingleReader = true
        });

    public ValueTask EnqueueAsync(MailJob job, CancellationToken token = default) =>
        _kenh.Writer.WriteAsync(job, token);

    public IAsyncEnumerable<MailJob> ReadAllAsync(CancellationToken token) =>
        _kenh.Reader.ReadAllAsync(token);
}
