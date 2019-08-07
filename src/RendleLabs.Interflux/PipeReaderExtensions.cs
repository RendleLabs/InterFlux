using System.Buffers;
using System.IO.Pipelines;

namespace RendleLabs.Interflux
{
    public static class PipeReaderExtensions
    {
        public static AsyncPipeReaderEnumerable LinesAsAsyncEnumerable(this PipeReader pipeReader) =>
            LinesAsAsyncEnumerable(pipeReader, ArrayPool<byte>.Shared);

        public static AsyncPipeReaderEnumerable LinesAsAsyncEnumerable(this PipeReader pipeReader, ArrayPool<byte> pool) =>
            new AsyncPipeReaderEnumerable(pipeReader, pool);
    }
}