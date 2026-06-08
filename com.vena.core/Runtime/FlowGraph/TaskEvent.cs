
namespace Vena
{
    public interface IHasLogicEventClip
    {
        TaskEvent[] GetEvents();

        void ExecuteEvent(int eventId);
    }

    public enum EventType
    {
        /// <summary>
        /// 可选执行
        /// </summary>
        Optional,

        /// <summary>
        /// 必须执行 
        /// </summary>
        Required
    }

    public readonly struct TaskEvent
    {
        public readonly EventType type;
        public readonly float time;
        public readonly int eventId;

        /// <summary>
        /// 事件片段
        /// </summary>
        /// <param name="eventId"> 事件id </param>
        /// <param name="type"> 事件类型 </param>
        /// <param name="time"> 执行时间 </param>
        public TaskEvent(int eventId, EventType type = EventType.Required, float time = -1f)
        {
            this.time = time;
            this.eventId = eventId;
            this.type = type;
        }
    }
}