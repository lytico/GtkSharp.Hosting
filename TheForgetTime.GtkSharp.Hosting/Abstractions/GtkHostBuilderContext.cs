using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting.Internal;

namespace GtkSharp.Hosting
{
    public class GtkHostBuilderContext
    {
        public HostingEnvironment HostingEnvironment { get; set; } = default!;

        public IConfiguration Configuration { get; set; } = default!;
    }
}