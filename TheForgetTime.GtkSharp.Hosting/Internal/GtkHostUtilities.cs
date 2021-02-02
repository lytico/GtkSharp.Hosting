using System;
using Microsoft.Extensions.Configuration;

namespace GtkSharp.Hosting
{
    internal class GtkHostUtilities
    {
        public static bool ParseBool(IConfiguration configuration, string key)
        {
            return string.Equals("true", configuration[key], StringComparison.OrdinalIgnoreCase)
                   || string.Equals("1", configuration[key], StringComparison.OrdinalIgnoreCase);
        }
    }
}