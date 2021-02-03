using System;
using System.Collections.Generic;

namespace GtkSharp.Hosting
{
    public class ApplicationBuilder : IApplicationBuilder
    {
        private const string ApplicationServicesKey = "application.Services";

        public ApplicationBuilder(IServiceProvider serviceProvider)
        {
            Properties = new Dictionary<string, object?>(StringComparer.Ordinal);
            ApplicationServices = serviceProvider;
        }

        public IServiceProvider ApplicationServices
        {
            get => GetProperty<IServiceProvider>(ApplicationServicesKey)!;
            set => SetProperty(ApplicationServicesKey, value);
        }

        public IDictionary<string, object?> Properties { get; }

        private T? GetProperty<T>(string key)
        {
            return Properties.TryGetValue(key, out var value) ? (T?) value : default;
        }

        private void SetProperty<T>(string key, T value)
        {
            Properties[key] = value;
        }
    }
}