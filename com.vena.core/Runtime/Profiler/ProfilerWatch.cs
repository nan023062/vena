//****************************************************************************
// File: ProfilerWatch.cs
// Author: Li Nan
// Date: 2023-01-05 12:00
// Version: 1.0
//****************************************************************************

using System;
using UnityEngine.Profiling;

namespace Vena
{
    public readonly struct ProfilerWatch : IDisposable
    {
        public ProfilerWatch(string message)
        {
#if VENA_DEVELOP
            Profiler.BeginSample(message);
#endif
        }
        
        public void Dispose()
        {
#if VENA_DEVELOP
            Profiler.EndSample();
#endif
        }
    }
}