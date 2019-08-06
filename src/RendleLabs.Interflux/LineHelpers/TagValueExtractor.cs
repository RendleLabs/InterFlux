using System;

namespace RendleLabs.Interflux.LineHelpers
{
    public static class TagValueExtractor
    {
        private const byte Space = (byte) ' ';
        private const byte Comma = (byte) ',';
        private const byte EqualsSign = (byte) '=';
        private const byte Backslash = (byte) '\\';

        public static bool TryGetTag(this in ReadOnlySpan<byte> span, in ReadOnlySpan<byte> tag, out ReadOnlySpan<byte> value)
        {
            int space = IndexOfUnescaped(span, Space);
            if (space < 0)
            {
                value = default;
                return false;
            }

            while (span[space - 1] == Backslash && span[space - 2] != Backslash)
            {
                space += span.Slice(space + 1).IndexOf(Space) + 1;
            }

            var slice = span.Slice(0, space);
            int comma = slice.IndexOf(Comma);
            if (comma < 0)
            {
                value = default;
                return false;
            }

            while (comma > 0)
            {
                slice = slice.Slice(comma + 1);
                int equals = slice.IndexOf(EqualsSign);
                if (equals > 0)
                {
                    var key = slice.Slice(0, equals);
                    if (key.SequenceEqual(tag))
                    {
                        var rest = slice.Slice(equals + 1);
                        int end = IndexOfUnescaped(rest, Comma);
                        value = end < 0 ? rest : rest.Slice(0, end);
                        return true;
                    }
                }

                comma = slice.IndexOf(Comma);
            }

            value = default;
            return false;
        }

        private static int IndexOfUnescaped(in ReadOnlySpan<byte> span, byte token)
        {
            int index = span.IndexOf(token);
            if (index < 1) return index;
            if (span[index - 1] != Backslash) return index;
            if (index > 1 && span[index - 2] == Backslash) return index;
            int next = IndexOfUnescaped(span.Slice(index + 1), token);
            return next < 0 ? next : index + next + 1;
        }
    }
}