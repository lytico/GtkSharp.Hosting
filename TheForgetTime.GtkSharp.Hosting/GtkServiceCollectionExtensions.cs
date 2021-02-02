using System;
using Gtk;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace GtkSharp.Hosting
{
    public static class GtkServiceCollectionExtensions
    {
        public static void AddMainWindow<T>(this IServiceCollection services)
            where T : Window, IMainWindow
        {
            if (services is null) throw new ArgumentNullException(nameof(services));

            services.TryAddSingleton<IMainWindow, T>();
        }
    }
}