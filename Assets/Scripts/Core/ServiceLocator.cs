using System;
using System.Collections.Generic;
using UnityEngine;

namespace PixelVanguard.Core
{
    /// <summary>
    /// Global service locator for platform-agnostic dependency injection.
    /// Initialized during Bootstrap scene.
    /// </summary>
    public static class ServiceLocator
    {
        private static readonly Dictionary<Type, object> services = new Dictionary<Type, object>();
        private static bool isInitialized = false;

        public static bool IsInitialized => isInitialized;

        /// <summary>
        /// Register a service implementation. Call during Bootstrap only.
        /// </summary>
        public static void Register<T>(T implementation) where T : class
        {
            var type = typeof(T);
            if (services.ContainsKey(type))
            {
                Debug.LogWarning($"[ServiceLocator] Service {type.Name} already registered. Overwriting.");
            }
            services[type] = implementation;
        }

        /// <summary>
        /// Get a registered service. Throws if not found.
        /// </summary>
        public static T Get<T>() where T : class
        {
            var type = typeof(T);
            if (services.TryGetValue(type, out var service))
            {
                return service as T;
            }
            throw new Exception($"[ServiceLocator] Service {type.Name} not registered! Did Bootstrap complete?");
        }

        /// <summary>
        /// Check if a service is registered.
        /// </summary>
        public static bool Has<T>() where T : class
        {
            return services.ContainsKey(typeof(T));
        }

        /// <summary>
        /// Mark initialization as complete. Called by Bootstrap when all services are ready.
        /// </summary>
        public static void MarkAsInitialized()
        {
            isInitialized = true;
            Debug.Log($"[ServiceLocator] Initialized with {services.Count} services.");
        }

        /// <summary>
        /// Clear all services. For testing only.
        /// </summary>
        public static void Clear()
        {
            services.Clear();
            isInitialized = false;
        }
    }
}
