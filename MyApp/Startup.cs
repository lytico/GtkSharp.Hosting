using Gtk;
using GtkSharp.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using MyApp.Data;

namespace MyApp
{
    public class Startup
    {
        public IConfiguration Configuration { get; }

        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public void ConfigureServices(IServiceCollection services)
        {
            services
                .AddDbContext<MyDbContext>(builder =>
                {
                    builder.UseSqlite(Configuration.GetConnectionString("Default"));
                });
            services
                .AddMainWindow<MainWindow>();
        }

        public void Configure(IApplicationBuilder app)
        {
            app.UseMainWindow(window =>
            {
                window.SetDefaultSize(800, 600);
                window.SetPosition(WindowPosition.Center);
            });
        }
    }
}