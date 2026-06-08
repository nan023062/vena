using System;

namespace Vena.Framework
{
    public partial class GameWorld
    {
        private static ISceneLoader _SceneLoader;
        
        internal static ISceneHandle LoadSceneAsync(string sceneType)
        {
            return _SceneLoader.LoadSceneAsync(sceneType);
        }

        internal static void UnloadScene(ISceneHandle handle)
        {
            _SceneLoader.UnloadScene(handle);
        }
    }
    
    public interface ISceneHandle
    {
        string sceneType { get; }
        
        bool isDone { get; }
    }
    
    public interface ISceneLoader
    {
        ISceneHandle LoadSceneAsync(string sceneType);
        
        void UnloadScene(ISceneHandle handle);
    }
    
    /// <summary>
    /// define unity scene name, will load scene when level start
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, Inherited = true, AllowMultiple = false)]
    public sealed class UnitySceneAttribute : Attribute
    {
        public readonly string name;
    
        public UnitySceneAttribute(string name)
        {
            this.name = name;
        }
    }

    /// <summary>
    /// game scene controller
    /// </summary>
    internal class SceneController : IModule
    {
        private ISceneHandle _sceneHandle;

        public readonly string sceneName;
        
        EstimatedSeconds IModule.EstimatedSeconds => new EstimatedSeconds(2f, 1f);
         
        internal SceneController(string sceneName)
        {
            this.sceneName = sceneName;
        }
        
        Result IModule.Start()
        {
            Result result = default;
            
            if (null == _sceneHandle)
            {
                _sceneHandle = GameWorld.LoadSceneAsync(sceneName);
                
                return result;
            }
            
            result.Percent = 0.5f;
            
            result.IsDone = false;

            if (_sceneHandle.isDone)
            {
                result.Percent = 1f;
                
                result.IsDone = true;
            }
            
            return result;
        }
        
        Result IModule.Release()
        {
            Result result = default;
            
            result.Percent = 0.5f;
            
            if (null != _sceneHandle)
            {
               var tempHandle = _sceneHandle;
               
               _sceneHandle = null;
                
                GameWorld.UnloadScene(tempHandle);
                
                return result;
            }
            
            result.IsDone = true;
            
            result.Percent = 1f;
            
            return result;
        }
    }
}
