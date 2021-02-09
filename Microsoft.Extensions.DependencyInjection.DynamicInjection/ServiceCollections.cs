using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;

namespace Microsoft.Extensions.DependencyInjection.DynamicInjection
{
    public static class ServiceCollections
    {
        private static List<Assembly> _loadedAssemblies = new List<Assembly>();

        public static void AddServicesOfType<T>(this IServiceCollection serviceCollection, params string[] scanAssembliesStartsWith)
        {
            if (serviceCollection == null)
            {
                throw new ArgumentNullException(nameof(serviceCollection));
            }

            ServiceLifetime lifetime = ServiceLifetime.Scoped;

            switch (typeof(T).Name)
            {
                case nameof(ITransientService):
                    lifetime = ServiceLifetime.Transient;
                    break;
                case nameof(IScopedService):
                    lifetime = ServiceLifetime.Scoped;
                    break;
                case nameof(ISingletonService):
                    lifetime = ServiceLifetime.Singleton;
                    break;
                default:
                    throw new ArgumentException($"The type {typeof(T).Name} is not a valid type in this context.");
            }

            if (!_loadedAssemblies.Any())
            {
                LoadAssemblies(scanAssembliesStartsWith);
            }

            List<Type> implementations = _loadedAssemblies.SelectMany(assembly => assembly.GetTypes()).Where(type => typeof(T).IsAssignableFrom(type) && type.IsClass).ToList();

            foreach (Type implementation in implementations)
            {
                Type[] servicesToBeRegistered = implementation.GetInterfaces().Where(i => i != typeof(ITransientService) && i != typeof(IScopedService) && i != typeof(ISingletonService)).ToArray();

                if (servicesToBeRegistered.Any())
                {
                    foreach (Type serviceType in servicesToBeRegistered)
                    {
                        bool isGenericTypeDefinition = implementation.IsGenericType && implementation.IsGenericTypeDefinition;
                        Type service = isGenericTypeDefinition
                            && serviceType.IsGenericType
                            && serviceType.IsGenericTypeDefinition == false
                            && serviceType.ContainsGenericParameters
                                ? serviceType.GetGenericTypeDefinition()
                                : serviceType;

                        bool isAlreadyRegistered = serviceCollection.Any(s => s.ServiceType == service && s.ImplementationType == implementation);

                        if (!isAlreadyRegistered)
                        {
                            serviceCollection.Add(new ServiceDescriptor(service, implementation, lifetime));
                        }
                    }
                }
                else
                {
                    if (implementation.IsClass)
                    {
                        bool isAlreadyRegistered = serviceCollection.Any(s => s.ServiceType == implementation && s.ImplementationType == implementation);

                        if (!isAlreadyRegistered)
                        {
                            serviceCollection.Add(new ServiceDescriptor(implementation, implementation, lifetime));
                        }
                    }
                }
            }
        }

        public static void AddServicesAttributeOfType<T>(this IServiceCollection serviceCollection, params string[] scanAssembliesStartsWith)
        {
            if (serviceCollection == null)
            {
                throw new ArgumentNullException(nameof(serviceCollection));
            }

            ServiceLifetime lifetime = ServiceLifetime.Scoped;

            switch (typeof(T).Name)
            {
                case nameof(TransientServiceAttribute):
                    lifetime = ServiceLifetime.Transient;
                    break;
                case nameof(ScopedServiceAttribute):
                    lifetime = ServiceLifetime.Scoped;
                    break;
                case nameof(SingletonServiceAttribute):
                    lifetime = ServiceLifetime.Singleton;
                    break;
                default:
                    throw new ArgumentException($"The type {typeof(T).Name} is not a valid type in this context.");
            }

            if (!_loadedAssemblies.Any())
            {
                LoadAssemblies(scanAssembliesStartsWith);
            }

            List<Type> servicesToBeRegistered = _loadedAssemblies.SelectMany(assembly => assembly.GetTypes()).Where(type => type.IsDefined(typeof(T), false)).ToList();

            foreach (Type serviceType in servicesToBeRegistered)
            {
                List<Type> implementations = new List<Type>();

                if (serviceType.IsGenericType && serviceType.IsGenericTypeDefinition)
                {
                    implementations = _loadedAssemblies.SelectMany(a => a.GetTypes())
                    .Where(type => type.IsGenericType && type.IsClass && type.GetInterfaces()
                    .Any(i => i.IsGenericType && i.GetGenericTypeDefinition() == serviceType.GetGenericTypeDefinition()))
                    .ToList();
                }
                else
                {
                    implementations = _loadedAssemblies.SelectMany(a => a.GetTypes())
                    .Where(type => serviceType.IsAssignableFrom(type) && type.IsClass).ToList();
                }

                if (implementations.Any())
                {
                    foreach (Type implementation in implementations)
                    {
                        bool isGenericTypeDefinition = implementation.IsGenericType && implementation.IsGenericTypeDefinition;
                        Type service = isGenericTypeDefinition
                            && serviceType.IsGenericType
                            && serviceType.IsGenericTypeDefinition == false
                            && serviceType.ContainsGenericParameters
                                  ? serviceType.GetGenericTypeDefinition()
                                  : serviceType;

                        bool isAlreadyRegistered = serviceCollection.Any(s => s.ServiceType == service && s.ImplementationType == implementation);

                        if (!isAlreadyRegistered)
                        {
                            serviceCollection.Add(new ServiceDescriptor(service, implementation, lifetime));
                        }
                    }
                }
                else
                {
                    if (serviceType.IsClass)
                    {
                        bool isAlreadyRegistered = serviceCollection.Any(s => s.ServiceType == serviceType && s.ImplementationType == serviceType);

                        if (!isAlreadyRegistered)
                        {
                            serviceCollection.Add(new ServiceDescriptor(serviceType, serviceType, lifetime));
                        }
                    }
                }
            }
        }

        private static void LoadAssemblies(params string[] scanAssembliesStartsWith)
        {
            List<Assembly> loadedAssemblies = new List<Assembly>();

            List<string> assembliesToBeLoaded = new List<string>();

            if (scanAssembliesStartsWith != null && scanAssembliesStartsWith.Any())
            {
                if (scanAssembliesStartsWith.Length == 1)
                {
                    string searchPattern = $"{scanAssembliesStartsWith.First()}*.dll";
                    string[] assemblyPaths = Directory.GetFiles(AppDomain.CurrentDomain.BaseDirectory, searchPattern);
                    assembliesToBeLoaded.AddRange(assemblyPaths);
                }

                if (scanAssembliesStartsWith.Length > 1)
                {
                    foreach (string starsWith in scanAssembliesStartsWith)
                    {
                        string searchPattern = $"{starsWith}*.dll";
                        string[] assemblyPaths = Directory.GetFiles(AppDomain.CurrentDomain.BaseDirectory, searchPattern);
                        assembliesToBeLoaded.AddRange(assemblyPaths);
                    }
                }
            }
            else
            {
                string[] assemblyPaths = Directory.GetFiles(AppDomain.CurrentDomain.BaseDirectory, "*.dll");
                assembliesToBeLoaded.AddRange(assemblyPaths);
            }

            foreach (string path in assembliesToBeLoaded)
            {
                try
                {
                    Assembly assembly = Assembly.LoadFrom(path);
                    loadedAssemblies.Add(assembly);
                }
                catch (Exception)
                {
                    continue;
                }
            }

            _loadedAssemblies = loadedAssemblies;
        }
    }
}
