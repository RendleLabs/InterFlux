using System.IO.Pipelines;

namespace RendleLabs.Interflux
{
    public static class PipeReaderExtensions
    {
        public static AsyncPipeReaderEnumerable AsAsyncEnumerable(this PipeReader pipeReader) => new AsyncPipeReaderEnumerable(pipeReader);
    }
}