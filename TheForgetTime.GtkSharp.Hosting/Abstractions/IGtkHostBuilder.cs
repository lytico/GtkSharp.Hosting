using System;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace GtkSharp.Hosting
{
    public interface IGtkHostBuilder
    {
        IGtkHostBuilder ConfigureAppConfiguration(
            Action<GtkHostBuilderContext, IConfigurationBuilder> configureDelegate);

        IGtkHostBuilder ConfigureServices(Action<IServiceCollection> configureServices);

        IGtkHostBuilder ConfigureServices(Action<GtkHostBuilderContext, IServiceCollection> configureServices);
    }
}