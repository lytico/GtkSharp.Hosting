using System;
using System.Diagnostics.CodeAnalysis;

namespace GtkSharp.Hosting
{
    [AttributeUsage(AttributeTargets.Assembly, Inherited = false, AllowMultiple = true)]
    public sealed class HostingStartupAttribute : Attribute
    {
        public HostingStartupAttribute(
            [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicParameterlessConstructor)]
            Type hostingStartupType)
        {
            if (hostingStartupType == null)
            {
                throw new ArgumentNullException(nameof(hostingStartupType));
            }

            if (!typeof(IHostingStartup).IsAssignableFrom(hostingStartupType))
            {
                throw new ArgumentException($@"""{hostingStartupType}"" does not implement {typeof(IHostingStartup)}.",
                    nameof(hostingStartupType));
            }

            HostingStartupType = hostingStartupType;
        }

        [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicParameterlessConstructor)]
        public Type HostingStartupType { get; }
    }
}