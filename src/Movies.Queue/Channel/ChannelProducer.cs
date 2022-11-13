using System.Threading.Channels;
namespace Movies.Queue.Channels;
public sealed class ChannelProducer<T> : IProducer<T>
{
    private readonly Channel<T> _channel;
    public ChannelProducer(
        Channel<T> channel)
    {
        _channel = channel ?? throw new ArgumentNullException(nameof(channel));
    }
    public async Task Write(T value)
    {
        await _channel.Writer.WriteAsync(value);
    }
}