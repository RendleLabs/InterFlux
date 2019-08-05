using System;
using System.IO.Pipelines;
using System.Threading;

namespace RendleLabs.Interflux
{
    public struct AsyncPipeReaderEnumerable
    {
        private readonly PipeReader _pipeReader;
        private int _used;

        public AsyncPipeReaderEnumerable(PipeReader pipeReader)
        {
            _pipeReader = pipeReader;
            _used = 0;
        }

        public AsyncPipeReaderEnumerator GetAsyncEnumerator(CancellationToken cancellationToken = new CancellationToken())
        {
            if (Interlocked.Exchange(ref _used, 1) != 0)
            {
                throw new InvalidOperationException("Can only enumerate PipeReader once.");
            }
            return new AsyncPipeReaderEnumerator(_pipeReader);
        }
    }
}