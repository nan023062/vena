using System;
using System.Collections.Generic;

namespace Vena
{
    public delegate void MessageHandler<T>(in T msg) where T : struct;
    
    /// <summary>
    /// The control layer of the game architecture
    /// 1. The base class of the control layer
    /// 2. The control layer is responsible for the logic of the game, and the control layer is divided into two parts: service and command
    /// </summary>
    public abstract class Controller : Actor
    {
        private Dictionary<Type, Delegate> _handlers;
        
        /// <summary>
        /// fire message to all listeners
        /// </summary>
        /// <param name="message"></param>
        /// <typeparam name="T"></typeparam>
        public void Fire<T>(in T message) where T : struct
        {
            if (null != _handlers && _handlers.TryGetValue(typeof(T), out var @delegate))
            {
                if (@delegate is MessageHandler<T> handler)
                {
                    handler(message);
                }
                else
                {
                    _handlers.Remove(typeof(T));
                }
            }
        }
        
        /// <summary>
        /// add message listener
        /// </summary>
        /// <param name="handler"></param>
        /// <typeparam name="T"></typeparam>
        public void AddListener<T>(MessageHandler<T> handler) where T : struct
        {
            _handlers ??= new Dictionary<Type, Delegate>();
            
            if (_handlers.TryGetValue(typeof(T), out var @delegate) && @delegate != null)
            {
                _handlers[typeof(T)] = Delegate.Combine(@delegate, handler);
                
                return;
            }
            
            _handlers[typeof(T)] = handler;
        }
        
        /// <summary>
        /// remove message listener
        /// </summary>
        /// <param name="handler"></param>
        /// <typeparam name="T"></typeparam>
        public void RemoveListener<T>(MessageHandler<T> handler) where T : struct
        {
            if (null != _handlers && _handlers.TryGetValue(typeof(T), out var  @delegate))
            {
                @delegate = Delegate.Remove(@delegate, handler);
                
                if (@delegate == null)
                {
                    _handlers.Remove(typeof(T));
                }
                else
                {
                    _handlers[typeof(T)] = @delegate;
                }
            }
        }
    }
}