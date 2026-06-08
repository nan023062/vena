using System;
using System.Collections.Generic;

namespace Vena
{
    public interface IChainJob
    {
        void Start(SteppedJobChain chain);

        JobResult ExecuteStep();

        void End();
    }

    /// <summary>
    /// 任务链
    /// </summary>
    public abstract class SteppedJobChain : ISteppedJob
    {
        private IChainJob _current;

        private Queue<IChainJob> _executing;

        private Queue<IChainJob> _executed;

        public abstract int priority { get; }

        protected SteppedJobChain(params IChainJob[] jobs)
        {
            _executing = new Queue<IChainJob>(jobs);
            
            _executed = new Queue<IChainJob>(jobs.Length);
            
            _current = null;
        }

        JobResult ISteppedJob.ExecuteStep()
        {
            if (null == _current)
            {
                if (_executing.Count <= 0)
                {
                    return JobResult.Done;
                }

                _current = _executing.Dequeue();
                
                try
                {
                    _current.Start(this);
                }
                catch (Exception e)
                {
                    var chainJob = _current;
                    
                    _executed.Enqueue(_current);
                    
                    _current = null;
                    
                    chainJob.End();
                    
                    throw new Exception($"{chainJob} : Start() has exception !", e);
                }

                return new JobResult(false);
            }

            try
            {
                var result = _current.ExecuteStep();
                
                if (result.isDone)
                {
                    var chainJob = _current;
                    
                    _executed.Enqueue(_current);
                    
                    _current = null;
                    
                    chainJob.End();
                }

                return new JobResult(false, result.description);
            }
            catch (Exception e)
            {
                var chainJob = _current;
                
                _executed.Enqueue(_current);
                
                _current = null;
                
                chainJob?.End();
                
                throw new Exception($"{chainJob} : ExecuteStep() has exception !", e);
            }
        }

        protected void ResetJobChain()
        {
            while (_executing.Count > 0)
            {
                _executed.Enqueue(_executing.Dequeue());
            }

            (_executing, _executed) = (_executed, _executing);
        }
    }
}