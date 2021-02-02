using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reflection;
using System.Runtime.ExceptionServices;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Hosting.Internal;

namespace GtkSharp.Hosting
{
    public class GenericGtkHostBuilder : IGtkHostBuilder, ISupportsStartup, ISupportsUseDefaultServiceProvider
    {
        private readonly IHostBuilder _builder;
        private readonly IConfiguration _config;
        private object? _startupObject;
        private readonly object _startupKey = new object();

        private AggregateException? _hostingStartupErrors;
        private HostingStartupGtkHostBuilder? _hostingStartupGtkHostBuilder;

        public GenericGtkHostBuilder(IHostBuilder builder, GtkHostBuilderOptions options)
        {
            _builder = builder;
            var configBuilder = new ConfigurationBuilder()
                .AddInMemoryCollection();

            if (options.SuppressEnvironmentConfiguration)
            {
                configBuilder.AddEnvironmentVariables(prefix: "GTKNETCORE_");
            }

            _config = configBuilder.Build();

            _builder.ConfigureHostConfiguration(config =>
            {
                config.AddConfiguration(_config);
                ExecuteHostingStartups();
            });

            _builder.ConfigureAppConfiguration((context, configurationBuilder) =>
            {
                if (_hostingStartupGtkHostBuilder is not null)
                {
                    var gtkHostContext = GetGtkHostBuilderContext(context);
                    _hostingStartupGtkHostBuilder.ConfigureAppConfiguration(gtkHostContext, configurationBuilder);
                }
            });

            _builder.ConfigureServices((context, services) =>
            {
                var gtkHostContext = GetGtkHostBuilderContext(context);
                var gtkHostOptions = (GtkHostOptions) context.Properties[typeof(GtkHostOptions)];

                services.AddSingleton(gtkHostContext.HostingEnvironment);

                services.Configure<GenericGtkHostServiceOptions>(options =>
                {
                    options.GtkHostOptions = gtkHostOptions;
                    options.HostingStartupExceptions = _hostingStartupErrors;
                });

                services.TryAddSingleton<IApplicationBuilderFactory, ApplicationBuilderFactory>();

                _hostingStartupGtkHostBuilder?.ConfigureServices(gtkHostContext, services);

                if (!string.IsNullOrEmpty(gtkHostOptions.StartupAssembly))
                {
                    try
                    {
                        var startupType = StartupLoader.FindStartupType(gtkHostOptions.StartupAssembly,
                            gtkHostContext.HostingEnvironment.EnvironmentName);
                        UseStartup(startupType, context, services);
                    }
                    catch (Exception ex) when (gtkHostOptions.CaptureStartupErrors)
                    {
                        var capture = ExceptionDispatchInfo.Capture(ex);

                        services.Configure<GenericGtkHostServiceOptions>(options =>
                        {
                            options.ConfigureApplication = app =>
                            {
                                // Throw if there was any errors initializing startup
                                capture.Throw();
                            };
                        });
                    }
                }
            });
        }

        private void ExecuteHostingStartups()
        {
            var gtkHostOptions =
                new GtkHostOptions(_config, Assembly.GetEntryAssembly()?.GetName().Name ?? string.Empty);
            if (gtkHostOptions.PreventHostingStartup)
            {
                return;
            }

            var exceptions = new List<Exception>();
            _hostingStartupGtkHostBuilder = new HostingStartupGtkHostBuilder(this);

            foreach (var assemblyName in gtkHostOptions.GetFinalHostingStartupAssemblies()
                .Distinct(StringComparer.OrdinalIgnoreCase))
            {
                try
                {
                    var assembly = Assembly.Load(new AssemblyName(assemblyName));
                    foreach (var attribute in assembly.GetCustomAttributes<HostingStartupAttribute>())
                    {
                        var hostingStartup = (IHostingStartup) Activator.CreateInstance(attribute.HostingStartupType)!;
                        hostingStartup.Configure(_hostingStartupGtkHostBuilder);
                    }
                }
                catch (Exception ex)
                {
                    exceptions.Add(new InvalidOperationException($"启动程序集{assemblyName}无法执行。有关更多详细信息，请参见内部异常。", ex));
                }
            }

            if (exceptions.Count > 0)
            {
                _hostingStartupErrors = new AggregateException(exceptions);
            }
        }

