// -----------------------------------------------------------------------------
// Vena Framework
// Unity game framework layer built on Vena Core.
// Copyright (c) Nan Li.
// Licensed under the terms defined in the repository LICENSE file.
// -----------------------------------------------------------------------------

using System;

namespace Vena.Framework
{
    /// <summary>
    /// game state transition job
    /// </summary>
    public interface IGameStateJob
    {
        /// <summary>
        /// estimated cost seconds
        /// </summary>
        float estimatedSeconds { get; }
        
        /// <summary>
        /// name 
        /// </summary>
        string name { get; }
        
        /// <summary>
        /// run transition job
        /// </summary>
        /// <returns></returns>
        Result Run();
    }
    
    sealed class ActionJob : IGameStateJob
    {
        private readonly Action _action;
        
        private readonly string _name;

        public float estimatedSeconds => 0.033f;

        string IGameStateJob.name => _name;
        
        public ActionJob(Action action, string name)
        {
            _action = action;
            _name = name;
        }

        Result IGameStateJob.Run()
        {
            Result result = default;
            result.IsDone = true;
            result.Percent = 1f;
            result.ErrorCode = 0;
            _action.Invoke();
            return result;
        }
    }
    
    /// <summary>
    /// game state transition panel interface
    /// </summary>
    public interface IGameStateTransitionPanel
    {
        bool SetProgress(in TransitProgress progress);
    }
    
    public readonly struct EstimatedSeconds
    {
        public readonly float StartSeconds;
        public readonly float ReleaseSeconds;
        
        public EstimatedSeconds(float startEstimatedSeconds ,float releaseEstimatedSeconds = 0.01f)
        {
            StartSeconds = startEstimatedSeconds;
            ReleaseSeconds = releaseEstimatedSeconds;
        }
    }
    
    public struct TransitProgress
    {
        public float Percent;
        public string Name;
        public int Error;
        public string ErrorInfo;
    }
    
    public struct Result
    {
        public bool IsDone;
        public float Percent;
        public int ErrorCode;
        public string ErrorInfo;
    }
}