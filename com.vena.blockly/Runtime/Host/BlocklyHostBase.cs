// -----------------------------------------------------------------------------
// Vena Blockly
// Visual scripting block graph runtime for the Vena open-source Unity framework.
// Copyright (c) Nan Li.
// Licensed under the terms defined in the repository LICENSE file.
// -----------------------------------------------------------------------------

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
            private bool _initialized;
            private INodeMetadataProvider _metadataProvider;

            internal INodeMetadataProvider MetadataProvider => _metadataProvider;

            public T Create<T>(IBlocklySource source) where T : class
            {
                return (T)Activator.CreateInstance(typeof(T));
            }

            /// <summary>
            /// 一次性反射装载：在已加载程序集中找到首个非抽象 <see cref="INodeMetadataProvider"/> 实现，
            /// 实例化并持有。多次调用幂等。生产宿主可 override <c>NodeFactory</c> 走自定义实现替换之。
            /// </summary>
            public void Initialize()
            {
                if (_initialized) return;
                _initialized = true;

                var providerType = FindFirstMetadataProviderType();
                if (providerType != null)
                {
                    _metadataProvider = (INodeMetadataProvider)Activator.CreateInstance(providerType);
                }
            }

            private static Type FindFirstMetadataProviderType()
            {
                var assemblies = AppDomain.CurrentDomain.GetAssemblies();
                foreach (var asm in assemblies)
                {
                    Type[] types;
                    try { types = asm.GetTypes(); }
                    catch (System.Reflection.ReflectionTypeLoadException ex)
                    {
                        types = ex.Types;
                    }

                    for (int i = 0; i < types.Length; i++)
                    {
                        var t = types[i];
                        if (t == null || !t.IsClass || t.IsAbstract || t.IsInterface) continue;
                        if (typeof(INodeMetadataProvider).IsAssignableFrom(t))
                        {
                            return t;
                        }
                    }
                }
                return null;
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
