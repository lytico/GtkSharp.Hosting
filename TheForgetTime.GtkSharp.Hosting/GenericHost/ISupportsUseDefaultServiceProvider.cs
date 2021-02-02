using System;
using Microsoft.Extensions.DependencyInjection;

namespace GtkSharp.Hosting
{
    internal interface ISupportsUseDefaultServiceProvider
    {
        IGtkHostBuilder UseDefaultServiceProvider(Action<GtkHostBuilderContext, ServiceProviderOptions> configure);
    }
}