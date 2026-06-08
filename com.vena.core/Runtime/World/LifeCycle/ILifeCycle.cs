namespace Vena
{
    public interface ICreate
    {
        void OnCreate();
    }

    public interface IStart
    {
        void OnStart();
    }

    public interface IUpdate
    {
        void Update(float deltaTime);
    }

    public interface ILateUpdate
    {
        void LateUpdate(float deltaTime);
    }

    [RevertOrder]
    public interface IBeforeDestroy
    {
        void OnBeforeDestroy();
    }

    [RevertOrder]
    public interface IDestroy
    {
        void OnDestroy();
    }

    /// <summary>
    /// APP事件
    /// </summary>
    public interface IApplicationFocus
    {
        void OnApplicationFocus(bool focus);
    }

    /// <summary>
    /// APP事件
    /// </summary>
    public interface IApplicationPause
    {
        void OnApplicationPause(bool pause);
    }
    
    /// <summary>
    /// APP事件
    /// </summary>
    public interface IApplicationQuit
    {
        void OnApplicationQuit();
    }

    public interface IGizmos
    {
        void OnDrawGizmos();
    }

    public interface IGUI
    {
        void OnGUI();
    }
}