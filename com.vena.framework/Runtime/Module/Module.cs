// -----------------------------------------------------------------------------
// Vena Framework
// Unity game framework layer built on Vena Core.
// Copyright (c) Nan Li.
// Licensed under the terms defined in the repository LICENSE file.
// -----------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
// ReSharper disable Unity.PerformanceCriticalCodeInvocation
// ReSharper disable ParameterHidesMember

namespace Vena.Framework
{
    public partial class GameWorld
    {
        static readonly Dictionary<Type, Dictionary<Type, FieldInfo>>  injectFieldInfos = new Dictionary<Type, Dictionary<Type, FieldInfo>>(); 
        
        static readonly Dictionary<Type, List<Type>> stateModuleTypes = new Dictionary<Type, List<Type>>();
        
        static readonly Dictionary<Type, List<Type>> stateModelTypes = new Dictionary<Type, List<Type>>();
        
        static readonly Dictionary<Type, HashSet<Type>> moduleChildren = new Dictionary<Type, HashSet<Type>>();
        
        static readonly Dictionary<Type, ModuleNode> moduleDict = new Dictionary<Type, ModuleNode>();
        
        private static void InitializeModules(Type[] stateTypes, Type[] moduleTypes)
        {
            using var _ = new TimeWatch("init game module");
                
            stateModuleTypes.Clear();
            
            stateModelTypes.Clear();
            
            moduleChildren.Clear();
            
            foreach (var stateType in stateTypes)
            {
                stateModuleTypes.Add(stateType, new List<Type>());
                
                stateModelTypes.Add(stateType, new List<Type>());
            }
            
            injectFieldInfos.Clear();
            
            foreach (var type in moduleTypes)
            {
                // find inject dependencies
                var injectFields = new Dictionary<Type, FieldInfo>();
                
                injectFieldInfos.Add(type, injectFields);
                
                GetAllPrivateInjectFields(type).ForEach(fieldInfo =>
                    {
                        if (!fieldInfo.FieldType.IsAbstract)
                        {
                            if(injectFields.ContainsKey(fieldInfo.FieldType)) 
                                throw new Exception($"[{type}] has repeat inject field [{fieldInfo.FieldType}]");
                            injectFields.Add(fieldInfo.FieldType, fieldInfo);
                        }
                    });
                
                // find children modules
                var attribute = type.GetCustomAttribute<GroupAttribute>();
                
                if (attribute != null)
                {
                    bool isModel = typeof(IDataModule).IsAssignableFrom(type);
                    
                    bool parentIsModel = typeof(IDataModule).IsAssignableFrom(attribute.groupType);

                    if (isModel == parentIsModel)
                    {
                        if(!moduleChildren.TryGetValue(attribute.groupType , out var hashSet))
                        {
                            hashSet = new HashSet<Type>();
                        
                            moduleChildren.Add(attribute.groupType, hashSet);
                        }
                    
                        hashSet.Add(type);
                    }
                    else
                    {
                        Debug.Warning( $"[{type}] define group '{attribute.groupType}' must be the same type");
                    }
                }

                // find owner state
                Type baseType = type.BaseType;
                
                while (baseType != null)
                {
                    if (baseType.IsGenericType)
                    {
                        Type defineType = baseType.GetGenericTypeDefinition();

                        if (defineType == typeof(ViewModule<>))
                        {
                            Type stateType = baseType.GetGenericArguments()[0];
                            
                            if (!stateModuleTypes.TryGetValue(stateType, out List<Type> typeList))
                            {
                                throw new Exception( $"[{stateType}] is not a valid game state type");
                            }
                             
                            typeList.Add(type);
                        }
                        else if (defineType == typeof(DataModule<>))
                        {
                            Type stateType = baseType.GetGenericArguments()[0];
                            
                            if (!stateModelTypes.TryGetValue(stateType, out List<Type> typeList))
                            {
                                throw new Exception( $"[{stateType}] is not a valid game state type");
                            }
                             
                            typeList.Add(type);
                        }
                        
                        break;
                    }
                    
                    baseType = baseType.BaseType;
                }
            }
        }
        
