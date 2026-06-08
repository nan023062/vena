//****************************************************************************
// File: Debug.cs
// Author: Li Nan
// Date: 2023-05-27 12:00
// Version: 1.0
//****************************************************************************
using System.Diagnostics;
using System.Runtime.CompilerServices;
namespace Vena
{
    public static class Debug
    {
        public const string VENA_DEBUG = "VENA_DEBUG";
        
        public const string VENA_DEVELOP = "VENA_DEVELOP";
        
        public static IDebugImpl debugImpl;
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Log(in string log, int category = 0) => debugImpl?.Log(category, log);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Warning(in string log, int category = 0) => debugImpl?.Warning(category, log);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Error(in string log, int category = 0) => debugImpl?.Error(category, log);
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static  void Report(in string log, int category = 0) => debugImpl?.Report(category, log);
    }
    
    public interface IDebugExpand
    {
        int GetDebugCategory();
        
        string GetDebugExpandPrefix();
    }
    
    public static class DebugExtensions
    {
        [Conditional(Debug.VENA_DEBUG),MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Log(this IDebugExpand expand, in string log)
        {
            Debug.Log($"{expand.GetDebugExpandPrefix()}-{log}", expand.GetDebugCategory());
        }
        
        [Conditional(Debug.VENA_DEBUG),MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Warning(this IDebugExpand expand, in string log)
        {
            Debug.Warning($"{expand.GetDebugExpandPrefix()}-{log}", expand.GetDebugCategory());  
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Error(this IDebugExpand expand, in string log)
        {
            Debug.Error($"{expand.GetDebugExpandPrefix()}-{log}", expand.GetDebugCategory());  
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Report(this IDebugExpand expand, in string log)
        {
            Debug.Report($"{expand.GetDebugExpandPrefix()}-{log}", expand.GetDebugCategory());
        }
    }
    
    public interface IDebugImpl
    {
        void Log(int category, in string log);
        
        void Warning(int category, in string log);
        
        void Error(int category, in string log);
        
        void Report(int category, in string log);
    }
}