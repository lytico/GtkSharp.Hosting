using System;
using System.Threading;
using System.Threading.Tasks;
using Gtk;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;

namespace GtkSharp.Hosting
{
    public class GenericGtkHostService : IHostedService
    {
        private readonly IHost _host;

        public GenericGtkHostService(IHost host,
            IOptions<GenericGtkHostServiceOptions> options,
            IApplicationBuilderFactory applicationBuilderFactory
        )
        {
            _host = host;
            Options = options.Value;
            ApplicationBuilderFactory = applicationBuilderFactory;
        }

        public GenericGtkHostServiceOptions Options { get; }
        public IApplicationBuilderFactory ApplicationBuilderFactory { get; }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            try
            {
                var configure = Options.ConfigureApplication;
                if (configure == null)
                {
                    throw new InvalidOperationException($"No application configured. Please specify an application via IGtkHostBuilder.UseStartup, IGtkHostBuilder.Configure, or specifying the startup assembly via {nameof(GtkHostDefaults.StartupAssemblyKey)} in the gtk host configuration.");
                }

                var builder = ApplicationBuilderFactory.CreateBuilder();

                configure(builder);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                throw;
            }

            await Task.CompletedTask;
#pragma warning disable 4014
            // 这个方法不能用 await 否则退不出去
            Task.Run(() =>
            {
                Application.Run();
                _host.StopAsync(cancellationToken);
            }, cancellationToken);
#pragma warning restore 4014
        }

        public async Task StopAsync(CancellationToken cancellationToken)
        {
            Application.Quit();
            _host.Dispose();
            await Task.CompletedTask;
        }
    }
}