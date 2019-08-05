using System;
using System.Buffers;
using System.Diagnostics;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace RendleLabs.Interflux
{
    public class Flux
    {
        private const byte Comma = (byte) ',';
        private const byte Space = (byte) ' ';

        public async Task RunAsync(HttpContext context)
        {
            await foreach (var sequence in context.Request.BodyReader.AsAsyncEnumerable())
            {
                if (sequence.IsEmpty) continue;
                if (sequence.IsSingleSegment)
                {
                    Process(sequence.FirstSpan);
                }
                else
                {
                    Process(sequence);
                }
            }

            context.Response.StatusCode = 201;
        }

        private void Process(in ReadOnlySequence<byte> sequence)
        {
            Span<byte> line = stackalloc byte[(int) sequence.Length];
            sequence.CopyTo(line);
            Process(line);
        }

        private void Process(in ReadOnlySpan<byte> line)
        {
            int comma = line.IndexOf(Comma);
            int space = line.IndexOf(Space);
            int delimiter = MinGreaterThanZero(comma, space);
            var measurement = Encoding.UTF8.GetString(line.Slice(0, delimiter));
            Debug.WriteLine(measurement);
        }

        private static int MinGreaterThanZero(int a, int b)
        {
            if (a > 0 && b > 0) return Math.Min(a, b);
            return a > 0 ? a : b;
        }
    }
}