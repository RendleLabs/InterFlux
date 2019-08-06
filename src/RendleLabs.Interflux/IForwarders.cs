using System.Threading;
using System.Threading.Tasks;

namespace RendleLabs.Interflux
{
    public interface IForwarders
    {
        ValueTask AddAsync(Line line, CancellationToken token = default);
    }
}