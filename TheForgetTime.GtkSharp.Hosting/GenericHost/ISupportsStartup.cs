using System;
using System.Diagnostics.CodeAnalysis;

namespace GtkSharp.Hosting
{
    internal interface ISupportsStartup
    {
        IGtkHostBuilder UseStartup([DynamicallyAccessedMembers(StartupLinkerOptions.Accessibility)]
            Type startupType);

        IGtkHostBuilder UseStartup<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicMethods)]
            TStartup>(Func<GtkHostBuilderContext, TStartup> startupFactory);
    }
}