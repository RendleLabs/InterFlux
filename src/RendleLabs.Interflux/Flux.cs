using System;
using System.Buffers;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace RendleLabs.Interflux
{
    public class Flux
    {
        private readonly IForwarders _forwarders;
        private static readonly ArrayPool<byte> Pool = ArrayPool<byte>.Shared;

        public Flux(IForwarders forwarders)
        {
            _forwarders = forwarders;
        }

        public async Task RunAsync(HttpContext context)
        {
            ValueTask task = default;
            await foreach (var line in context.Request.BodyReader.AsAsyncEnumerable(Pool))
            {
                task = task.Append(_forwarders.AddAsync(line));
            }

            await task;

            context.Response.StatusCode = 201;
        }

        private Line Copy(in ReadOnlySequence<byte> sequence)
        {
            var data = Pool.Rent((int) sequence.Length);
            sequence.CopyTo(data);
            return new Line(data, (int)sequence.Length);
        }
    }
}