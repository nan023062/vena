#if UNITY_EDITOR
#define DEBUG_WORLD
#endif
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.ExceptionServices;

namespace Vena
{
    /// <summary>
    /// world is a container for all actors and components.
    ///  - actor management
    ///  - component management
    ///  - life cycle management
    /// </summary>
    public sealed partial class World
    {
        public static void Log(string log)
        {
        }

        public static void LogWarning(string log)
        {
        }

        public static void LogError(string log)
        {
        }

        public readonly short Id;

        private string _name;

        private float _prevTime, _intervalTime;

        private int _frameRate;

        private bool _updated;
        
        public int frameCount
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get;
            private set;
        }

        public float deltaTime
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get;
            private set;
        }

        public int frameRate
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => _frameRate;
            set
            {
                if (_frameRate != value)
                {
                    _frameRate = value;

                    if (_frameRate > 0)
                    {
                        _intervalTime = 1f / _frameRate;
                    }
                }
            }
        }

        public string name
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => _name;
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            set
            {
                if (string.IsNullOrEmpty(name))
                {
                    return;
                }

                _name = value;
#if DEBUG_WORLD
                _debugTransform.name = this.ToString();
#endif
            }
        }

        private readonly Dictionary<uint, Actor> _actors;

        private readonly Dictionary<uint, Component> _components;

        private readonly Dictionary<Type, Filter> _filters;

        private List<Filter>[] _includeFiltersByTypeId;

        private List<Filter>[] _excludeFiltersByTypeId;

        public int actorCount
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => _actors.Count;
        }

#if DEBUG_WORLD
        private UnityEngine.Transform _debugTransform;
