// -----------------------------------------------------------------------------
// Vena Framework
// Unity game framework layer built on Vena Core.
// Copyright (c) Nan Li.
// Licensed under the terms defined in the repository LICENSE file.
// -----------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Vena.Framework
{
    internal interface ICommand
    {
    }
    
    /// <summary>
    /// command
    /// </summary>
    /// <typeparam name="T"> message type </typeparam>
    public abstract class Command<T> : ICommand where T : struct
    {
        public abstract Task<int> Execute(T msg);
    }
    
    public partial class GameWorld
    {
        static readonly Dictionary<Type, ICommand> _commands = new Dictionary<Type, ICommand>();
        
        private static ICommand _executeCommand;
        
        private static void InitializeCommands(Type[] types)
        {
            using var _ = new TimeWatch("init game commands");
            
            foreach (var type in types)
            {
                Type baseType = type.BaseType;
                if (null == baseType)
                {
                    continue;
                }
                
                // 继承至Command<T>
                if ( type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Command<>))
                {
                    Type msgType = baseType.GenericTypeArguments[0];
                    
                    _commands.Add(msgType, (ICommand)Activator.CreateInstance(type));
                }
            }
        }
        
        public static async Task<int> Execute<T>(T msg) where T : struct
        {
            if (null != _executeCommand)
            {
                throw new Exception( "Game is executing command!");
            }
            
            Type type = typeof(T);
            
            if (_commands.TryGetValue(type, out var command))
            {
                _executeCommand = command;
                
                try
                {
                    return await ((Command<T>)command).Execute(msg);
                }
                finally
                {
                    _executeCommand = null;
                }
            }
            
            throw new Exception($"Command {type.Name} is not exist!");
        }
    }
}