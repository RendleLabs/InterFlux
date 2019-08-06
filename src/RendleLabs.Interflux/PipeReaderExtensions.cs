using System.Buffers;
using System.IO.Pipelines;

namespace RendleLabs.Interflux
{
    public static class PipeReaderExtensions
    {
        public static AsyncPipeReaderEnumerable AsAsyncEnumerable(this PipeReader pipeReader) =>
            AsAsyncEnumerable(pipeReader, ArrayPool<byte>.Shared);

        public static AsyncPipeReaderEnumerable AsAsyncEnumerable(this PipeReader pipeReader, ArrayPool<byte> pool) =>
            new AsyncPipeReaderEnumerable(pipeReader, pool);
    }
}