        static bool IsRequire(Type origin, Type requireType)
        {
            if (injectFieldInfos.TryGetValue(origin, out var dictionary))
            {
                if (dictionary.ContainsKey(requireType))
                {
                    return true;
                }

                foreach (Type key in dictionary.Keys.ToArray())
                {
                    if (CheckRequire(origin, key, requireType))
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        static bool CheckRequire(Type origin, Type srcType, Type dstType)
        {
            if (origin == srcType)
            {
                throw new Exception($"{origin.Name} 与 {dstType.Name} 循环依赖！！");
            }
            
            if (injectFieldInfos.TryGetValue(srcType, out var dictionary))
            {
                if (dictionary.ContainsKey(dstType))
                {
                    return true;
                }

                foreach (Type key in dictionary.Keys.ToArray())
                {
                    if (CheckRequire(origin, key, dstType))
                    {
                        return true;
                    }
                }
            }

            return false;
        }
        
        static List<FieldInfo> GetAllPrivateInjectFields(Type type)
        {
            if (type == null)
            {
                return new List<FieldInfo>();
            }

            BindingFlags flags = BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly;
            
            var fields = new List<FieldInfo>(type.GetFields(flags).Where(t=>t.GetCustomAttribute<InjectFieldAttribute>() != null));
            
            // 获取父类的字段
            fields.AddRange(GetAllPrivateInjectFields(type.BaseType));
            
            return fields;
        }

        internal static List<IModule> RegisterModules(BasedStackGameState stackState)
        {
            Type gameStateType = stackState.GetType();

            ModuleNode root = default;
            
            // default each level set SceneController
            if (gameStateType.IsSubclassOf(typeof(GameLevel)))
            {
                var attr = stackState.GetType().GetCustomAttribute<UnitySceneAttribute>();

                if (attr == null)
                {
                    throw new Exception($"[{gameStateType}] must have a BindSceneAttribute");
                }
                
                root = new ModuleNode(new SceneController(attr.name));
            }
            
            // create modules
            if (stateModuleTypes.TryGetValue(gameStateType, out List<Type> types))
            {
                foreach (var moduleType in types)
                {
                    if (!moduleDict.TryGetValue(moduleType, out var node))
                    {
                        node = new ModuleNode((IModule)World.Default.CreateActor(moduleType));
                        
                        moduleDict.Add(moduleType, node);
                    }
                    
                    // init children modules
                    if(moduleChildren.TryGetValue( moduleType, out var hashSet))
                    {
                        foreach (var childType in hashSet)
                        {
                            if (!moduleDict.TryGetValue(childType, out var childNode))
                            {
                                IModule module = (IModule)World.Default.CreateActor(childType);
                                
                                childNode = new ModuleNode(module);
                                    
                                moduleDict.Add(childType, childNode);
                            }
                            
                            childNode.SetParent(node);
                        }
                    }
                }
            }
            
            root ??= new ModuleNode(null);
            
            foreach (var node in moduleDict.Values)
            {
                if (null == node.parent)
                {
                    node.SetParent(root);
                }
            }
            
            List<IModule> sorted = root.GetSorted();
            
            // inject module fields
            foreach (var module in sorted)
            {
                if(injectFieldInfos.TryGetValue(module.GetType(), out var dictionary))
                {
                    foreach (var keyValue in dictionary)
                    {
                        Type injectType = keyValue.Key;
                        
                        FieldInfo fieldInfo = keyValue.Value;
                        
                        if (moduleDict.TryGetValue(injectType, out var node))
                        {
                            fieldInfo.SetValue(module, node.module);
                        }
                        else if (_services.TryGetValue(injectType, out var service))
                        {
                            fieldInfo.SetValue(module, service);
                        }
                        else if (injectType == gameStateType)
                        {
                            fieldInfo.SetValue(module, stackState);
                        }
                    }
                }
            }
            
            return sorted;
        }
        
        internal static void UnRegisterModules(BasedStackGameState stackState)
        {
            Type gameStateType = stackState.GetType();
            
            if (stateModuleTypes.TryGetValue(gameStateType, out List<Type> types))
            {
                foreach (var moduleType in types)
                {
                    if (moduleDict.TryGetValue(moduleType, out var node))
                    {
                        if (node.module is Controller controller)
                        {
                            controller.Destroy(); 
                        }
                        moduleDict.Remove(moduleType);
                    }
                }
            }
        }
        
        internal static List<IModule> RegisterModels(BasedStackGameState stackState)
        {
            Type gameStateType = stackState.GetType();
            
            // create models
            if (stateModelTypes.TryGetValue(gameStateType, out List<Type> types))
            {
                foreach (var modelType in types)
                {
                    if(!moduleDict.TryGetValue(modelType, out var node))
                    {
                        node = new ModuleNode((IDataModule)World.Default.CreateActor(modelType));
                    
                        moduleDict.Add(modelType, node);
                    }
                    
                    // init children modules
                    if(moduleChildren.TryGetValue( modelType, out var hashSet))
                    {
                        foreach (var childType in hashSet)
                        {
                            if (!moduleDict.TryGetValue(childType, out var childNode))
                            {
                                childNode = new ModuleNode( (IModule)World.Default.CreateActor(childType));
                                    
                                moduleDict.Add(childType, childNode);
                            }
                            
                            childNode.SetParent(node);
                        }
                    }
                }
            }
            
            ModuleNode root = new ModuleNode(null);
            
            foreach (var node in moduleDict.Values)
            {
                if (null == node.parent)
                {
                    node.SetParent(root);
                }
            }
            
            List<IModule> sorted = root.GetSorted();
            
            // inject model fields
            foreach (var module in sorted)
            {
                if(injectFieldInfos.TryGetValue(module.GetType(), out var dictionary))
                {
                    foreach (var keyValue in dictionary)
                    {
                        Type injectType = keyValue.Key;
                        
                        FieldInfo fieldInfo = keyValue.Value;
                        
                        if (moduleDict.TryGetValue(injectType, out var node))
                        {
                            fieldInfo.SetValue(module, node.module);
                        }
                        else if (_services.TryGetValue(injectType, out var service))
                        {
                            fieldInfo.SetValue(module, service);
                        }
                        else if (injectType == gameStateType)
                        {
                            fieldInfo.SetValue(module, stackState);
                        }
                    }
                }
            }
            
            return sorted;
        }
        
        internal static void UnRegisterModels(BasedStackGameState stackState)
        {
            Type gameStateType = stackState.GetType();
            
            if (stateModelTypes.TryGetValue(gameStateType, out List<Type> types))
            {
                foreach (var modelType in types)
                {
                    if (moduleDict.TryGetValue(modelType, out var node))
                    {
                        if (node.module is Controller controller)
                        {
                            controller.Destroy();
                        }
                        moduleDict.Remove(modelType);
                    }
                }
            }
        }
        
        sealed class ModuleNode : IComparable<ModuleNode>
        {
            public readonly IModule module;
        
            public ModuleNode parent;

            public readonly List<ModuleNode> children;
        
            public ModuleNode(IModule module)
            {
                this.module = module;
            
                children = new List<ModuleNode>();
            }
        
            public void SetParent(ModuleNode parent)
            {
                if (parent != this.parent)
                {
                    if (this.parent != null)
                    {
                        this.parent.children.Remove(this);
                    }
                
                    this.parent = parent;
                
                    if (parent != null)
                    {
                        parent.children.Add(this);
                    }
                }
            }
            
            public void CollectSorted(List<IModule> sorted)
            {
                children.Sort();
                
                foreach (var node in children)
                {
                    sorted.Add(node.module);
                    
                    node.CollectSorted(sorted);
                }
            }

            public int CompareTo(ModuleNode other)
            {
                Type aType = module.GetType();
                
                Type bType = other.module.GetType();
                
                bool aIsSceneController = aType == typeof(SceneController);

                if (aIsSceneController) return -1;
                
                bool bIsSceneController = bType == typeof(SceneController);
                
                if (bIsSceneController) return 1;
                
                bool aInjectB = IsRequire(aType, bType);
                
                bool bInjectA = IsRequire(bType, aType);
                
                if (aInjectB == bInjectA) return 0;
                
                return aInjectB ? 1 : -1;
            }
            
            public List<IModule> GetSorted()
            {
                List<IModule> sorted = new List<IModule>();

                if (null != module)
                {
                    sorted.Add(module);
                }
                
                CollectSorted(sorted);
                
                return sorted;
            }
        }
    }

    interface IModule
    {
        EstimatedSeconds EstimatedSeconds { get; }
        
        Result Start();
        
        Result Release();
    }

    interface IDataModule : IModule
    {
    }
    
    /// <summary>
    /// Inject controller attribute.
    /// 只可用于注册Entity类型
    /// </summary>
    [AttributeUsage( AttributeTargets.Field, Inherited = true)]
    public sealed class InjectFieldAttribute : System.Attribute
    {
    }
    
    /// <summary>
    /// define the group of the module
    /// </summary>
    [AttributeUsage( AttributeTargets.Class, Inherited = true)]
    // ReSharper disable once ClassNeverInstantiated.Global
    public sealed class GroupAttribute : System.Attribute
    {
        public readonly Type groupType;
        
        public GroupAttribute(Type groupType)
        {
            this.groupType = groupType;
        }
    }
    
    /// <summary>
    ///  a kind of Controller for data manager
    ///  Model will be created and started under the Scope of the game state declared explicitly
    ///  Model can declare other services and modules needed, which will be injected automatically
    /// </summary>
    public abstract class DataModule<TGameState> : Controller, IDataModule where TGameState : BasedStackGameState
    {
        [InjectField]
        private TGameState _gameState;
        
        // ReSharper disable once ConvertToAutoProperty
        // ReSharper disable once InconsistentNaming
        public TGameState gameState => _gameState;
        
        EstimatedSeconds IModule.EstimatedSeconds => new EstimatedSeconds(0, 0);
        
        Result IModule.Start()
        {
            OnStart();
            
            return new Result(){ IsDone = true };
        }

        Result IModule.Release()
        {
            OnRelease();
            
            return new Result(){ IsDone = true };
        }
        
        protected abstract void OnStart();
        
        protected abstract void OnRelease();
    }
    
    /// <summary>
    /// game module, also a kind of Controller, supports composite mode
    /// module is a business module of the GAME STATE, which will be created and started under the parent game state declared explicitly
    /// module can declare other services and modules needed, which will be injected automatically
    /// </summary>
    public abstract class ViewModule<TGameState> : Controller, IModule, IBeforeDestroy
        where TGameState : BasedStackGameState
    {
        [InjectField]
        private TGameState _gameState;
        
        private State _state = State.Created;
        
        enum State : byte { Created, Starting, Started, Realeasing }
        
        // ReSharper disable once MemberCanBeProtected.Global
        // ReSharper disable once UnusedMember.Global
        // ReSharper disable once ConvertToAutoProperty
        // ReSharper disable once InconsistentNaming
        public TGameState gameState => _gameState;
        
        void IBeforeDestroy.OnBeforeDestroy()
        {
            if (_state != State.Created)
            {
                _state = State.Created;
                
                OnDestroyed();
            }
        }
        
        EstimatedSeconds IModule.EstimatedSeconds => GetEstimatedSeconds();
        
        Result IModule.Start()
        {
            if (_state == State.Created)
            {
                _state = State.Starting;
                
                OnStart();
            }

            Result result = default;
            
            result.IsDone = false;

            if (_state == State.Starting)
            {
                result = IsStarted();
                
                if (result.IsDone)
                {
                    _state = State.Started;
                    
                    OnStarted();
                }
            }
            
            return result;
        }
        
        Result IModule.Release()
        {
            if (_state == State.Started || _state == State.Starting)
            {
                _state = State.Realeasing;
                
                OnRelease();
            }

            Result result = default;
            
            result.IsDone = false;
            
            if (_state == State.Realeasing)
            {
                result = IsReleased();
                
                if (result.IsDone)
                {
                    _state = State.Created;
                    
                    OnDestroyed();
                }
            }
            
            return result;
        }
        
        protected abstract EstimatedSeconds GetEstimatedSeconds();

        protected virtual void OnStart() { }

        protected abstract Result IsStarted();
        
        protected virtual void OnStarted(){ }
        
        protected virtual void OnRelease() { }
        
        protected abstract Result IsReleased();
        
        protected virtual void OnDestroyed() { }
    }

    sealed class ModuleStartJob : IGameStateJob
    {
        private readonly IModule _module;
        
        private readonly string _name;
        
        string IGameStateJob.name => _name;
        
        float IGameStateJob.estimatedSeconds => _module.EstimatedSeconds.StartSeconds;
        
        public ModuleStartJob(IModule module)
        {
            _module = module;
            
            _name = $"{_module.GetType().Name}_Start";
        }
        
        Result IGameStateJob.Run()
        {
            return _module.Start();
        }
    }
    
    sealed class ModuleReleaseJob : IGameStateJob
    {
        private readonly IModule _module;
        
        private readonly string _name;
        
        string IGameStateJob.name => _name;
        
        float IGameStateJob.estimatedSeconds => _module.EstimatedSeconds.ReleaseSeconds;

        public ModuleReleaseJob(IModule module)
        {
            _module = module;
            
            _name = $"{_module.GetType().Name}_Release";
        }

        Result IGameStateJob.Run()
        {
            return _module.Release();
        }
    }
}