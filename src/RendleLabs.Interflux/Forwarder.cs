using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RendleLabs.Interflux
{
    public class ForwardersBuilder
    {
        private readonly List<Func<ReadOnlyMemory<byte>, Func<ValueTask>, ValueTask>> _pipeline =
            new List<Func<ReadOnlyMemory<byte>, Func<ValueTask>, ValueTask>>();

        private readonly List<Func<ReadOnlyMemory<byte>, ValueTask>> _always =
            new List<Func<ReadOnlyMemory<byte>, ValueTask>>();

        public ForwardersBuilder Use(Func<ReadOnlyMemory<byte>, Func<ValueTask>, ValueTask> func)
        {
            _pipeline.Add(func);
            return this;
        }

        public ForwardersBuilder Always(Func<ReadOnlyMemory<byte>, ValueTask> func)
        {
            _always.Add(func);
            return this;
        }

        public Forwarders Build() => new Forwarders(BuildPipelineFunc(), BuildAlwaysFunc());

        private Func<ReadOnlyMemory<byte>, ValueTask> BuildPipelineFunc()
        {
            ValueTask NoOp() => default;

            var last = _pipeline.Last();
            Func<ReadOnlyMemory<byte>, ValueTask> pipeline = line => last(line, NoOp);

            foreach (var func in _pipeline.AsEnumerable().Reverse().Skip(1))
            {
                var n = pipeline;
                pipeline = line => func(line, () => n(line));
            }

            return pipeline;
        }

        private Func<ReadOnlyMemory<byte>, ValueTask>? BuildAlwaysFunc()
        {
            if (_always.Count == 0) return null;

            var always = _always[0];
            if (_always.Count > 1)
            {
                foreach (var func in _always)
                {
                    var a = always;
                    always = line => a(line).Append(func(line));
                }
            }

            return always;
        }
    }
}