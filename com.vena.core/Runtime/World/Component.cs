#if UNITY_EDITOR
#define DEBUG_WORLD
#endif
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace Vena
{
    internal interface IActorComponent
    {
        void InternalConstruct(Actor actor);

        void InternalDeconstruct();
    }

    /// <summary>
    /// ActorComponent is a base class for components that can be added to an actor.
    /// </summary>
    public abstract class Component : ObjectWithId, IActorComponent
    {
        public Actor actor
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get;
            private set;
        }

        void IActorComponent.InternalConstruct(Actor owner)
        {
            actor = owner;

            (this as ICreate)?.OnCreate();
        }

        void IActorComponent.InternalDeconstruct()
        {
            try
            {
                (this as IDestroy)?.OnDestroy();
            }
            finally
            {
                actor = null;
            }
        }

        public sealed override void CopyTo(ObjectWithId target)
        {
            if (target is Component targetComponent)
            {
                if (GetType() != targetComponent.GetType())
                {
                    throw new InvalidOperationException(
                        $"Cannot copy component to target of type {target.GetType().Name}.");
                }

                this.CopyTo(targetComponent);
                return;
            }

            throw new InvalidOperationException($"Cannot copy component to target of type {target.GetType().Name}.");
        }

        /// <summary>
        /// Copy the component data to the target component.
        /// </summary>
        /// <param name="target"></param>
        protected virtual void CopyTo(Component target)
        {
        }

        public override string ToString()
        {
            return $"{GetType().Name}:{Id}";
        }
    }

    /// <summary>
    /// Filter is a base class for filters that can be used to filter actors based on their components.
    /// </summary>
    public abstract class Filter : IDisposable
    {
        private DelayedOp[] _delayedOps = new DelayedOp[16];
        private int _delayedOpsCount = 0;
        private bool _hasAddOp = false;
        private int _lockCount = 0;
        internal ArchetypeId incArchetype = ArchetypeId.Invalid;
        internal ArchetypeId excArchetype = ArchetypeId.Invalid;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        [Conditional("UNITY_EDITOR1")]
        internal static void LogWarning(string message)
        {
            World.LogWarning($"[Filter]:{message}");
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal bool IsCompatible(in ArchetypeId actorArchetype)
        {
            return actorArchetype.HasAll(incArchetype) && !actorArchetype.HasAny(excArchetype);
        }

        internal void Add(Actor actor)
        {
            if (_lockCount > 0)
            {
                AddDelayedOp(true, actor);
                return;
            }

            OnAdd(actor);
        }

        internal void Remove(Actor actor)
        {
            if (_lockCount > 0)
            {
                AddDelayedOp(false, actor);
                return;
            }

            OnRemove(actor);
        }

        protected abstract void OnAdd(Actor actor);

        protected abstract void OnRemove(Actor actor);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void AddDelayedOp(bool isAdd, in Actor actor)
        {
            if (_delayedOps.Length == _delayedOpsCount)
                Array.Resize(ref _delayedOps, _delayedOpsCount << 1);

            ref var op = ref _delayedOps[_delayedOpsCount++];
            op.IsAdd = isAdd;
            op.Actor = actor;
            if (isAdd)
            {
                _hasAddOp = true;
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected void Lock()
        {
            if (_delayedOpsCount > 0)
                LogWarning($"{GetType().Name}.Enumerator时_delayedOpsCount>0，数据未更到最新");

            _lockCount++;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected void Unlock()
        {
#if DEBUG_WORLD
            if (_lockCount <= 0)
                throw new Exception($"Invalid lock-unlock balance for \"{GetType().Name}\".");
#endif
            if (_hasAddOp)
                LogWarning($"{GetType().Name}.Enumerator.Dispose时_hasAddOp，存在数据遗漏");


            _lockCount--;
            if (_lockCount == 0 && _delayedOpsCount > 0)
            {
                // process delayed operations.
                for (int i = 0, iMax = _delayedOpsCount; i < iMax; i++)
                {
                    ref var op = ref _delayedOps[i];
                    if (op.IsAdd)
                        OnAdd(op.Actor);
                    else
                        OnRemove(op.Actor);
                }

                _hasAddOp = false;
                _delayedOpsCount = 0;
            }
        }

        struct DelayedOp
        {
            public bool IsAdd;

            public Actor Actor;
        }

        public void Dispose()
        {
            // TODO release managed resources here
            OnDispose();
        }

        protected abstract void OnDispose();
    }


    class Container<T> : IDisposable
    {
        private int count = 0;

        public (Actor, T)[] array = new (Actor, T)[16];

        public readonly Dictionary<Actor, int> actors = new Dictionary<Actor, int>();

        public int Count
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => count;
        }

        public void Add(Actor actor, T t)
        {
            if (count == array.Length)
            {
                Array.Resize(ref array, count << 1);
            }

            array[count] = (actor, t);

            actors.Add(actor, count);

            count++;
        }

        public bool Remove(Actor actor)
        {
            if (actors.TryGetValue(actor, out int index))
            {
                ref (Actor actor, T c) t0 = ref array[index];

                actors.Remove(actor);

                count--;

                if (count == index)
                {
                    t0 = default;
                }
                else
                {
                    (Actor actor, T c) t = array[count];
                    array[count] = default;
                    t0 = t;
                    actors[t0.actor] = index;
                }

                return true;
            }

            return false;
        }

        public void Dispose()
        {
            // TODO release managed resources here
            actors.Clear();
            array = null;
            count = 0;
        }
    }

    public class Filter<Inc1> : Filter
        where Inc1 : Component
    {
        private readonly Container<Inc1> _container;

        protected Filter()
        {
            _container = new Container<Inc1>();
            incArchetype = Archetype<Inc1>.Id;
        }

        protected override void OnAdd(Actor actor)
        {
            Inc1 t = actor.GetComponent<Inc1>();
            if (t != null)
            {
                _container.Add(actor, t);
                LogWarning($"{GetType().GetTypeName()}.OnAdd({actor}_{actor.archetype})");
            }
        }

        protected override void OnRemove(Actor actor)
        {
            if (_container.Remove(actor))
            {
                LogWarning($"{GetType().GetTypeName()}.OnRemove({actor}_{actor.archetype})");
            }
        }

        protected override void OnDispose()
        {
            _container.Dispose();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Enumerator GetEnumerator()
        {
            return new Enumerator(this);
        }

        public struct Enumerator : IDisposable
        {
            private readonly Filter<Inc1> _filter;
            private readonly (Actor, Inc1)[] _array;
            private readonly int _count;
            private int _current;

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            internal Enumerator(Filter<Inc1> filter)
            {
                _filter = filter;
                _array = _filter._container.array;
                _count = _filter._container.Count;
                _current = -1;
                _filter.Lock();
            }

            public Inc1 Current
            {
                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                get
                {
                    ref readonly (Actor actor, Inc1 t) item = ref _array[_current];
                    return item.t;
                }
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public void Dispose()
            {
                _filter.Unlock();
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public bool MoveNext()
            {
                return ++_current < _count;
            }
        }

        public class Exclude<Exc1> : Filter<Inc1>
            where Exc1 : Component
        {
            protected Exclude()
            {
                excArchetype = Archetype<Exc1>.Id;
            }
        }

        public class Exclude<Exc1, Exc2> : Filter<Inc1>
            where Exc1 : Component
            where Exc2 : Component
        {
            protected Exclude()
            {
                excArchetype = Archetype<Exc1, Exc2>.Id;
            }
        }

        public class Exclude<Exc1, Exc2, Exc3> : Filter<Inc1>
            where Exc1 : Component
            where Exc2 : Component
            where Exc3 : Component
        {
            protected Exclude()
            {
                excArchetype = Archetype<Exc1, Exc2, Exc3>.Id;
            }
        }

        public class Exclude<Exc1, Exc2, Exc3, Exc4> : Filter<Inc1>
            where Exc1 : Component
            where Exc2 : Component
            where Exc3 : Component
            where Exc4 : Component
        {
            protected Exclude()
            {
                excArchetype = Archetype<Exc1, Exc2, Exc3, Exc4>.Id;
            }
        }

        public class Exclude<Exc1, Exc2, Exc3, Exc4, Exc5> : Filter<Inc1>
            where Exc1 : Component
            where Exc2 : Component
            where Exc3 : Component
            where Exc4 : Component
            where Exc5 : Component
        {
            protected Exclude()
            {
                excArchetype = Archetype<Exc1, Exc2, Exc3, Exc4, Exc5>.Id;
            }
        }
    }

    public class Filter<Inc1, Inc2> : Filter
        where Inc1 : Component
        where Inc2 : Component
    {
        private readonly Container<(Inc1, Inc2)> _container;

        protected Filter()
        {
            _container = new Container<(Inc1, Inc2)>();

            incArchetype = Archetype<Inc1, Inc2>.Id;
        }

        protected override void OnAdd(Actor actor)
        {
            if (actor.TryGetComponents(out Inc1 t1, out Inc2 t2))
            {
                _container.Add(actor, (t1, t2));
                LogWarning($"{GetType().GetTypeName()}.OnAdd({actor}_{actor.archetype})");
            }
        }

        protected override void OnRemove(Actor actor)
        {
            if (_container.Remove(actor))
            {
                LogWarning($"{GetType().GetTypeName()}.OnRemove({actor}_{actor.archetype})");
            }
        }

        protected override void OnDispose()
        {
            _container.Dispose();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Enumerator GetEnumerator()
        {
            return new Enumerator(this);
        }

        public struct Enumerator : IDisposable
        {
            private readonly Filter<Inc1, Inc2> _filter;
            private readonly (Actor, (Inc1, Inc2))[] _array;
            private readonly int _count;
            private int _current;

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            internal Enumerator(Filter<Inc1, Inc2> filter)
            {
                _filter = filter;
                _array = _filter._container.array;
                _count = _filter._container.Count;
                _current = -1;
                _filter.Lock();
            }

            public (Inc1, Inc2) Current
            {
                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                get
                {
                    ref readonly (Actor actor, (Inc1, Inc2) t) item = ref _array[_current];
                    return item.t;
                }
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public void Dispose()
            {
                _filter.Unlock();
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public bool MoveNext()
            {
                return ++_current < _count;
            }
        }

        public class Exclude<Exc1> : Filter<Inc1, Inc2>
            where Exc1 : Component
        {
            protected Exclude()
            {
                excArchetype = Archetype<Exc1>.Id;
            }
        }

        public class Exclude<Exc1, Exc2> : Filter<Inc1, Inc2>
            where Exc1 : Component
            where Exc2 : Component
        {
            protected Exclude()
            {
                excArchetype = Archetype<Exc1, Exc2>.Id;
            }
        }

        public class Exclude<Exc1, Exc2, Exc3> : Filter<Inc1, Inc2>
            where Exc1 : Component
            where Exc2 : Component
            where Exc3 : Component
        {
            protected Exclude()
            {
                excArchetype = Archetype<Exc1, Exc2, Exc3>.Id;
            }
        }

        public class Exclude<Exc1, Exc2, Exc3, Exc4> : Filter<Inc1, Inc2>
            where Exc1 : Component
            where Exc2 : Component
            where Exc3 : Component
            where Exc4 : Component
        {
            protected Exclude()
            {
                excArchetype = Archetype<Exc1, Exc2, Exc3, Exc4>.Id;
            }
        }

        public class Exclude<Exc1, Exc2, Exc3, Exc4, Exc5> : Filter<Inc1, Inc2>
            where Exc1 : Component
            where Exc2 : Component
            where Exc3 : Component
            where Exc4 : Component
            where Exc5 : Component
        {
            protected Exclude()
            {
                excArchetype = Archetype<Exc1, Exc2, Exc3, Exc4, Exc5>.Id;
            }
        }
    }

    public class Filter<Inc1, Inc2, Inc3> : Filter
        where Inc1 : Component
        where Inc2 : Component
        where Inc3 : Component
    {
        private readonly Container<(Inc1, Inc2, Inc3)> _container;

        protected Filter()
        {
            _container = new Container<(Inc1, Inc2, Inc3)>();

            incArchetype = Archetype<Inc1, Inc2, Inc3>.Id;
        }

        protected override void OnAdd(Actor actor)
        {
            if (actor.TryGetComponents(out Inc1 t1, out Inc2 t2, out Inc3 t3))
            {
                _container.Add(actor, (t1, t2, t3));
                LogWarning($"{GetType().GetTypeName()}.OnAdd({actor}_{actor.archetype})");
            }
        }

        protected override void OnRemove(Actor actor)
        {
            if (_container.Remove(actor))
            {
                LogWarning($"{GetType().GetTypeName()}.OnRemove({actor}_{actor.archetype})");
            }
        }

        protected override void OnDispose()
        {
            _container.Dispose();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Enumerator GetEnumerator()
        {
            return new Enumerator(this);
        }

        public struct Enumerator : IDisposable
        {
            private readonly Filter<Inc1, Inc2, Inc3> _filter;
            private readonly (Actor, (Inc1, Inc2, Inc3))[] _array;
            private readonly int _count;
            private int _current;

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            internal Enumerator(Filter<Inc1, Inc2, Inc3> filter)
            {
                _filter = filter;
                _array = _filter._container.array;
                _count = _filter._container.Count;
                _current = -1;
                _filter.Lock();
            }

            public (Inc1, Inc2, Inc3) Current
            {
                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                get
                {
                    ref readonly (Actor actor, (Inc1, Inc2, Inc3) t) item = ref _array[_current];
                    return item.t;
                }
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public void Dispose()
            {
                _filter.Unlock();
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public bool MoveNext()
            {
                return ++_current < _count;
            }
        }

        public class Exclude<Exc1> : Filter<Inc1, Inc2, Inc3>
            where Exc1 : Component
        {
            protected Exclude()
            {
                excArchetype = Archetype<Exc1>.Id;
            }
        }

        public class Exclude<Exc1, Exc2> : Filter<Inc1, Inc2, Inc3>
            where Exc1 : Component
            where Exc2 : Component
        {
            protected Exclude()
            {
                excArchetype = Archetype<Exc1, Exc2>.Id;
            }
        }

        public class Exclude<Exc1, Exc2, Exc3> : Filter<Inc1, Inc2, Inc3>
            where Exc1 : Component
            where Exc2 : Component
            where Exc3 : Component
        {
            protected Exclude()
            {
                excArchetype = Archetype<Exc1, Exc2, Exc3>.Id;
            }
        }

        public class Exclude<Exc1, Exc2, Exc3, Exc4> : Filter<Inc1, Inc2, Inc3>
            where Exc1 : Component
            where Exc2 : Component
            where Exc3 : Component
            where Exc4 : Component
        {
            protected Exclude()
            {
                excArchetype = Archetype<Exc1, Exc2, Exc3, Exc4>.Id;
            }
        }

        public class Exclude<Exc1, Exc2, Exc3, Exc4, Exc5> : Filter<Inc1, Inc2, Inc3>
            where Exc1 : Component
            where Exc2 : Component
            where Exc3 : Component
            where Exc4 : Component
            where Exc5 : Component
        {
            protected Exclude()
            {
                excArchetype = Archetype<Exc1, Exc2, Exc3, Exc4, Exc5>.Id;
            }
        }
    }

    public class Filter<Inc1, Inc2, Inc3, Inc4> : Filter
        where Inc1 : Component
        where Inc2 : Component
        where Inc3 : Component
        where Inc4 : Component
    {
        private readonly Container<(Inc1, Inc2, Inc3, Inc4)> _container;

        protected Filter()
        {
            _container = new Container<(Inc1, Inc2, Inc3, Inc4)>();

            incArchetype = Archetype<Inc1, Inc2, Inc3, Inc4>.Id;
        }

        protected override void OnAdd(Actor actor)
        {
            if (actor.TryGetComponents(out Inc1 t1, out Inc2 t2, out Inc3 t3, out Inc4 t4))
            {
                _container.Add(actor, (t1, t2, t3, t4));
                LogWarning($"{GetType().GetTypeName()}.OnAdd({actor}_{actor.archetype})");
            }
        }

        protected override void OnRemove(Actor actor)
        {
            if (_container.Remove(actor))
            {
                LogWarning($"{GetType().GetTypeName()}.OnRemove({actor}_{actor.archetype})");
            }
        }

        protected override void OnDispose()
        {
            _container.Dispose();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Enumerator GetEnumerator()
        {
            return new Enumerator(this);
        }

        public struct Enumerator : IDisposable
        {
            private readonly Filter<Inc1, Inc2, Inc3, Inc4> _filter;
            private readonly (Actor, (Inc1, Inc2, Inc3, Inc4))[] _array;
            private readonly int _count;
            private int _current;

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            internal Enumerator(Filter<Inc1, Inc2, Inc3, Inc4> filter)
            {
                _filter = filter;
                _array = _filter._container.array;
                _count = _filter._container.Count;
                _current = -1;
                _filter.Lock();
            }

            public (Inc1, Inc2, Inc3, Inc4) Current
            {
                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                get
                {
                    ref readonly (Actor actor, (Inc1, Inc2, Inc3, Inc4) t) item = ref _array[_current];
                    return item.t;
                }
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public void Dispose()
            {
                _filter.Unlock();
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public bool MoveNext()
            {
                return ++_current < _count;
            }
        }

        public class Exclude<Exc1> : Filter<Inc1, Inc2, Inc3, Inc4>
            where Exc1 : Component
        {
            protected Exclude()
            {
                excArchetype = Archetype<Exc1>.Id;
            }
        }

        public class Exclude<Exc1, Exc2> : Filter<Inc1, Inc2, Inc3, Inc4>
            where Exc1 : Component
            where Exc2 : Component
        {
            protected Exclude()
            {
                excArchetype = Archetype<Exc1, Exc2>.Id;
            }
        }

        public class Exclude<Exc1, Exc2, Exc3> : Filter<Inc1, Inc2, Inc3, Inc4>
            where Exc1 : Component
            where Exc2 : Component
            where Exc3 : Component
        {
            protected Exclude()
            {
                excArchetype = Archetype<Exc1, Exc2, Exc3>.Id;
            }
        }

        public class Exclude<Exc1, Exc2, Exc3, Exc4> : Filter<Inc1, Inc2, Inc3, Inc4>
            where Exc1 : Component
            where Exc2 : Component
            where Exc3 : Component
            where Exc4 : Component
        {
            protected Exclude()
            {
                excArchetype = Archetype<Exc1, Exc2, Exc3, Exc4>.Id;
            }
        }

        public class Exclude<Exc1, Exc2, Exc3, Exc4, Exc5> : Filter<Inc1, Inc2, Inc3, Inc4>
            where Exc1 : Component
            where Exc2 : Component
            where Exc3 : Component
            where Exc4 : Component
            where Exc5 : Component
        {
            protected Exclude()
            {
                excArchetype = Archetype<Exc1, Exc2, Exc3, Exc4, Exc5>.Id;
            }
        }
    }

    public class Filter<Inc1, Inc2, Inc3, Inc4, Inc5> : Filter
        where Inc1 : Component
        where Inc2 : Component
        where Inc3 : Component
        where Inc4 : Component
        where Inc5 : Component
    {
        private readonly Container<(Inc1, Inc2, Inc3, Inc4, Inc5)> _container;

        protected Filter()
        {
            _container = new Container<(Inc1, Inc2, Inc3, Inc4, Inc5)>();

            incArchetype = Archetype<Inc1, Inc2, Inc3, Inc4, Inc5>.Id;
        }

        protected override void OnAdd(Actor actor)
        {
            if (actor.TryGetComponents(out Inc1 t1, out Inc2 t2, out Inc3 t3, out Inc4 t4, out Inc5 t5))
            {
                _container.Add(actor, (t1, t2, t3, t4, t5));
                LogWarning($"{GetType().GetTypeName()}.OnAdd({actor}_{actor.archetype})");
            }
        }

        protected override void OnRemove(Actor actor)
        {
            if (_container.Remove(actor))
            {
                LogWarning($"{GetType().GetTypeName()}.OnRemove({actor}_{actor.archetype})");
            }
        }

        protected override void OnDispose()
        {
            _container.Dispose();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Enumerator GetEnumerator()
        {
            return new Enumerator(this);
        }

        public struct Enumerator : IDisposable
        {
            private readonly Filter<Inc1, Inc2, Inc3, Inc4, Inc5> _filter;
            private readonly (Actor, (Inc1, Inc2, Inc3, Inc4, Inc5))[] _array;
            private readonly int _count;
            private int _current;

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            internal Enumerator(Filter<Inc1, Inc2, Inc3, Inc4, Inc5> filter)
            {
                _filter = filter;
                _array = _filter._container.array;
                _count = _filter._container.Count;
                _current = -1;
                _filter.Lock();
            }

            public (Inc1, Inc2, Inc3, Inc4, Inc5) Current
            {
                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                get
                {
                    ref readonly (Actor actor, (Inc1, Inc2, Inc3, Inc4, Inc5) t) item = ref _array[_current];
                    return item.t;
                }
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public void Dispose()
            {
                _filter.Unlock();
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public bool MoveNext()
            {
                return ++_current < _count;
            }
        }

        public class Exclude<Exc1> : Filter<Inc1, Inc2, Inc3, Inc4, Inc5>
            where Exc1 : Component
        {
            protected Exclude()
            {
                excArchetype = Archetype<Exc1>.Id;
            }
        }

        public class Exclude<Exc1, Exc2> : Filter<Inc1, Inc2, Inc3, Inc4, Inc5>
            where Exc1 : Component
            where Exc2 : Component
        {
            protected Exclude()
            {
                excArchetype = Archetype<Exc1, Exc2>.Id;
            }
        }

        public class Exclude<Exc1, Exc2, Exc3> : Filter<Inc1, Inc2, Inc3, Inc4, Inc5>
            where Exc1 : Component
            where Exc2 : Component
            where Exc3 : Component
        {
            protected Exclude()
            {
                excArchetype = Archetype<Exc1, Exc2, Exc3>.Id;
            }
        }

        public class Exclude<Exc1, Exc2, Exc3, Exc4> : Filter<Inc1, Inc2, Inc3, Inc4, Inc5>
            where Exc1 : Component
            where Exc2 : Component
            where Exc3 : Component
            where Exc4 : Component
        {
            protected Exclude()
            {
                excArchetype = Archetype<Exc1, Exc2, Exc3, Exc4>.Id;
            }
        }

        public class Exclude<Exc1, Exc2, Exc3, Exc4, Exc5> : Filter<Inc1, Inc2, Inc3, Inc4, Inc5>
            where Exc1 : Component
            where Exc2 : Component
            where Exc3 : Component
            where Exc4 : Component
            where Exc5 : Component
        {
            protected Exclude()
            {
                excArchetype = Archetype<Exc1, Exc2, Exc3, Exc4, Exc5>.Id;
            }
        }
    }
}