// -----------------------------------------------------------------------------
// Vena Core
// Core primitives for the Vena open-source Unity framework.
// Copyright (c) Nan Li.
// Licensed under the terms defined in the repository LICENSE file.
// -----------------------------------------------------------------------------

using System;
using System.Reflection;

namespace Vena
{
    public interface ITaskContext : IDisposable
    {
        void OnBeforeStart();
        
        void OnAfterFinish();
        
        void CopyFrom(ITaskContext context);
    }

    public interface ITaskContextWithTask
    {
        Type TaskNodeType { get; }
    }

    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = true)]
    public sealed class TaskContextAttribute : Attribute
    {
        public readonly Type TaskNodeType;

        public TaskContextAttribute(Type taskNodeType)
        {
            TaskNodeType = taskNodeType;
        }
    }
    
    internal static class TaskExtensions
    {
        public static Type GetExecuteClassType(this ITaskContext context)
        {
            if (context == null)
                throw new ArgumentNullException(nameof(context));

            Type taskNodeType = null;
            if (context is ITaskContextWithTask contextWithTask)
            {
                taskNodeType = contextWithTask.TaskNodeType;
            }
            else
            {
                taskNodeType = context.GetType().GetCustomAttribute<TaskContextAttribute>()?.TaskNodeType;
            }

            if (taskNodeType == null)
            {
                throw new InvalidOperationException(
                    $"{context.GetType().Name} must implement {nameof(ITaskContextWithTask)} or use {nameof(TaskContextAttribute)}.");
            }

            if (!typeof(TaskNode).IsAssignableFrom(taskNodeType))
            {
                throw new InvalidOperationException($"{taskNodeType.Name} must inherit from {nameof(TaskNode)}.");
            }

            if (taskNodeType.IsAbstract || taskNodeType.IsInterface)
            {
                throw new InvalidOperationException($"{taskNodeType.Name} must be a concrete {nameof(TaskNode)} type.");
            }

            return taskNodeType;
        }
    }
}
