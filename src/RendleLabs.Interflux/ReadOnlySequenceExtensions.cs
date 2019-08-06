using System;
using System.Buffers;

namespace RendleLabs.Interflux
{
    internal static class ReadOnlySequenceExtensions
    {
        public static bool TryPositionOf<T>(this ReadOnlySequence<T> sequence, T value, out SequencePosition position)
            where T : IEquatable<T>
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