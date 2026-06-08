namespace Vena
{
    public readonly struct JobResult
    {
        public static JobResult Done = new JobResult(true);

        public readonly bool isDone;
        
        public readonly string description;

        public JobResult(bool isDone, string description = "null")
        {
            this.isDone = isDone;
            
            this.description = description;
        }
    }

    /// <summary>
    /// 可分步的任务 step-by-step job
    /// 说明： 请控制Job.ExecuteStep函数的工作量-“perStepCost”
    ///       太小的工作量会导致任务执行频率过高，浪费CPU来执行循环
    ///       太大的工作量会导致任务切片过少，无法达到平衡的效果
    /// 1 目前经验值perStepCost控制在( 0.5ms - 3ms) 范围内
    /// 2 perStepCost越大。切片越少，会增加帧平衡的锯齿幅度，如果希望平衡度更高，可以减小最大值
    /// 3 perStepCost越小。切换越多，会增加单帧遍历切片频率，浪费CPU来执行循环
    /// </summary>
    public interface ISteppedJob
    {
        /// <summary>
        /// 优先级-越小越优先
        /// </summary>
        int priority { get; }

        /// <summary>
        /// 执行步骤-返回true表示还有下一步
        /// </summary>
        JobResult ExecuteStep();
    }
}