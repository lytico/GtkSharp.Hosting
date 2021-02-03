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

            // 这个地方必须是 Scoped 否则在窗口依赖DbContext实例时无法添加迁移
            services.TryAddScoped<IMainWindow, T>();
        }
    }
}