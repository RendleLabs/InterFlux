using System;
using System.Buffers;
using System.IO.Pipelines;
using System.Threading.Tasks;

namespace RendleLabs.Interflux
{
    public class AsyncPipeReaderEnumerator
    {
        private const byte Newline = (byte) '\n';

        private static readonly Line EmptyLine = new Line(Array.Empty<byte>(), 0);
        private readonly PipeReader _pipeReader;
        private readonly ArrayPool<byte> _pool;
        private Line _current;
        private SequencePosition? _next;

        public AsyncPipeReaderEnumerator(PipeReader pipeReader, ArrayPool<byte> pool)
        {
            _pipeReader = pipeReader;
            _pool = pool;
            _current = default;
            _next = null;
        }

        public Line Current => !_current.IsEmpty ? _current : throw new InvalidOperationException("No data available");

        public ValueTask DisposeAsync()
        {
            return default;
        }

        public ValueTask<bool> MoveNextAsync()
        {
            if (_next.HasValue) _pipeReader.AdvanceTo(_next.Value);

            if (_pipeReader.TryRead(out var result))
            {
                var buffer = result.Buffer;
                if (buffer.IsEmpty && result.IsCompleted)
                {
                    _pipeReader.AdvanceTo(buffer.End);
                    _pipeReader.Complete();
                    _current = EmptyLine;
                    return new ValueTask<bool>(false);
                }

                if (TrySingleSegment(buffer) || TryAllSegments(buffer))
                {
                    return new ValueTask<bool>(true);
                }

                _pipeReader.AdvanceTo(buffer.Start, buffer.End);
            }

            return new ValueTask<bool>(MoveNextAsyncImpl());
        }

        private async Task<bool> MoveNextAsyncImpl()
        {
            while (true)
            {
                var result = await _pipeReader.ReadAsync();
                var buffer = result.Buffer;
                if (buffer.IsEmpty && result.IsCompleted)
                {
                    _pipeReader.AdvanceTo(buffer.End);
                    _pipeReader.Complete();
                    return false;
                }

                if (TrySingleSegment(buffer) || TryAllSegments(buffer))
                {
                    return true;
                }

                _pipeReader.AdvanceTo(buffer.Start, buffer.End);
            }
        }

        private bool TrySingleSegment(ReadOnlySequence<byte> buffer)
        {
            var span = buffer.FirstSpan;
            int skipped = SkipNewLines(ref span);

            if (skipped > 0)
            {
                buffer = buffer.Slice(skipped);
            }

            int endIndex = span.IndexOf(Newline);
            if (endIndex > -1)
            {
                span = span.Slice(0, endIndex);
                var array = _pool.Rent(endIndex + 1);
                span.CopyTo(array);
                _current = new Line(array, span.Length);
                _next = buffer.GetPosition(endIndex + 1);
                return true;
            }

            if (buffer.IsSingleSegment && buffer.Length > 0)
            {
                var array = _pool.Rent(span.Length);
                span.CopyTo(array);
                _current = new Line(array, span.Length);
                _next = buffer.End;
                return true;
            }

            return false;
        }

        private int SkipNewLines(ref ReadOnlySpan<byte> span)
        {
            int skipped = 0;
            while (span.Length > 0 && span[0] == Newline)
            {
                ++skipped;
            }

            return skipped;
        }

        private void SkipNewLines(ref ReadOnlySequence<byte> buffer)
        {
            var span = buffer.FirstSpan;
            while (span[0] == Newline)
            {
                int skip = SkipNewLines(ref span);
                if (skip == 0)
                {
                    return;
                }

                buffer = buffer.Slice(skip);
                span = buffer.FirstSpan;
            }
        }

        private bool TryAllSegments(ReadOnlySequence<byte> buffer)
        {
            byte[] array;
            SkipNewLines(ref buffer);
            if (!buffer.TryPositionOf(Newline, out var position))
            {
                if (buffer.Length > 0)
                {
                    array = _pool.Rent((int) buffer.Length);
                    buffer.CopyTo(array);
                    _current = new Line(array, (int) buffer.Length);
                    _next = buffer.GetPosition(1, position);
                    return true;
                }

                return false;
            }

            var slice = buffer.Slice(0, position);
            array = _pool.Rent((int) slice.Length);
            slice.CopyTo(array);
            _current = new Line(array, (int) slice.Length);
            _next = buffer.GetPosition(1, position);
            return true;
        }
    }
}