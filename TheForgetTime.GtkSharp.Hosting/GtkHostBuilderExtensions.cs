using System;
using System.Diagnostics.CodeAnalysis;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace GtkSharp.Hosting
{
    public static class GtkHostBuilderExtensions
    {
        public static IGtkHostBuilder UseStartup<
            [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicMethods)]
            TStartup>(this IGtkHostBuilder hostBuilder, Func<GtkHostBuilderContext, TStartup> startupFactory)
            where TStartup : class
        {
            if (startupFactory == null)
            {
                throw new ArgumentNullException(nameof(startupFactory));
            }

            // Light up the GenericWebHostBuilder implementation
            if (hostBuilder is ISupportsStartup supportsStartup)
            {
                return supportsStartup.UseStartup(startupFactory);
            }

            return hostBuilder
                .ConfigureServices((context, services) =>
                {
                    services.AddSingleton(typeof(IStartup), sp =>
                    {
                        var instance = startupFactory(context) ??
                                       throw new InvalidOperationException(
                                           "The specified factory returned null startup instance.");

                        var hostingEnvironment = sp.GetRequiredService<IHostEnvironment>();

                        // Check if the instance implements IStartup before wrapping
                        if (instance is IStartup startup)
                        {
                            return startup;
                        }

                        return new ConventionBasedStartup(StartupLoader.LoadMethods(sp, instance.GetType(),
                            hostingEnvironment.EnvironmentName, instance));
                    });
                });
        }

        public static IGtkHostBuilder UseStartup(this IGtkHostBuilder hostBuilder,
            [DynamicallyAccessedMembers(StartupLinkerOptions.Accessibility)]
            Type startupType)
        {
            if (startupType == null)
            {
                throw new ArgumentNullException(nameof(startupType));
            }

            // Light up the GenericWebHostBuilder implementation
            if (hostBuilder is ISupportsStartup supportsStartup)
            {
                return supportsStartup.UseStartup(startupType);
            }

            return hostBuilder
                .ConfigureServices(services =>
                {
                    if (typeof(IStartup).IsAssignableFrom(startupType))
                    {
                        services.AddSingleton(typeof(IStartup), startupType);
                    }
                    else
                    {
                        services.AddSingleton(typeof(IStartup), sp =>
                        {
                            var hostingEnvironment = sp.GetRequiredService<IHostEnvironment>();
                            return new ConventionBasedStartup(StartupLoader.LoadMethods(sp, startupType,
                                hostingEnvironment.EnvironmentName));
                        });
                    }
                });
        }

        public static IGtkHostBuilder UseStartup<[DynamicallyAccessedMembers(StartupLinkerOptions.Accessibility)]
            TStartup>(this IGtkHostBuilder hostBuilder) where TStartup : class
        {
            return hostBuilder.UseStartup(typeof(TStartup));
        }
    }
}