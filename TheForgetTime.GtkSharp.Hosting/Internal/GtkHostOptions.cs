using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.Configuration;

namespace GtkSharp.Hosting
{
    public class GtkHostOptions
    {
        public GtkHostOptions(IConfiguration configuration, string applicationNameFallback)
        {
            if (configuration == null)
            {
                throw new ArgumentNullException(nameof(configuration));
            }

            ApplicationName = configuration[GtkHostDefaults.ApplicationKey] ?? applicationNameFallback;
            StartupAssembly = configuration[GtkHostDefaults.StartupAssemblyKey];

            CaptureStartupErrors = GtkHostUtilities.ParseBool(configuration, GtkHostDefaults.CaptureStartupErrorsKey);
            Environment = configuration[GtkHostDefaults.EnvironmentKey];
            PreventHostingStartup = GtkHostUtilities.ParseBool(configuration, GtkHostDefaults.PreventHostingStartupKey);

            HostingStartupAssemblies =
                Split($"{ApplicationName};{configuration[GtkHostDefaults.HostingStartupAssembliesKey]}");
            HostingStartupExcludeAssemblies = Split(configuration[GtkHostDefaults.HostingStartupExcludeAssembliesKey]);
        }

        public string ApplicationName { get; set; }

        public bool PreventHostingStartup { get; set; }

        public IReadOnlyList<string> HostingStartupAssemblies { get; set; }
        public IReadOnlyList<string> HostingStartupExcludeAssemblies { get; set; }

        public bool CaptureStartupErrors { get; set; }
        public string Environment { get; set; }
        public string StartupAssembly { get; set; }

        public IEnumerable<string> GetFinalHostingStartupAssemblies()
        {
            return HostingStartupAssemblies.Except(HostingStartupExcludeAssemblies, StringComparer.OrdinalIgnoreCase);
        }

        private IReadOnlyList<string> Split(string value)
        {
            return value?.Split(';', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries)
                   ?? Array.Empty<string>();
        }
    }
}