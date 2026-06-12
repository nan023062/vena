// -----------------------------------------------------------------------------
// Vena Framework
// Unity game framework layer built on Vena Core and Vena World.
// Copyright (c) Nan Li.
// Licensed under the terms defined in the repository LICENSE file.
// -----------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

namespace Vena.Framework
{
    public interface IGameTransition
    {
        void Start();
    
        bool IsReady();

        bool SetProgress(in TransitProgress progress);
    
        void Finish();
    }
    
    struct TransitArgs
    {
        public readonly BasedStackGameState Prev, Next;
        
        public readonly Queue<IGameStateJob> Jobs;
        
        public float TotalSeconds;
        
        public TransitArgs(BasedStackGameState prev, BasedStackGameState next)
        {
            Prev = prev;
            Next = next;
            TotalSeconds = 0f;
            Jobs = new Queue<IGameStateJob>();
        }

        public override string ToString()
        {
            string prev = null == Prev ? "Null" : Prev.GetType().Name;
            string next = null == Next ? "Null" : Next.GetType().Name;
            return $"Transition<{prev} - {next}>";
        }
    }
    
    /// <summary>
    /// game state transition
    /// </summary>
    sealed class Transition : IDisposable
    {
        private static readonly bool DebugLog = true;
        private readonly IGameTransition _transition;
        private TransitArgs _arg;
        private TransitProgress _progress;
        private float _startTime, _start, _end;
        private float _smoothProgress;
        private bool _disposed;
        
        static void Log(string message)
        {
            if (DebugLog)
            {
                Debug.Log(message);
            }
        }
        
        public Transition(IGameTransition transition)
        {
            _transition = transition;
            _smoothProgress = 0;
            _disposed = false;
        }
        
        public async Task Run(TransitArgs args)
        {
            _startTime = Time.time;
            _progress = default;
            _smoothProgress = 0;
            _start = _end = 0f;
            _arg = args;
            
            Debug.Log($"[Game]: {_arg}.Start!");

            if (null != _transition)
            {
                _transition.Start();

                while (!_transition.IsReady())
                {
                    await Task.Yield();
                    if(_disposed) return;
                }
            }

            
            // do transition
            TransitProgress progress = default;
            progress.Percent = 0f;
            progress.Error = 0;
            
            float stepTotalSeconds = 0f;
            while (_arg.Jobs.Count > 0 && progress.Error == 0)
            {
                var job = _arg.Jobs.Dequeue();
                float estimatedSeconds = job.estimatedSeconds;
                stepTotalSeconds += estimatedSeconds;
                _end = stepTotalSeconds / _arg.TotalSeconds;
                progress.Percent = 0;
                progress.Name = job.name;
                Result result = default;
                try
                {
                    float stepStartTime = Time.time;
                    float timeOut = Mathf.Clamp(estimatedSeconds, 5, estimatedSeconds * 10f);
                    Log($"[Game]: {progress.Name} Start!");
                    while (!result.IsDone && progress.Error == 0)
                    {
                        result = job.Run();
                        float stepElapsed = Time.time - stepStartTime;
                        if(timeOut < stepElapsed)
                        {
                            progress.Error = -1;
                            progress.ErrorInfo = "TimeOut";
                            Debug.Error( $"[Game]: {progress.Name} TimeOut! {timeOut:F2}/{stepElapsed:F2}");
                            break;
                        }
                        float percent = Mathf.Max(Mathf.Clamp01(stepElapsed / estimatedSeconds), result.Percent);
                        progress.Percent = result.IsDone ? 1f : percent;
                        progress.Error = result.ErrorCode;
                        progress.ErrorInfo = result.ErrorInfo;
                        HandleProgress(progress);
                        await Task.Yield();
                        if(_disposed) return;
                    }

                    float stepCostTime = Time.time - stepStartTime;
                    Log($"[Game]: {progress.Name} Finish! TIME : {stepCostTime:F2}/ {estimatedSeconds:F2}");
                }
                catch (Exception e)
                {
                    progress.Percent = 1f;
                    progress.Error = -1;
                    progress.ErrorInfo = $"{e}";
                    HandleProgress(progress);
                    break;
                }
                
                _start = _end;
            }
            
            // finish, wait progress done...
            progress.Percent = 1f;
            while (progress.Error == 0 && !HandleProgress(progress))
            {
                await Task.Yield();
                if(_disposed) return;
            }
            
            if (progress.Error != 0)
            {
                Debug.Error($"[Game]: {_arg}.error={progress.Error},info={progress.ErrorInfo}");
            }
            else
            {
                float totalCostTime = Time.time - _startTime;
                Debug.Log($"[Game]: {_arg}.Finish! TIME : {totalCostTime:F2}/{_arg.TotalSeconds:F2}");
            }
            
            await Task.Yield();
            if(_disposed) return;
            _transition?.Finish();
        }
        
        private bool HandleProgress(in TransitProgress progress)
        {
            float percent = Mathf.Lerp(_start, _end, progress.Percent);
            bool isDone = Mathf.Approximately(percent, 1f);
            float current;
            if (isDone)
            {
                current = Mathf.MoveTowards( _smoothProgress, 1f, 2f * Time.deltaTime);
            }
            else
            {
                current = Mathf.SmoothStep(_smoothProgress, percent, 0.5f * Time.deltaTime);
            }
            
            current = Mathf.Clamp(current, 0f, 1f);
           
            if (isDone || progress.Error != 0 ||
                Mathf.Abs(current - _smoothProgress) > 0.01f)
            {
                _smoothProgress = current;
                _progress = progress;
                _progress.Percent = _smoothProgress;
                bool finished;
                try
                {
                    Log($"[Game] {_progress.Name} ({progress.Percent:P} / {_progress.Percent:P})");
                    finished = _transition?.SetProgress(_progress) ?? true;
                }
                catch (Exception e)
                {
                    finished = true;
                    Debug.Error($"[Game] {e}");
                }
                
                return finished && isDone;
            }
            
            return false;
        }

        public void Dispose()
        {
            // TODO release managed resources here
            _disposed = true;
        }
    }
}