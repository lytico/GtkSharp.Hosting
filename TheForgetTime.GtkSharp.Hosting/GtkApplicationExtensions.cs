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
                throw new InvalidOperationException($"Please call {nameof(IServiceCollection)} extension method AddMainWindow to set a main window");
            }

            windowConfigure(window);
            window.Destroyed += delegate { Application.Quit(); };
            window.ShowAll();
            return window;
        }
    }
}