#endif

        World(short id)
        {
            Id = id;

            _name = GetType().Name;

            _actors = new Dictionary<uint, Actor>();

            _components = new Dictionary<uint, Component>();

            _filters = new Dictionary<Type, Filter>();

            _includeFiltersByTypeId = new List<Filter>[8];

            _excludeFiltersByTypeId = new List<Filter>[8];

            _frameRate = 30;

            _intervalTime = 1f / _frameRate;

            _prevTime = _time = UnityEngine.Time.time;

            frameCount = 0;

            _updated = false;

#if DEBUG_WORLD
            UnityEngine.GameObject go = new UnityEngine.GameObject(this.ToString());

            go.hideFlags = UnityEngine.HideFlags.NotEditable;

            UnityEngine.Object.DontDestroyOnLoad(go);

            _debugTransform = go.transform;
#endif
            _allSystems = new Dictionary<Type, ISystem>();
            _injections = new Dictionary<Type, object>() { { typeof(World), this } };
            _initialized = false;
        }

        public override string ToString()
        {
            return $"{name} ({GetType()}:{Id})";
        }

        #region World Manager

        public static readonly World Default;

        private static short _nextId = 1;

        private static float _time;

        private static readonly Dictionary<short, World> _worlds;

        public static float time
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => _time;
        }

        public static World Get(short id)
        {
            return _worlds.TryGetValue(id, out var world) ? world : null;
        }

        public static World Create(string name = null)
        {
            var world = new World(_nextId);

            _worlds.Add(_nextId++, world);

            world.name = name;

            return world;
        }

        static World()
        {
#if UNITY_EDITOR
            if (!UnityEngine.Application.isPlaying)
                return;
#endif
            _worlds = new Dictionary<short, World>();

            Default = Create("Default World");
        }

        public void Destroy()
        {
            if (LockDestroyOperation.PreCheckCanDestroy(Id))
            {
                _worlds.Remove(Id);

                InternalDestroy();
            }
        }

        private void InternalDestroy()
        {
            InternalSystemDestroy();

#if DEBUG_WORLD
            if (_debugTransform)
            {
                UnityEngine.Object.DestroyImmediate(_debugTransform.gameObject);
                _debugTransform = null;
            }
#endif

            ExceptionDispatchInfo exception = null;

            // PreDestroy
            foreach (var actor in _actors.Values)
            {
                try
                {
                    ((IActor)actor).InternalPreDestroy();
                }
                catch (Exception e)
                {
                    exception ??= ExceptionDispatchInfo.Capture(e);
                }
            }

            // Destroy
            foreach (var actor in _actors.Values)
            {
                try
                {
                    ((IActor)actor).InternalDeconstruct();
                }
                catch (Exception e)
                {
                    exception ??= ExceptionDispatchInfo.Capture(e);
                }

                lock (ObjectWithId.__lock__)
                {
                    using var _ = new ObjectWithId.Checker(ObjectWithId.CallFunc.Destroy);

                    actor.InternalMarkAsDestroyed();
                }
            }

            _actors.Clear();

            exception?.Throw();
        }

        #endregion

        #region filter

        public Filter GetFilter(Type filterType, bool createIfNotExists = true)
        {
            if (_filters.TryGetValue(filterType, out var filter))
            {
                return filter;
            }

            if (!createIfNotExists) return null;

            filter = (Filter)Activator.CreateInstance(filterType, BindingFlags.NonPublic | BindingFlags.Instance, null,
                null, CultureInfo.CurrentCulture);

            _filters[filterType] = filter;

            foreach (var info in filter.incArchetype.GetTypes())
            {
                List<Filter> list = GetOrAddFilters(ref _includeFiltersByTypeId, info.Id);

                list.Add(filter);
            }

            foreach (var info in filter.excArchetype.GetTypes())
            {
                List<Filter> list = GetOrAddFilters(ref _excludeFiltersByTypeId, info.Id);

                list.Add(filter);
            }

            return filter;
        }

        public Filter GetFilter<T>(bool createIfNotExists = true) where T : Filter
        {
            return GetFilter(typeof(T), createIfNotExists);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static List<Filter> GetOrAddFilters(ref List<Filter>[] array, int index)
        {
            int size = array.Length;
            if (size <= index)
            {
                do
                {
                    size *= 2;
                } while (size <= index);

                Array.Resize(ref array, size);
            }

            ref List<Filter> refValue = ref array[index];
            if (refValue == null)
            {
                refValue = new List<Filter>(8);
            }

            return refValue;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static bool TryGetFilters(List<Filter>[] array, int index, out List<Filter> list)
        {
            if (array.Length <= index)
            {
                list = null;
                return false;
            }

            return (list = array[index]) != null;
        }

        internal void UpdateFiltersAfterAddComponent(Actor actor, in TypeInfo info)
        {
            using var _ = new ProfilerWatch("UpdateFiltersAfterAddComponent");

            if (TryGetFilters(_includeFiltersByTypeId, info.Id, out var filterList))
            {
                ArchetypeId archetype = actor.archetype;

                for (int i = 0, iMax = filterList.Count; i < iMax; i++)
                {
                    Filter filter = filterList[i];

                    if (filter.IsCompatible(archetype))
                    {
                        filter.Add(actor);
                    }
                }
            }

            if (TryGetFilters(_excludeFiltersByTypeId, info.Id, out filterList))
            {
                for (int i = 0, iMax = filterList.Count; i < iMax; i++)
                {
                    Filter filter = filterList[i];

                    filter.Remove(actor);
                }
            }
        }

        internal void UpdateFiltersAfterRemoveComponent(Actor actor, in TypeInfo info)
        {
            using var _ = new ProfilerWatch("UpdateFiltersAfterRemoveComponent");

            if (TryGetFilters(_includeFiltersByTypeId, info.Id, out var filterList))
            {
                for (int i = 0, iMax = filterList.Count; i < iMax; i++)
                {
                    Filter filter = filterList[i];

                    filter.Remove(actor);
                }
            }

            if (TryGetFilters(_excludeFiltersByTypeId, info.Id, out filterList))
            {
                ArchetypeId archetype = actor.archetype;

                for (int i = 0, iMax = filterList.Count; i < iMax; i++)
                {
                    Filter filter = filterList[i];

                    if (filter.IsCompatible(archetype))
                    {
                        filter.Add(actor);
                    }
                }
            }
        }

        #endregion

        #region actor

        public T CreateActor<T>(params Type[] componentTypes) where T : Actor, new()
        {
            lock (ObjectWithId.__lock__)
            {
                T actor = default;

                using (var _ = new ObjectWithId.Checker(ObjectWithId.CallFunc.Create))
                {
                    actor = new T();

                    _actors[actor.Id] = actor;

                    ((IActor)actor).InternalConstruct(this);

                    RegisterLifeCycle(actor);

                    ((IActor)actor).InternalStart();
                }

                if (componentTypes !=null &&  componentTypes.Length > 0 )
                {
                    foreach (var componentType in componentTypes)
                    {
                        actor.AddComponent(componentType);
                    }
                }

                return actor;
            }
        }

        public Actor CreateActor(Type type, params Type[] componentTypes)
        {
            lock (ObjectWithId.__lock__)
            {
                using var _ = new ObjectWithId.Checker(ObjectWithId.CallFunc.Create);

                var actor = (Actor)Activator.CreateInstance(type);

                _actors[actor.Id] = actor;

                ((IActor)actor).InternalConstruct(this);

                RegisterLifeCycle(actor);

                ((IActor)actor).InternalStart();

                if (componentTypes !=null &&  componentTypes.Length > 0 )
                {
                    foreach (var componentType in componentTypes)
                    {
                        actor.AddComponent(componentType);
                    }
                }

                return actor;
            }
        }

        public Actor GetActor(uint id)
        {
            return _actors.TryGetValue(id, out var actor) ? actor : null;
        }

        internal void DestroyActor(uint id)
        {
            if (_actors.TryGetValue(id, out var actor))
            {
                _actors.Remove(id);

                ExceptionDispatchInfo exception = null;
                try
                {
                    ((IActor)actor).InternalPreDestroy();
                }
                catch (Exception e)
                {
                    exception = ExceptionDispatchInfo.Capture(e);
                }

                UnRegisterLifeCycle(actor);

                try
                {
                    ((IActor)actor).InternalDeconstruct();
                }
                catch (Exception e)
                {
                    exception ??= ExceptionDispatchInfo.Capture(e);
                }

                lock (ObjectWithId.__lock__)
                {
                    using var _ = new ObjectWithId.Checker(ObjectWithId.CallFunc.Destroy);

                    actor.InternalMarkAsDestroyed();
                }

                exception?.Throw();

                return;
            }

            LogWarning($"Actor {id} not found.");
        }

        internal UnityEngine.Transform NewDebugTransform()
        {
#if DEBUG_WORLD
            var go = new UnityEngine.GameObject();

            go.hideFlags = UnityEngine.HideFlags.NotEditable;

            var debugTransform = go.transform;

            debugTransform.SetParent(_debugTransform);

            return debugTransform;
#else
        return null;
#endif
        }

        public void GetAllActors(ICollection<Actor> actors)
        {
            foreach (var actor in actors)
            {
                actors.Add(actor);
            }
        }

        public void GetAllActorIds(ICollection<uint> actors)
        {
            foreach (var id in _actors.Keys)
            {
                actors.Add(id);
            }
        }

        #endregion

        #region components

        internal Component SpawnComponent(Actor actor, in TypeInfo info)
        {
            Component component = null;

            lock (ObjectWithId.__lock__)
            {
                using var _ = new ObjectWithId.Checker(ObjectWithId.CallFunc.Create);

                component = (Component)Activator.CreateInstance(info.Type);
            }

            if (null == component)
            {
                throw new Exception($"Can't create component ' {info.Type} ' .");
            }

            _components.Add(component.Id, component);

            RegisterLifeCycle(component);

            return component;
        }

        internal void UnSpawnComponent(Actor actor, Component component)
        {
            if (!_components.Remove(component.Id))
            {
                throw new Exception($"Component {component} not found.");
            }

            UnRegisterLifeCycle(component);

            lock (ObjectWithId.__lock__)
            {
                using var _ = new ObjectWithId.Checker(ObjectWithId.CallFunc.Destroy);

                component.InternalMarkAsDestroyed();
            }
        }

        #endregion

        struct LockDestroyOperation : IDisposable
        {
            private static readonly List<short> _destroyList = new List<short>();

            private static bool _lockDestroy;

            public static bool PreCheckCanDestroy(short id)
            {
                if (_lockDestroy)
                {
                    _destroyList.Add(id);

                    return false;
                }

                return true;
            }

            public LockDestroyOperation(int arg)
            {
                _lockDestroy = true;
            }

            public void Dispose()
            {
                // TODO release managed resources here
                _lockDestroy = false;

                if (_destroyList.Count > 0)
                {
                    foreach (var id in _destroyList)
                    {
                        if (_worlds.TryGetValue(id, out var world))
                        {
                            _worlds.Remove(id);

                            world.InternalDestroy();
                        }
                    }

                    _destroyList.Clear();
                }
            }
        }

        public static void Update(float deltaTime)
        {
            using var _ = new LockDestroyOperation(1);

            _time = UnityEngine.Time.time;

            foreach (var world in _worlds.Values)
            {
                world.InternalUpdate();//
            }
        }

        private void InternalUpdate()
        {
            float dt = _time - _prevTime;

            if (frameRate > 0)
            {
                if (dt < _intervalTime)
                {
                    return;
                }

                _prevTime = _time;
            }

            deltaTime = dt;

            frameCount++;

            _updated = true;

            InvokeLifeCycle<IUpdate>(deltaTime);

            InternalSystemUpdate();
        }

        private void InternalLateUpdate()
        {
            if (_updated)
            {
                _updated = false;

                InvokeLifeCycle<ILateUpdate>(deltaTime);

                InternalSystemLateUpdate();
            }
        }

        public static void LateUpdate(float deltaTime)
        {
            using var _ = new LockDestroyOperation(0);

            foreach (var world in _worlds.Values)
            {
                world.InternalLateUpdate();
            }
        }

        public static void ApplicationPause(bool pauseStatus)
        {
#if UNITY_EDITOR
            if(!UnityEngine.Application.isPlaying) 
                return;
#endif
            
            using var _ = new LockDestroyOperation(0);

            foreach (var world in _worlds.Values)
            {
                world.InvokeLifeCycle<IApplicationPause>(pauseStatus);
            }
        }

        public static void ApplicationFocus(bool hasFocus)
        {
#if UNITY_EDITOR
            if(!UnityEngine.Application.isPlaying) 
                return;
#endif
            using var _ = new LockDestroyOperation(0);

            foreach (var world in _worlds.Values)
            {
                world.InvokeLifeCycle<IApplicationFocus>(hasFocus);
            }
        }

        public static void OnDrawGizmos()
        {
#if UNITY_EDITOR
            if(!UnityEngine.Application.isPlaying) 
                return;
#endif
#if DEBUG_WORLD
            using var _ = new LockDestroyOperation(0);

            foreach (var world in _worlds.Values)
            {
                world.InvokeLifeCycle<IGizmos>();
            }
#endif
        }

        public static void OnGUI()
        {
#if UNITY_EDITOR
            if(!UnityEngine.Application.isPlaying) 
                return;
#endif
#if DEBUG_WORLD
            using var _ = new LockDestroyOperation(0);

            foreach (var world in _worlds.Values)
            {
                world.InvokeLifeCycle<IGUI>();
            }
#endif
        }
        
        private static void OnApplicationQuit()
        {
            foreach (var world in _worlds.Values.ToArray())
            {
                _worlds.Remove( world.Id );
                
                world.InternalDestroy();
            }
        }


        #region Life Cycle Invoker

        readonly LifeCycle[] _lifeCycles = new LifeCycle[LifeCycle.Count]
        {
            new Tick(),
            new LateTick(),
            new OnApplicationPause(),
            new OnApplicationFocus(),
#if UNITY_EDITOR
            new DrawGizmos(),
            new GUI(),
#endif
        };

        private void RegisterLifeCycle(ObjectWithId obj)
        {
            for (int i = 0; i < LifeCycle.Count; i++)
            {
                _lifeCycles[i].TryAdd(obj);
            }
        }

        private void UnRegisterLifeCycle(ObjectWithId obj)
        {
            for (int i = 0; i < LifeCycle.Count; i++)
            {
                _lifeCycles[i].TryRemove(obj);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void InvokeLifeCycle<T>()
        {
            ((LifeCycle_None<T>)_lifeCycles[LifeCycle_None<T>.Index]).Invoke();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void InvokeLifeCycle<T>(float f)
        {
            ((LifeCycle_Float<T>)_lifeCycles[LifeCycle_Float<T>.Index]).Invoke(f);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void InvokeLifeCycle<T>(bool b)
        {
            ((LifeCycle_Bool<T>)_lifeCycles[LifeCycle_Bool<T>.Index]).Invoke(b);
        }

        internal abstract class LifeCycle
        {
#if UNITY_EDITOR
            public const int Count = 6;
#else
        public const int Count = 4;
#endif
            private static byte _index = 0;
            private static Info[] _array = new Info[Count];
            private static readonly Dictionary<Type, int> _typeToIdx = new Dictionary<Type, int>();

            protected readonly struct Info
            {
                public readonly byte id;
                public readonly bool reverse;

                public Info(byte id, bool reverse)
                {
                    this.id = id;
                    this.reverse = reverse;
                }
            }

            protected static Info Init<T>()
            {
                if (_index >= Count)
                {
                    throw new Exception($"LifeCycle count more than {Count}. Please check.");
                }

                if (_index >= _array.Length)
                {
                    Array.Resize(ref _array, _array.Length * 2);
                }

                byte index = _index++;
                Type type = typeof(T);
                Info info = new Info(index, false);
                _array[index] = info;
                _typeToIdx[type] = index;
                return info;
            }

            public abstract int Id { get; }

            public abstract bool IsEmpty { get; }

            public abstract bool TryAdd(ObjectWithId obj);

            public abstract bool TryRemove(ObjectWithId obj);
        }

        internal abstract class LifeCycle<T> : LifeCycle
        {
            public static readonly byte Index;

            static LifeCycle()
            {
                Info info = Init<T>();

                Index = info.id;
            }

            protected readonly Dictionary<uint, T> dictionary = new Dictionary<uint, T>();

            protected readonly LinkedList<uint> linkedList = new LinkedList<uint>();

            public sealed override int Id => Index;

            public sealed override bool IsEmpty => dictionary.Count == 0;

            public sealed override bool TryAdd(ObjectWithId obj)
            {
                if (obj is T lifeCycle)
                {
                    if (!dictionary.ContainsKey(obj.Id))
                    {
                        dictionary.Add(obj.Id, lifeCycle);

                        linkedList.AddLast(obj.Id);

                        return true;
                    }
                }

                return false;
            }

            public sealed override bool TryRemove(ObjectWithId obj)
            {
                if (obj is T)
                {
                    return dictionary.Remove(obj.Id);
                }

                return false;
            }
        }

        #region LifeCycle_None

        abstract class LifeCycle_None<T> : LifeCycle<T>
        {
            public void Invoke()
            {
                if (linkedList.Count <= 0)
                {
                    return;
                }

                Exception exception = null;

                LinkedListNode<uint> node = linkedList.First;

                while (node != null)
                {
                    LinkedListNode<uint> next = node.Next;

                    if (dictionary.TryGetValue(node.Value, out T t))
                    {
                        try
                        {
                            Invoke(t);
                        }
                        catch (Exception e)
                        {
                            exception ??= e;
                        }
                    }
                    else
                    {
                        linkedList.Remove(node);
                    }

                    node = next;
                }

                if (exception != null)
                {
                    throw exception;
                }
            }

            protected abstract void Invoke(T lifeCycle);
        }

        sealed class DrawGizmos : LifeCycle_None<IGizmos>
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            protected override void Invoke(IGizmos lifeCycle)
            {
                lifeCycle.OnDrawGizmos();
            }
        }

        sealed class GUI : LifeCycle_None<IGUI>
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            protected override void Invoke(IGUI lifeCycle)
            {
                lifeCycle.OnGUI();
            }
        }

        #endregion

        #region LifeCycle_Float

        abstract class LifeCycle_Float<T> : LifeCycle<T>
        {
            public void Invoke(float f)
            {
                if (linkedList.Count <= 0)
                {
                    return;
                }

                ExceptionDispatchInfo capturedException = null;

                LinkedListNode<uint> node = linkedList.First;

                while (node != null)
                {
                    LinkedListNode<uint> next = node.Next;

                    if (dictionary.TryGetValue(node.Value, out T t))
                    {
                        try
                        {
                            Invoke(t, f);
                        }
                        catch (Exception e)
                        {
                            capturedException ??= ExceptionDispatchInfo.Capture(e);
                        }
                    }
                    else
                    {
                        linkedList.Remove(node);
                    }

                    node = next;
                }

                if (capturedException != null)
                {
                    capturedException.Throw();
                }
            }

            protected abstract void Invoke(T lifeCycle, float f);
        }

        sealed class Tick : LifeCycle_Float<IUpdate>
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            protected override void Invoke(IUpdate lifeCycle, float f)
            {
                lifeCycle.Update(f);
            }
        }

        sealed class LateTick : LifeCycle_Float<ILateUpdate>
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            protected override void Invoke(ILateUpdate lifeCycle, float f)
            {
                lifeCycle.LateUpdate(f);
            }
        }

        #endregion

        #region LifeCycle_Bool

        abstract class LifeCycle_Bool<T> : LifeCycle<T>
        {
            public void Invoke(bool b)
            {
                if (linkedList.Count <= 0)
                {
                    return;
                }

                Exception exception = null;

                LinkedListNode<uint> node = linkedList.First;

                while (node != null)
                {
                    LinkedListNode<uint> next = node.Next;

                    if (dictionary.TryGetValue(node.Value, out T t))
                    {
                        try
                        {
                            Invoke(t, b);
                        }
                        catch (Exception e)
                        {
                            exception ??= e;
                        }
                    }
                    else
                    {
                        linkedList.Remove(node);
                    }

                    node = next;
                }

                if (exception != null)
                {
                    throw exception;
                }
            }

            protected abstract void Invoke(T lifeCycle, bool b);
        }

        sealed class OnApplicationPause : LifeCycle_Bool<IApplicationPause>
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            protected override void Invoke(IApplicationPause lifeCycle, bool b)
            {
                lifeCycle.OnApplicationPause(b);
            }
        }

        sealed class OnApplicationFocus : LifeCycle_Bool<IApplicationFocus>
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            protected override void Invoke(IApplicationFocus lifeCycle, bool b)
            {
                lifeCycle.OnApplicationFocus(b);
            }
        }

        #endregion

        #endregion
    }
}