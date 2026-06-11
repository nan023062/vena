// -----------------------------------------------------------------------------
// Vena Core
// Core primitives for the Vena open-source Unity framework.
// Copyright (c) Nan Li.
// Licensed under the terms defined in the repository LICENSE file.
// -----------------------------------------------------------------------------

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