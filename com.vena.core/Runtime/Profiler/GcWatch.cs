//****************************************************************************
// File: GcWatch.cs
// Author: Li Nan
// Date: 2023-03-25 12:00
// Version: 1.0
//****************************************************************************

using System;

namespace Vena
{
    public readonly struct GcWatch : IDisposable
    {
        private readonly string _log;
        
        private readonly long _totalMemory;

        private static long k = 1024;

        private static long m = k * k;
    
        public GcWatch(string log)
        {
            _log = log;
            _totalMemory = GC.GetTotalMemory(true);
        }
    
        void IDisposable.Dispose()
        {
            long totalByte = GC.GetTotalMemory(true) - _totalMemory;
            if (totalByte < k)
            {
                UnityEngine.Debug.LogWarning( $"GcWatch_{_log} ( alloc {totalByte}B )"); 
            }
            else if( totalByte < m)
            {
                long kCount = totalByte / k;
                long bCount = totalByte % k;
                UnityEngine.Debug.LogWarning($"GcWatch_{_log} ( alloc {kCount}K/{bCount}B )"); 
            }
            else
            {
                long mCount = totalByte / m;
                totalByte %= m;
                long kCount = totalByte / k;
                long bCount = totalByte % k;
                UnityEngine.Debug.LogWarning($"GcWatch_{_log} ( alloc {mCount}M/{kCount}K/{bCount}B )"); 
            }
        }
    }
}