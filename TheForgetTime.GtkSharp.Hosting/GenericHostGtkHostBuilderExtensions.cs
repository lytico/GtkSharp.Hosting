using System;
using Gtk;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace GtkSharp.Hosting
{
    public static class GenericHostGtkHostBuilderExtensions
    {
        public static IHostBuilder ConfigureGtkHost(this IHostBuilder builder,
            Action<IGtkHostBuilder> configure)
        {
            if (configure is null) throw new ArgumentNullException(nameof(configure));

            return builder.ConfigureGtkHost("", Array.Empty<string>(), configure, _ => { });
        }

        public static IHostBuilder ConfigureGtkHost(this IHostBuilder builder, string applicationId, string[] args,
            Action<IGtkHostBuilder> configure)
        {
            if (configure is null) throw new ArgumentNullException(nameof(configure));

            return builder.ConfigureGtkHost(applicationId, args, configure, _ => { });
        }

        public static IHostBuilder ConfigureGtkHost(this IHostBuilder builder, string applicationId, string[] args,
            Action<IGtkHostBuilder> configure,
            Action<GtkHostBuilderOptions> configureGtkHostBuilder)
        {
            if (args == null)
            {
                throw new ArgumentNullException(nameof(args));
            }

            if (string.IsNullOrEmpty(applicationId))
            {
                Application.Init();
            }
            else
            {
                Application.Init(applicationId, ref args);
            }

            if (configure is null) throw new ArgumentNullException(nameof(configure));

            if (configureGtkHostBuilder is null) throw new ArgumentNullException(nameof(configureGtkHostBuilder));

            var gtkHostBuilderOptions = new GtkHostBuilderOptions();
            configureGtkHostBuilder(gtkHostBuilderOptions);
            var gtkHostBuilder = new GenericGtkHostBuilder(builder, gtkHostBuilderOptions);
            configure(gtkHostBuilder);
            builder.ConfigureServices((context, services) => services.AddHostedService<GenericGtkHostService>());
            return builder;
        }
    }
}