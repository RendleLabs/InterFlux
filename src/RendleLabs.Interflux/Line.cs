using System;

namespace RendleLabs.Interflux
{
    public struct Line
    {
        private readonly byte[] _data;
        private readonly int _length;

        public Line(byte[] data, int length)
        {
            _data = data;
            _length = length;
        }

        public ReadOnlyMemory<byte> Memory => _data.AsMemory(0, _length);

        internal byte[] Array => _data;

        public bool IsEmpty => _length == 0;
    }
}