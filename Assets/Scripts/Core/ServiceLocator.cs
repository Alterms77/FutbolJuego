using System;
using System.Collections.Generic;
using UnityEngine;

namespace FutbolJuego.Core
{
    /// <summary>
    /// Lightweight static service locator used to decouple system access
    /// across the codebase without requiring singleton MonoBehaviours for
    /// every system.
    /// </summary>
    public static class ServiceLocator
    {
        private static readonly Dictionary<Type, object> services = new Dictionary<Type, object>();

        /// <summary>
        /// Registers <paramref name="service"/> under type <typeparamref name="T"/>.
        /// Overwrites any previously registered service of the same type.
        /// </summary>
        public static void Register<T>(T service) where T : class
        {
            if (service == null)
                throw new ArgumentNullException(nameof(service), $"[ServiceLocator] Cannot register null service of type {typeof(T).Name}.");

            services[typeof(T)] = service;
            Debug.Log($"[ServiceLocator] Registered {typeof(T).Name}");
        }

        /// <summary>
        /// Retrieves the registered service of type <typeparamref name="T"/>.
        /// Throws <see cref="InvalidOperationException"/> if none is registered.
        /// </summary>
        public static T Get<T>() where T : class
        {
            if (services.TryGetValue(typeof(T), out object service))
                return (T)service;

            throw new InvalidOperationException(
                $"[ServiceLocator] Service of type '{typeof(T).Name}' is not registered. " +
                "Ensure it is registered before use.");
        }

        /// <summary>
        /// Tries to retrieve the service of type <typeparamref name="T"/>.
        /// Returns <c>false</c> without throwing if the service is not registered.
        /// </summary>
        public static bool TryGet<T>(out T service) where T : class
        {
            if (services.TryGetValue(typeof(T), out object raw))
            {
                service = (T)raw;
                return true;
            }
            service = null;
            return false;
        }

        /// <summary>
        /// Removes the registered service of type <typeparamref name="T"/>.
        /// No-op if the type is not currently registered.
        /// </summary>
        public static void Unregister<T>() where T : class
        {
            if (services.Remove(typeof(T)))
                Debug.Log($"[ServiceLocator] Unregistered {typeof(T).Name}");
        }

        /// <summary>
        /// Returns <c>true</c> if a service of type <typeparamref name="T"/> is registered.
        /// </summary>
        public static bool IsRegistered<T>() where T : class => services.ContainsKey(typeof(T));

        /// <summary>Removes all registered services.</summary>
        public static void Clear()
        {
            services.Clear();
            Debug.Log("[ServiceLocator] All services cleared.");
        }
    }
}
