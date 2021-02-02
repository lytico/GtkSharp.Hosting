using System;

namespace GtkSharp.Hosting
{
    public class GenericGtkHostServiceOptions
    {
        public Action<IApplicationBuilder>? ConfigureApplication { get; set; }

        public GtkHostOptions GtkHostOptions { get; set; } = default!; // Always set when options resolved by DI

        public AggregateException? HostingStartupExceptions { get; set; }
    }
}