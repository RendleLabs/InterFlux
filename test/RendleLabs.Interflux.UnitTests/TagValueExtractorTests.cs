using System;
using System.Text;
using System.Threading.Tasks;
using RendleLabs.Interflux.LineHelpers;
using Xunit;

namespace RendleLabs.Interflux.UnitTests
{
    public class TagValueExtractorTests
    {
        [Fact]
        public void ExtractsSimpleValueWhenOneTag()
        {
            const string test = "m,x=Foo y=1";
            var bytes = Encoding.UTF8.GetBytes(test);
            var key = new[] {(byte) 'x'};
            var expected = Encoding.UTF8.GetBytes("Foo");
            Assert.True(TagValueExtractor.TryGetTag(bytes, key, out var actual));
            var debug = Encoding.UTF8.GetString(actual);
            Assert.True(expected.AsSpan().SequenceEqual(actual));
        }

        [Fact]
        public void ExtractsSimpleValueWhenTwoTags()
        {
            const string test = "m,x=Foo,z=Bar y=1";
            var bytes = Encoding.UTF8.GetBytes(test);
            var key = new[] {(byte) 'x'};
            var expected = Encoding.UTF8.GetBytes("Foo");
            Assert.True(TagValueExtractor.TryGetTag(bytes, key, out var actual));
            var debug = Encoding.UTF8.GetString(actual);
            Assert.True(expected.AsSpan().SequenceEqual(actual));

            Span<byte> x = stackalloc byte[4];
            x[0] = 2;
        }

        [Fact]
        public void ExtractsSimpleValueWhenThreeTags()
        {
            const string test = "m,a=Wibble,x=Foo,z=Bar y=1";
            var bytes = Encoding.UTF8.GetBytes(test);
            var key = new[] {(byte) 'x'};
            var expected = Encoding.UTF8.GetBytes("Foo");
            Assert.True(TagValueExtractor.TryGetTag(bytes, key, out var actual));
            var debug = Encoding.UTF8.GetString(actual);
            Assert.True(expected.AsSpan().SequenceEqual(actual));
        }

        [Fact]
        public void ExtractsValueWithEscapedSpace()
        {
            const string test = "m,x=Foo\\ Bar y=1";
            var bytes = Encoding.UTF8.GetBytes(test);
            var key = new[] {(byte) 'x'};
            var expected = Encoding.UTF8.GetBytes("Foo\\ Bar");
            Assert.True(TagValueExtractor.TryGetTag(bytes, key, out var actual));
            var debug = Encoding.UTF8.GetString(actual);
            Assert.True(expected.AsSpan().SequenceEqual(actual));
        }

        [Fact]
        public void ExtractsValueWithEscapedComma()
        {
            const string test = "m,x=Foo\\,Bar y=1";
            var bytes = Encoding.UTF8.GetBytes(test);
            var key = new[] {(byte) 'x'};
            var expected = Encoding.UTF8.GetBytes("Foo\\,Bar");
            Assert.True(TagValueExtractor.TryGetTag(bytes, key, out var actual));
            var debug = Encoding.UTF8.GetString(actual);
            Assert.True(expected.AsSpan().SequenceEqual(actual));
        }
    }
}
