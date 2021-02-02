using System;
using System.Diagnostics.CodeAnalysis;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace GtkSharp.Hosting
{
    public class HostingStartupGtkHostBuilder : IGtkHostBuilder, ISupportsStartup, ISupportsUseDefaultServiceProvider
    {
        private readonly GenericGtkHostBuilder _builder;
        private Action<GtkHostBuilderContext, IConfigurationBuilder>? _configureConfiguration;
        private Action<GtkHostBuilderContext, IServiceCollection>? _configureServices;

        public HostingStartupGtkHostBuilder(GenericGtkHostBuilder builder)
        {
            _builder = builder;
        }

        public IGtkHostBuilder ConfigureAppConfiguration(
            Action<GtkHostBuilderContext, IConfigurationBuilder> configureDelegate)
        {
            _configureConfiguration += configureDelegate;
            return this;
        }

        public IGtkHostBuilder ConfigureServices(Action<IServiceCollection> configureServices)
        {
            return ConfigureServices((context, services) => configureServices(services));
        }

        public IGtkHostBuilder ConfigureServices(Action<GtkHostBuilderContext, IServiceCollection> configureServices)
        {
            _configureServices += configureServices;
            return this;
        }

        public void ConfigureServices(GtkHostBuilderContext context, IServiceCollection services)
        {
            _configureServices?.Invoke(context, services);
        }

        public void ConfigureAppConfiguration(GtkHostBuilderContext context, IConfigurationBuilder builder)
        {
            _configureConfiguration?.Invoke(context, builder);
        }

        public IGtkHostBuilder UseDefaultServiceProvider(
            Action<GtkHostBuilderContext, ServiceProviderOptions> configure)
        {
            return _builder.UseDefaultServiceProvider(configure);
        }

        public IGtkHostBuilder UseStartup([DynamicallyAccessedMembers(StartupLinkerOptions.Accessibility)]
            Type startupType)
        {
            return _builder.UseStartup(startupType);
        }

        public IGtkHostBuilder UseStartup<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicMethods)]
            TStartup>(Func<GtkHostBuilderContext, TStartup> startupFactory)
        {
            return _builder.UseStartup(startupFactory);
        }
    }
}