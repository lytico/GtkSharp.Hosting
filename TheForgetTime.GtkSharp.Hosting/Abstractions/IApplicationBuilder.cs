using System;
using System.Collections.Generic;

namespace GtkSharp.Hosting
{
    public interface IApplicationBuilder
    {
        IServiceProvider ApplicationServices { get; set; }

        IDictionary<string, object?> Properties { get; }

        IApplicationBuilder New();
    }
}