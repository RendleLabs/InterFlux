using System;
using Microsoft.Extensions.DependencyInjection;

namespace RendleLabs.Interflux
{
    public static class ForwardersServiceCollectionExtensions
    {
        public static IServiceCollection AddForwarders(this IServiceCollection services,
            Action<ForwardersBuilder> builderAction)
        {
            if (builderAction == null) throw new ArgumentNullException(nameof(builderAction));
            var builder = new ForwardersBuilder();
            builderAction(builder);

            services.AddSingleton<IForwarders>(builder.Build());

            return services;
        }
    }
}