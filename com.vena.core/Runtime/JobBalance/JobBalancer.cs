// ***********************************************************************************
// * Author : LiNan
// * File : JobBalance.cs
// * Description : for balance job execute in frame
// * Date : 2023-03-23-13:55
// ************************************************************************************

using System;
using UnityEngine;
using System.Diagnostics;
using System.Collections.Generic;

namespace Vena
{
    /// <summary>
    /// FPS稳定器 / 任务调度器
    /// 根据参数targetFrameRate，对需分切片任务进行单帧执行次数平衡，稳定帧率的作用
    /// 任务可以设置priority, 按优先级次序执行任务
    /// 说明：
    ///  1 这里执行对同步逻辑进行预估平衡，无法预测异步任务的CPU消耗， 需要将更多的异步任务使用BalanceJob实现
    ///  2 想要达到较好的消耗，需要对任务的perStepCost进行调优，参考IBalanceJob的注释
    ///  3 目前通过ticksPerFrame >> 2，保证每帧至少执行1/8的时间，避免leftoverTicks不足时卡住任务执行
    /// todo：1 后期需进一步根据业务需求，对任务进行分组，分组内任务按优先级执行，分组间任务按优先级执行
    /// </summary>
    public sealed class FpsBalance
    {
        private long _targetTimestamp;
        
        private int _targetFrameRate;
        
        private long _ticksPerFrame;
        
        private readonly Stopwatch _stepWatch;
        
        private readonly Stopwatch _jobWatch;
        
        private readonly PriorityQueue<JobExecutor> _queue = new PriorityQueue<JobExecutor>(128);
        
        private readonly HashSet<ISteppedJob> _hashSet = new HashSet<ISteppedJob>();

        public int targetFrameRate
        {
            get => _targetFrameRate;
            set
            {
                if (_targetFrameRate != value)
                {
                    _targetFrameRate = value;
                    
                    _ticksPerFrame = Mathf.FloorToInt(1f / _targetFrameRate * Stopwatch.Frequency);
                }
            }
        }

        public FpsBalance(int targetFrameRate = 60)
        {
            this.targetFrameRate = targetFrameRate;
            
            _targetTimestamp = Stopwatch.GetTimestamp();
            
            _stepWatch = new Stopwatch();
            
            _jobWatch = new Stopwatch();
        }

        /// <summary>
        /// 执行可分帧的任务
        /// </summary>
        /// <param name="job"></param>
        /// <returns></returns>
        public void DoJob(ISteppedJob job)
        {
            if (_hashSet.Add(job))
            {
                var jobExecutor = new JobExecutor(job);
                
                _queue.Enqueue(jobExecutor);
            }
        }

        /// <summary>
        /// 立即完成任务
        /// </summary>
        /// <param name="job"></param>
        public void DoneJob(ISteppedJob job)
        {
            _hashSet.Remove(job);

            bool isBreak = false;
            
            while (!isBreak)
            {
                JobResult result = default;
                try
                {
                    result = job.ExecuteStep();
                }
                catch (Exception e)
                {
                    result = JobResult.Done;
                    
                    throw new Exception($"{job} : ExecuteStep() has exception !", e);
                }
                finally
                {
                    isBreak = result.isDone;
                }
            }
        }

        /// <summary>
        /// 取消任务
        /// </summary>
        /// <param name="job"></param>
        /// <returns></returns>
        public bool Cancel(ISteppedJob job)
        {
            return _hashSet.Remove(job);
        }

        /// <summary>
        /// 是否有任务
        /// </summary>
        /// <param name="job"></param>
        /// <returns></returns>
        public bool HasJob(ISteppedJob job)
        {
            return _hashSet.Contains(job);
        }

