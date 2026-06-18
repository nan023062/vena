using System.Collections.Generic;

namespace Vena.Blockly
{

    public sealed class BoxedValue<T> : IBoxedValue
    {
        private static readonly Stack<BoxedValue<T>> _pool = new Stack<BoxedValue<T>>();

        public T value;

        public static BoxedValue<T> Create(T v)
        {
            BoxedValue<T> instance;
            if (_pool.Count > 0)
            {
                instance = _pool.Pop();
            }
            else
            {
                instance = new BoxedValue<T>();
            }
            instance.value = v;
            return instance;
        }

        public void Dispose()
        {
            value = default;
            _pool.Push(this);
        }
    }
}