        public IGtkHostBuilder ConfigureAppConfiguration(
            Action<GtkHostBuilderContext, IConfigurationBuilder> configureDelegate)
        {
            _builder.ConfigureAppConfiguration((context, builder) =>
            {
                var webhostBuilderContext = GetGtkHostBuilderContext(context);
                configureDelegate(webhostBuilderContext, builder);
            });

            return this;
        }

        public IGtkHostBuilder ConfigureServices(Action<IServiceCollection> configureServices)
        {
            return ConfigureServices((context, services) => configureServices(services));
        }

        public IGtkHostBuilder ConfigureServices(Action<GtkHostBuilderContext, IServiceCollection> configureServices)
        {
            _builder.ConfigureServices((context, builder) =>
            {
                var webHostBuilderContext = GetGtkHostBuilderContext(context);
                configureServices(webHostBuilderContext, builder);
            });

            return this;
        }

        public IGtkHostBuilder UseDefaultServiceProvider(
            Action<GtkHostBuilderContext, ServiceProviderOptions> configure)
        {
            _builder.UseServiceProviderFactory(context =>
            {
                var webHostBuilderContext = GetGtkHostBuilderContext(context);
                var options = new ServiceProviderOptions();
                configure(webHostBuilderContext, options);
                return new DefaultServiceProviderFactory(options);
            });

            return this;
        }

        public IGtkHostBuilder UseStartup([DynamicallyAccessedMembers(StartupLinkerOptions.Accessibility)]
            Type startupType)
        {
            _startupObject = startupType;
            _builder.ConfigureServices((context, services) =>
            {
                if (object.ReferenceEquals(_startupObject, startupType))
                {
                    UseStartup(startupType, context, services);
                }
            });
            return this;
        }