        /// <summary>
        /// 立即执行所有任务
        /// </summary>
        public void Flush()
        {
            while (_queue.Count > 0)
            {
                var job = _queue.Peek().job;
                
                if (null == job || !_hashSet.Contains(job))
                {
                    _queue.Dequeue();
                }
                else
                {
                    bool isBreak = false;
                    
                    while (!isBreak)
                    {
                        JobResult result = default;
                        try
                        {
                            result = job.ExecuteStep();
                        }
                        catch (Exception e)
                        {
                            result = JobResult.Done;
                            
                            throw new Exception($"{job} : ExecuteStep() has exception !", e);
                        }
                        finally
                        {
                            if (result.isDone)
                            {
                                isBreak = true;
                                
                                _queue.Dequeue();
                                
                                _hashSet.Remove(job);
                            }
                        }
                    }
                }
            }
        }

        /// <summary>
        /// 清空所有任务
        /// </summary>
        public void Clean()
        {
            _queue.Clear();
            
            _hashSet.Clear();
        }

        /// <summary>
        /// lateUpdate执行任务
        /// </summary>
        /// <param name="delta"></param>
        public void LateUpdate(float delta)
        {
            // atLeastTicks: 保证每帧至少执行1/4的时间
            long atLeastTicks = _ticksPerFrame >> 2;
            
            long leftoverTicks = _targetTimestamp - Stopwatch.GetTimestamp();
            
            if (atLeastTicks > leftoverTicks)
                
                leftoverTicks = atLeastTicks;

            // 外循环更换 job
            while (_queue.Count > 0 && leftoverTicks > 0)
            {
                var job = _queue.Peek().job;
                
                // check delay removed job
                if (null == job || !_hashSet.Contains(job))
                {
                    _queue.Dequeue();
                }
                else
                {
#if UNITY_EDITOR
                    //using var prf = new ProfilerWatch($"{job}.ExecuteStep");
                    //using var jw = new JobWatch(job, _jobWatch);
#endif
                    // 内循环执行job的多切片
                    bool isBreak = false;
                    
                    while (!isBreak && leftoverTicks > 0)
                    {
                        JobResult result = default;
                        
                        try
                        {
                            _stepWatch.Restart();
                            
                            result = job.ExecuteStep();
#if UNITY_EDITOR
                            //jw.Increment();
#endif
                        }
                        catch (Exception e)
                        {
                            result = JobResult.Done;
                            
                            throw new Exception($"{job} : ExecuteStep() has exception !", e);
                        }
                        finally
                        {
                            _stepWatch.Stop();
                            
                            leftoverTicks -= _stepWatch.ElapsedTicks;
                            
                            //float ms = _stepWatch.ElapsedTicks / (float)Stopwatch.Frequency * 1000;
                            //Debug.Log($"JobBalancer:ExecuteStep({result.description}), cost {ms} ms");
                            if (result.isDone)
                            {
                                isBreak = true;
                                
                                _queue.Dequeue();
                                
                                _hashSet.Remove(job);
                            }
                        }
                    }
                }
            }

            // next running destination timestamp
            _targetTimestamp = Stopwatch.GetTimestamp() + _ticksPerFrame;
        }

        /// <summary>
        ///  Profiler一个Job在单帧执行情况
        ///  1. 执行次数
        ///  2. 执行时间
        /// </summary>
        struct JobWatch : IDisposable
        {
            private readonly ISteppedJob _job;
            
            private readonly Stopwatch _stopwatch;
            
            private int _times;

            public void Increment() => _times++;

            public JobWatch(ISteppedJob job, Stopwatch stopwatch)
            {
                _times = 0;
                
                _job = job;
                
                _stopwatch = stopwatch;
                
                _stopwatch.Restart();
            }

            public void Dispose()
            {
                _stopwatch.Stop();
                
                float ms = _stopwatch.ElapsedTicks / (float)Stopwatch.Frequency * 1000;
                
                Debug.Log($"JobWatch: {_job},  times={_times}, {ms}ms");
            }
        }

        /// <summary>
        /// 任务执行器，用于优先级排序
        /// </summary>
        struct JobExecutor : IComparable<JobExecutor>
        {
            public readonly ISteppedJob job;

            public JobExecutor(ISteppedJob job)
            {
                this.job = job;
            }

            int IComparable<JobExecutor>.CompareTo(JobExecutor other)
            {
                if (job == other.job) return 0;
                
                return job.priority.CompareTo(other.job.priority);
            }
        }
    }
}