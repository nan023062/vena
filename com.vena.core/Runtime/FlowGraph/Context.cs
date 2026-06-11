// -----------------------------------------------------------------------------
// Vena Core
// Core primitives for the Vena open-source Unity framework.
// Copyright (c) Nan Li.
// Licensed under the terms defined in the repository LICENSE file.
// -----------------------------------------------------------------------------

using System;

namespace Vena
{
    public interface ITaskContext : IDisposable
    {
        void OnBeforeStart();
        
        void OnAfterFinish();
        
        void CopyFrom(ITaskContext context);
    }
    
    public static class TaskExtensions
    {
        public static Type GetExecuteClassType(this ITaskContext context)
        {
            return null;
        }
    }
}