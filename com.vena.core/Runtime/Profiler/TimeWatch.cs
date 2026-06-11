// -----------------------------------------------------------------------------
// Vena Core
// Core primitives for the Vena open-source Unity framework.
// Copyright (c) Nan Li.
// Licensed under the terms defined in the repository LICENSE file.
// -----------------------------------------------------------------------------

using System;
using System.Diagnostics;

namespace Vena
{
    public readonly struct TimeWatch : IDisposable
    {
        private readonly string _message;

        private readonly long _timestamp;
        
        public TimeWatch(string message)
        {
            _message = message;
            
            UnityEngine.Debug.Log($"[TimeWatch].Begin: {_message}");
            
            _timestamp = Stopwatch.GetTimestamp();
        }
        
        public void Dispose()
        {
            long ticks = Stopwatch.GetTimestamp() - _timestamp;
            
            float ms = (float)ticks / Stopwatch.Frequency * 1000f;
            
            UnityEngine.Debug.Log($"[TimeWatch].End: {_message} ------ {ms} ms!");
        }
    }
}