        public IGtkHostBuilder UseStartup<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicMethods)]
            TStartup>(Func<GtkHostBuilderContext, TStartup> startupFactory)
        {
            // Clear the startup type
            _startupObject = startupFactory;

            _builder.ConfigureServices((context, services) =>
            {
                // UseStartup can be called multiple times. Only run the last one.
                if (object.ReferenceEquals(_startupObject, startupFactory))
                {
                    var webHostBuilderContext = GetGtkHostBuilderContext(context);
                    var instance = startupFactory(webHostBuilderContext) ??
                                   throw new InvalidOperationException(
                                       "The specified factory returned null startup instance.");
                    UseStartup(instance.GetType(), context, services, instance);
                }
            });

            return this;
        }

        [UnconditionalSuppressMessage("ReflectionAnalysis", "IL2006:UnrecognizedReflectionPattern",
            Justification = "We need to call a generic method on IHostBuilder.")]
        private void UseStartup([DynamicallyAccessedMembers(StartupLinkerOptions.Accessibility)]
            Type startupType, HostBuilderContext context, IServiceCollection services, object? instance = null)
        {
            var gtkHostBuilderContext = GetGtkHostBuilderContext(context);
            var gtkHostOptions = (GtkHostOptions) context.Properties[typeof(GtkHostOptions)];

            ExceptionDispatchInfo? startupError = null;
            ConfigureBuilder? configureBuilder = null;

            try
            {
                if (typeof(IStartup).IsAssignableFrom(startupType))
                {
                    throw new NotSupportedException($"{typeof(IStartup)} isn't supported");
                }

                if (StartupLoader.HasConfigureServicesIServiceProviderDelegate(startupType,
                    context.HostingEnvironment.EnvironmentName))
                {
                    throw new NotSupportedException(
                        $"ConfigureServices returning an {typeof(IServiceProvider)} isn't supported.");
                }

                instance ??=
                    ActivatorUtilities.CreateInstance(new HostServiceProvider(gtkHostBuilderContext), startupType);
                context.Properties[_startupKey] = instance;

                var configureServicesBuilder =
                    StartupLoader.FindConfigureServicesDelegate(startupType,
                        context.HostingEnvironment.EnvironmentName);
                var configureServices = configureServicesBuilder.Build(instance);

                configureServices(services);

                var configureContainerBuilder =
                    StartupLoader.FindConfigureContainerDelegate(startupType,
                        context.HostingEnvironment.EnvironmentName);
                if (configureContainerBuilder.MethodInfo != null)
                {
                    var containerType = configureContainerBuilder.GetContainerType();
                    _builder.Properties[typeof(ConfigureContainerBuilder)] = configureContainerBuilder;

                    var actionType = typeof(Action<,>).MakeGenericType(typeof(HostBuilderContext), containerType);

                    var configureCallback = typeof(GenericGtkHostBuilder).GetMethod(nameof(ConfigureContainerImpl),
                            BindingFlags.NonPublic | BindingFlags.Instance)!
                        .MakeGenericMethod(containerType)
                        .CreateDelegate(actionType, this);

                    typeof(IHostBuilder).GetMethod(nameof(IHostBuilder.ConfigureContainer))!
                        .MakeGenericMethod(containerType)
                        .InvokeWithoutWrappingExceptions(_builder, new object[] {configureCallback});
                }

                configureBuilder =
                    StartupLoader.FindConfigureDelegate(startupType, context.HostingEnvironment.EnvironmentName);
            }
            catch (Exception ex)when (gtkHostOptions.CaptureStartupErrors)
            {
                startupError = ExceptionDispatchInfo.Capture(ex);
            }

            services.Configure<GenericGtkHostServiceOptions>(options =>
            {
                options.ConfigureApplication = app =>
                {
                    // Throw if there was any errors initializing startup
                    startupError?.Throw();

                    // Execute Startup.Configure
                    if (instance != null && configureBuilder != null)
                    {
                        configureBuilder.Build(instance)(app);
                    }
                };
            });
        }

        private void ConfigureContainerImpl<TContainer>(HostBuilderContext context, TContainer container)
            where TContainer : notnull
        {
            var instance = context.Properties[_startupKey];
            var builder = (ConfigureContainerBuilder) context.Properties[typeof(ConfigureContainerBuilder)];
            builder.Build(instance)(container);
        }


        private GtkHostBuilderContext GetGtkHostBuilderContext(HostBuilderContext context)
        {
            if (!context.Properties.TryGetValue(typeof(GtkHostBuilderContext), out var contextVal))
            {
                var options = new GtkHostOptions(context.Configuration,
                    Assembly.GetEntryAssembly()?.GetName().Name ?? string.Empty);
                var webHostBuilderContext = new GtkHostBuilderContext
                {
                    Configuration = context.Configuration,
                    HostingEnvironment = new HostingEnvironment(),
                };
                webHostBuilderContext.HostingEnvironment.Initialize(context.HostingEnvironment.ContentRootPath,
                    options);
                context.Properties[typeof(GtkHostBuilderContext)] = webHostBuilderContext;
                context.Properties[typeof(GtkHostOptions)] = options;
                return webHostBuilderContext;
            }

            // Refresh config, it's periodically updated/replaced
            var webHostContext = (GtkHostBuilderContext) contextVal;
            webHostContext.Configuration = context.Configuration;
            return webHostContext;
        }

        private class HostServiceProvider : IServiceProvider
        {
            private readonly GtkHostBuilderContext _context;

            public HostServiceProvider(GtkHostBuilderContext context)
            {
                _context = context;
            }

            public object? GetService(Type serviceType)
            {
                if (serviceType == typeof(IHostEnvironment))
                {
                    return _context.HostingEnvironment;
                }

                if (serviceType == typeof(IConfiguration))
                {
                    return _context.Configuration;
                }

                return null;
            }
        }
    }
}