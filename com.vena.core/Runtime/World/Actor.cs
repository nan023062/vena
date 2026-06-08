#if UNITY_EDITOR
#define DEBUG_WORLD
#endif
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.ExceptionServices;

namespace Vena
{
    interface IActor
    {
        void InternalConstruct(World world);

        void InternalStart();

        void InternalRemoveComponent(Component component);

        void InternalPreDestroy();

        void InternalDeconstruct();
    }

    /// <summary>
    /// Actor is a base class for objects that can be added to the world.
    /// </summary>
    public class Actor : ObjectWithId, IActor
    {
        private ComponentBlock _componentBlock;

#if DEBUG_WORLD
        UnityEngine.Transform _debugTransform;
#endif

        public void SetDebugName()
        {
#if DEBUG_WORLD
            _debugTransform.name = ToString();
#endif
        }

        public InstanceID InstanceID
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get;
            private set;
        }

        public World world
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get;
            private set;
        }

        public ArchetypeId archetype
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => _componentBlock.archetype;
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

        void IActor.InternalConstruct(World actorWorld)
        {
            world = actorWorld;

            InstanceID = new InstanceID(world.Id, Id);

#if DEBUG_WORLD
            _debugTransform = world.NewDebugTransform();
            _debugTransform.name = $"{this}";
#endif
            _componentBlock = new ComponentBlock(this);

            (this as ICreate)?.OnCreate();
        }

        void IActor.InternalStart()
        {
            Exception ex = null;
            try
            {
                (this as IStart)?.OnStart();
            }
            catch (Exception e)
            {
                ex = e;
            }

            _componentBlock.Start();

            if (null != ex) throw ex;
        }

        void IActor.InternalPreDestroy()
        {
            // Call Components OnDestroy
            _componentBlock.PreDestroy();

            // Call OnDestroy 
            (this as IBeforeDestroy)?.OnBeforeDestroy();
        }

        void IActor.InternalDeconstruct()
        {
#if DEBUG_WORLD
            if (_debugTransform)
            {
                UnityEngine.Object.Destroy(_debugTransform.gameObject);
                _debugTransform = null;
            }
#endif
            // Call Components OnDestroy
            _componentBlock.Dispose();

            // Call OnDestroy 
            (this as IDestroy)?.OnDestroy();
        }

        public override string ToString()
        {
            return $"{GetType()}:{InstanceID}";
        }

        public T AddComponent<T>() where T : Component
        {
            return AddComponent(typeof(T)) as T;
        }

        public Component AddComponent(Type componentType)
        {
            return AddComponentRecursively(componentType);
        }

        Component AddComponentRecursively(Type componentType)
        {
            Component component = _componentBlock.Get(componentType, true);

            if (component)
            {
                return component;
            }

            // first to register component type to archetype
            ref readonly var typeInfo = ref Archetype.GetTypeInfo(componentType);

            foreach (var type in typeInfo.Dependencies)
            {
                AddComponentRecursively(type);
            }

            component = world.SpawnComponent(this, typeInfo);

            if (_componentBlock.OnAdd(component))
            {
                try
                {
                    (component as IStart)?.OnStart();
                }
                finally
                {
                    world.UpdateFiltersAfterAddComponent(this, typeInfo);
                }

                return component;
            }


            throw new Exception("Component is already added to this actor.");
        }

        void IActor.InternalRemoveComponent(Component component)
        {
            if (component.actor != this)
            {
                throw new Exception("Component is not added to this actor.");
            }

            _componentBlock.OnRemove(component);

            world.UnSpawnComponent(this, component);
        }

        public T GetComponent<T>(bool IgnoreDerivedClasses = true) where T : Component
        {
            return _componentBlock.Get<T>(IgnoreDerivedClasses);
        }

        public Component GetComponent(Type type, bool IgnoreDerivedClasses = true)
        {
            return _componentBlock.Get(type, IgnoreDerivedClasses);
        }

        public bool HasComponent<T>() where T : Component
        {
            return _componentBlock.Has<T>();
        }

        public void GetComponents<T>(List<T> outComponents) where T : Component
        {
            _componentBlock.GetComponents(outComponents);
        }
        
        public IReadOnlyCollection<Component> GetComponents()
        {
            return _componentBlock.components.Values;
        }

        public bool TryGetComponents<T1, T2>(out T1 t1, out T2 t2)
            where T1 : Component where T2 : Component
        {
            if (!_componentBlock.archetype.HasAll<T1, T2>())
            {
                t1 = default;
                t2 = default;
                return false;
            }

            t1 = default;
            t2 = default;
            return true;
        }

        public bool TryGetComponents<T1, T2, T3>(out T1 t1, out T2 t2, out T3 t3)
            where T1 : Component where T2 : Component where T3 : Component
        {
            if (!_componentBlock.archetype.HasAll<T1, T2, T3>())
            {
                t1 = default;
                t2 = default;
                t3 = default;
                return false;
            }

            t1 = default;
            t2 = default;
            t3 = default;
            return true;
        }

        public bool TryGetComponents<T1, T2, T3, T4>(out T1 t1, out T2 t2, out T3 t3, out T4 t4)
            where T1 : Component where T2 : Component where T3 : Component where T4 : Component
        {
            if (!_componentBlock.archetype.HasAll<T1, T2, T3, T4>())
            {
                t1 = default;
                t2 = default;
                t3 = default;
                t4 = default;
                return false;
            }


            t1 = default;
            t2 = default;
            t3 = default;
            t4 = default;
            return true;
        }

        public bool TryGetComponents<T1, T2, T3, T4, T5>(out T1 t1, out T2 t2, out T3 t3, out T4 t4, out T5 t5)
            where T1 : Component where T2 : Component where T3 : Component where T4 : Component where T5 : Component
        {
            if (!_componentBlock.archetype.HasAll<T1, T2, T3, T4, T5>())
            {
                t1 = default;
                t2 = default;
                t3 = default;
                t4 = default;
                t5 = default;
                return false;
            }


            t1 = default;
            t2 = default;
            t3 = default;
            t4 = default;
            t5 = default;
            return true;
        }

        public sealed override void CopyTo(ObjectWithId target)
        {
            if (target is Actor actor)
            {
                ArchetypeId targetArchetype = actor.archetype;
                if (archetype != targetArchetype)
                {
                    throw new Exception("Can't copy Actor to different archetype.");
                }

                this.CopyTo(actor);

                foreach (var keyValue in _componentBlock.components)
                {
                    Component component = keyValue.Value;

                    Component targetComponent = actor.GetComponent(component.GetType());

                    component.CopyTo(targetComponent);
                }

                return;
            }

            throw new Exception("Can't copy Actor to non-Actor object.");
        }

        /// <summary>
        /// Copy Actor data to target Actor.
        /// </summary>
        /// <param name="target"></param>
        protected virtual void CopyTo(Actor target)
        {
            // copy actor data
        }

        public Actor Instantiate()
        {
            Actor clone = world.CreateActor(GetType());

            foreach (var keyValue in _componentBlock.components)
            {
                clone.AddComponent(keyValue.Value.GetType());
            }

            CopyTo(clone);

            return clone;
        }
    }

    /// <summary>
    /// ComponentBlock is a container for components.
    /// </summary>
    sealed class ComponentBlock : IDisposable
    {
        public readonly Actor actor;

        public readonly Dictionary<Type, Component> components;

        private ArchetypeId _archetype;

        private Lock _lock;

        private bool _started;

        enum Lock : byte
        {
            Free,
            Add,
            Remove,
            Dispose
        }


#if DEBUG_WORLD
        UnityEngine.Transform _debugtTransform;
#endif

        public ArchetypeId archetype
        {
            get
            {
                if (_archetype.bucket < 0)
                {
                    _archetype = Archetype.RegisterObjects(components.Values);
#if DEBUG_WORLD
                    _debugtTransform.name = $"{_archetype}";
#endif
                }

                return _archetype;
            }
        }

        public ComponentBlock(Actor actor)
        {
            this.actor = actor;

            _archetype = ArchetypeId.Invalid;

            components = new Dictionary<Type, Component>();

            _lock = Lock.Free;

            _started = false;
        }

        public void Start()
        {
            if (_started)
            {
                throw new Exception("ComponentBlock already started.");
            }

            _started = true;

#if DEBUG_WORLD
            _debugtTransform = actor.NewDebugTransform();
            _debugtTransform.name = _archetype.ToString();
#endif

            ExceptionDispatchInfo exception = null;

            foreach (var component in components.Values)
            {
                try
                {
                    (component as IStart)?.OnStart();
                }
                catch (Exception e)
                {
                    exception ??= ExceptionDispatchInfo.Capture(e);
                }
            }

            exception?.Throw();
        }

        internal bool OnAdd(Component component)
        {
            if (_lock != Lock.Free)
            {
                throw new Exception($"Can't add component during {_lock}.");
            }

            if (!components.ContainsKey(component.GetType()))
            {
                _lock = Lock.Add;

                components.Add(component.GetType(), component);

                _archetype = ArchetypeId.Invalid;

                ((IActorComponent)component).InternalConstruct(actor);

                _lock = Lock.Free;

#if DEBUG_WORLD
                ArchetypeId _ = archetype;
#endif

                return true;
            }

            return false;
        }

        internal void OnRemove(Component component)
        {
            if (_lock != Lock.Free)
            {
                throw new Exception($"Can't remove component during {_lock}.");
            }

            if (components.Remove(component.GetType()))
            {
                _lock = Lock.Remove;

                _archetype = ArchetypeId.Invalid;

#if DEBUG_WORLD
                ArchetypeId _ = archetype;
#endif
                ExceptionDispatchInfo exception = null;
                try
                {
                    (component as IBeforeDestroy)?.OnBeforeDestroy();
                }
                catch (Exception e)
                {
                    exception = ExceptionDispatchInfo.Capture(e);
                }

                try
                {
                    ref readonly var typeInfo = ref Archetype.GetTypeInfo(component.GetType());

                    actor.world.UpdateFiltersAfterRemoveComponent(actor, typeInfo);

                    ((IActorComponent)component).InternalDeconstruct();

                    exception?.Throw();
                }
                finally
                {
                    _lock = Lock.Free;
                }
            }
        }

        public void PreDestroy()
        {
            if (!_started)
            {
                throw new Exception("ComponentBlock not started.");
            }

            if (_lock != Lock.Free)
            {
                throw new Exception($"Can't PreDestroy component during {_lock}.");
            }

            _lock = Lock.Remove;

            ExceptionDispatchInfo exception = null
                ;
            foreach (var component in components.Values)
            {
                try
                {
                    (component as IBeforeDestroy)?.OnBeforeDestroy();
                }
                catch (Exception e)
                {
                    exception ??= ExceptionDispatchInfo.Capture(e);
                }
            }

            _lock = Lock.Free;

            exception?.Throw();
        }

        public void Dispose()
        {
            if (!_started)
            {
                throw new Exception("ComponentBlock not started.");
            }

            _started = false;

            if (_lock != Lock.Free)
            {
                throw new Exception($"Can't dispose component during {_lock}.");
            }

            _lock = Lock.Dispose;

            _archetype = ArchetypeId.Invalid;

            Component[] array = components.Values.ToArray();

            components.Clear();

            ExceptionDispatchInfo exception = null;

            foreach (var component in array)
            {
                try
                {
                    ref readonly var typeInfo = ref Archetype.GetTypeInfo(component.GetType());

                    actor.world.UpdateFiltersAfterRemoveComponent(actor, typeInfo);

                    ((IActorComponent)component).InternalDeconstruct();
                }
                catch (Exception e)
                {
                    exception ??= ExceptionDispatchInfo.Capture(e);
                }
                finally
                {
                    actor.world.UnSpawnComponent(actor, component);
                }
            }

            _archetype = ArchetypeId.Invalid;

#if DEBUG_WORLD
            if (_debugtTransform)
            {
                UnityEngine.Object.Destroy(_debugtTransform.gameObject);
                _debugtTransform = null;
            }
#endif
            _lock = Lock.Free;

            exception?.Throw();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal T Get<T>(bool IgnoreDerivedClasses) where T : Component
        {
            if (components.TryGetValue(typeof(T), out var component))
            {
                return component as T;
            }

            // if IgnoreDerivedClasses is false, 查找父类
            if (!IgnoreDerivedClasses)
            {
                foreach (var keyValue in components)
                {
                    if (typeof(T).IsAssignableFrom(keyValue.Key))
                    {
                        return keyValue.Value as T;
                    }
                }
            }

            return default;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal bool Has<T>() where T : Component
        {
            return components.ContainsKey(typeof(T));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal Component Get(Type type, bool IgnoreDerivedClasses)
        {
            if (components.TryGetValue(type, out var component))
            {
                return component;
            }

            // if IgnoreDerivedClasses is false, 查找父类
            if (!IgnoreDerivedClasses)
            {
                foreach (var keyValue in components)
                {
                    if (type.IsAssignableFrom(keyValue.Key))
                    {
                        return keyValue.Value;
                    }
                }
            }

            return default;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal void GetComponents<T>(List<T> outComponents) where T : Component
        {
            outComponents.Clear();

            foreach (var component in components.Values)
            {
                if (component is T t)
                {
                    outComponents.Add(t);
                }
            }
        }
    }
}