using System;
using System.Buffers;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;

namespace RendleLabs.Interflux
{
    public class Forwarders : IForwarders
    {
        private static readonly ArrayPool<byte> Pool = ArrayPool<byte>.Shared;

        private readonly Channel<Line> _channel = Channel.CreateBounded<Line>(new BoundedChannelOptions(8192)
        {
            AllowSynchronousContinuations = true,
            SingleReader = true,
            SingleWriter = false,
            FullMode = BoundedChannelFullMode.Wait
        });

        private readonly Func<ReadOnlyMemory<byte>, ValueTask> _pipeline;
        private readonly Func<ReadOnlyMemory<byte>, ValueTask>? _always;

        public Forwarders(Func<ReadOnlyMemory<byte>, ValueTask> pipeline, Func<ReadOnlyMemory<byte>, ValueTask>? always)
        {
            _pipeline = pipeline;
            _always = always;
        }

        public ValueTask AddAsync(Line line, CancellationToken token = default) =>
            _channel.Writer.TryWrite(line) ? default : _channel.Writer.WriteAsync(line, token);

        private async Task RunAsync(CancellationToken token)
        {
            while (await _channel.Reader.WaitToReadAsync(token).ConfigureAwait(false))
            {
                await RunLinesAsync(token);
            }
        }

        private ValueTask RunLinesAsync(CancellationToken token)
        {
            ValueTask task = default;

            while (_channel.Reader.TryRead(out var line))
            {
                task = task.Append(ProcessAsync(line));
            }

            return task;
        }

        private async ValueTask ProcessAsync(Line line)
        {
            try
            {
                if (_always == null)
                {

                    await _pipeline(line.Memory);
                }
                else
                {
                    
                    await _pipeline(line.Memory).Append(_always(line.Memory));
                }
            }
            finally
            {
                Pool.Return(line.Array);
            }
        }
    }
}