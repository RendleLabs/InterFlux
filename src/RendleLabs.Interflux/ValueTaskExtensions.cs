using System.Threading.Tasks;

namespace RendleLabs.Interflux
{
    internal static class ValueTaskExtensions
    {
        public static ValueTask Append(this ValueTask first, ValueTask second)
        {
            if (first.IsCompleted)
            {
                return second.IsCompleted ? default : second;
            }

            if (second.IsCompleted)
            {
                return first;
            }

            return Await(first, second);
        }

        private static async ValueTask Await(ValueTask first, ValueTask second)
        {
            await first;
            await second;
        }
    }
}