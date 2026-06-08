
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