using System.Diagnostics.CodeAnalysis;

namespace GtkSharp.Hosting
{
    public class StartupLinkerOptions
    {
        public const DynamicallyAccessedMemberTypes Accessibility =
            DynamicallyAccessedMemberTypes.PublicConstructors |
            DynamicallyAccessedMemberTypes.PublicMethods;
    }
}