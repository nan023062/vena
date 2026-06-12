// -----------------------------------------------------------------------------
// Vena Framework
// Unity game framework layer built on Vena Core and Vena World.
// Copyright (c) Nan Li.
// Licensed under the terms defined in the repository LICENSE file.
// -----------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Vena.Framework
{
    public partial class GameWorld
    {
        private static readonly Dictionary<Type, Service> _services = new Dictionary<Type, Service>();
        
        /// <summary>
        /// initialize services
        /// create all Service instances by reflection
        /// inject all Service referenced by Service by reflection
        /// </summary>
        /// <param name="serviceTypes"></param>
        /// <exception cref="Exception"></exception>
        private static void InitializeServices(Type[] serviceTypes)
        {
            using var tw = new TimeWatch("init game service");
            
            Type serviceType = typeof(Service);

            Dictionary<Service, Dictionary<Type, FieldInfo>> injectFields = new Dictionary<Service, Dictionary<Type, FieldInfo>>();
            
            _services.Clear();
            
            foreach (var type in serviceTypes)
            {
                Service service = World.Default.CreateActor(type) as Service;
                if (null == service)
                {
                    throw new Exception($"[{type}] is not inherit from Service");
                }
                
                // inject define controller fields
                Dictionary<Type, FieldInfo> fields = new Dictionary<Type, FieldInfo>();
                
                injectFields.Add(service, fields);
                
                _services.Add(type, service);

                type.GetFields(BindingFlags.NonPublic | BindingFlags.Instance)
                    .Where(fieldInfo => fieldInfo.GetCustomAttribute<InjectFieldAttribute>() != null)
                    .ToList().ForEach(fieldInfo => {
                        if (fieldInfo.FieldType.IsSubclassOf(serviceType) && !fieldInfo.FieldType.IsAbstract)
                        {
                            fields.Add(fieldInfo.FieldType, fieldInfo);
                        }
                    });
            }

            // sort by InjectFieldAttribute define dependency
            List<Service> serviceList = new List<Service>();
            
            serviceList.AddRange(injectFields.Keys);
            
            serviceList.Sort((a, b) =>
            {
                bool aHasInjectB = injectFields[a].ContainsKey(b.GetType());
                
                bool bHasInjectA = injectFields[b].ContainsKey(a.GetType());

                if (aHasInjectB == bHasInjectA) return 0;
                
                return aHasInjectB ? 1 : -1;
            });

            foreach (var service in serviceList)
            {
                // init all InjectField
                foreach (var fieldInfo in injectFields[service].Values)
                {
                    fieldInfo.SetValue(service, _services[fieldInfo.FieldType]);
                }
                
                using var _ = new TimeWatch($"{service.GetType()}.init");
                
                ((IService)service).Startup();
            }
        }
    }

    internal interface IService
    {
        void Startup();
        
        void Shutdown();
    }
    
    /// <summary>
    /// game service class, also a kind of actor, support composite mode
    /// is the core functional module of the game, and the service will be automatically created and started when the game starts
    /// service can declare the function implementation required, the game will automatically call
    /// service can declare the other services required, the game will automatically inject
    /// </summary>
    public abstract class Service : Controller, IService
    {
        void IService.Startup()
        {
            Startup();
        }

        void IService.Shutdown()
        {
            Shutdown();
        }
        
        protected abstract void Startup();
        
        protected abstract void Shutdown();
    }
}