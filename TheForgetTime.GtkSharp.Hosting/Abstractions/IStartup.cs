using System;
using Microsoft.Extensions.DependencyInjection;

namespace GtkSharp.Hosting
{
    public interface IStartup
    {
        IServiceProvider ConfigureServices(IServiceCollection services);

        void Configure(IApplicationBuilder app);
    }
}