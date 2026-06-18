using System;

namespace Vena.Blockly
{

    public abstract class BlocklyHostBase : IBlocklyHost
    {
        private IBlocklyLogger _logger;
        private IBlocklyNodeFactory _nodeFactory;
        private IBlocklyPool _pool;
        private IBlocklySerializer _serializer;
        private IBlocklyVariableStorageFactory _variableStorageFactory;

        public virtual IBlocklyLogger Logger => _logger ??= new NullLogger();

        public virtual IBlocklyNodeFactory NodeFactory => _nodeFactory ??= new ActivatorNodeFactory();

        public virtual IBlocklyPool Pool => _pool ??= new NoOpPool();

        public virtual IBlocklySerializer Serializer => _serializer ??= new NoOpSerializer();

        public virtual IBlocklyVariableStorageFactory VariableStorageFactory =>
            _variableStorageFactory ??= new DictionaryVariableStorageFactory();

        private sealed class NullLogger : IBlocklyLogger
        {
            public void Debug(string message) { }
            public void Warning(string message) { }
            public void Error(string message) { }
        }

        private sealed class ActivatorNodeFactory : IBlocklyNodeFactory
        {
            public T Create<T>(IBlocklySource source) where T : class
            {
                return (T)Activator.CreateInstance(typeof(T));
            }
        }

        private sealed class NoOpPool : IBlocklyPool
        {
            public T Get<T>() where T : class, new() => new T();
            public void Return(object instance) { }
        }

        private sealed class NoOpSerializer : IBlocklySerializer
        {
            public int ReadInt32() => 0;
            public void WriteInt32(int value) { }
            public string ReadString() => null;
            public void WriteString(string value) { }
            public bool ReadBoolean() => false;
            public void WriteBoolean(bool value) { }
        }
    }
}
