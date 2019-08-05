using System;
using System.Buffers;
using System.Collections.Generic;
using System.IO.Pipelines;
using System.Threading.Tasks;

namespace RendleLabs.Interflux
{
    public struct AsyncPipeReaderEnumerator
    {
        private const byte Newline = (byte) '\n';

        private readonly PipeReader _pipeReader;
        private ReadOnlySequence<byte> _current;
        private SequencePosition? _next;

        public AsyncPipeReaderEnumerator(PipeReader pipeReader)
        {
            _pipeReader = pipeReader;
            _current = default;
            _next = null;
        }

        public ReadOnlySequence<byte> Current => !_current.IsEmpty ? _current : throw new InvalidOperationException("No data available");

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

            int endIndex = buffer.FirstSpan.IndexOf(Newline);
            if (endIndex > -1)
            {
                _current = buffer.Slice(0, endIndex);
                _next = buffer.GetPosition(endIndex + 1);
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
            SkipNewLines(ref buffer);
            if (!buffer.TryPositionOf(Newline, out var position))
            {
                return false;
            }
            _current = buffer.Slice(0, position);
            _next = buffer.GetPosition(1, position);
            return true;
        }
    }

    internal static class ReadOnlySequenceExtensions
    {
        public static bool TryPositionOf<T>(this ReadOnlySequence<T> sequence, T value, out SequencePosition position) where T : IEquatable<T>
        {
            var maybePosition = sequence.PositionOf(value);
            if (maybePosition is null)
            {
                position = default;
                return false;
            }

            position = maybePosition.Value;
            return true;
        }
    }
}