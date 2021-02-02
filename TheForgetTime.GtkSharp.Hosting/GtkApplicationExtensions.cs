using System;
using Gtk;
using Microsoft.Extensions.DependencyInjection;

namespace GtkSharp.Hosting
{
    public static class GtkApplicationExtensions
    {
        public static Window UseMainWindow(this IApplicationBuilder app)
        {
            return UseMainWindow(app, _ => { });
        }

        public static Window UseMainWindow(this IApplicationBuilder app, Action<Window> windowConfigure)
        {
            var window = (Window) app.ApplicationServices.GetRequiredService<IMainWindow>();
            if (window is null)
            {
                throw new InvalidOperationException($"请先调用{nameof(IServiceCollection)}的扩展方法 AddMainWindow 设置一个住窗体");
            }

            windowConfigure(window);
            window.Destroyed += delegate { Application.Quit(); };
            window.ShowAll();
            return window;
        }
    }
}