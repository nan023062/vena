// -----------------------------------------------------------------------------
// Vena Blockly
// Visual scripting block graph runtime for the Vena open-source Unity framework.
// Copyright (c) Nan Li.
// Licensed under the terms defined in the repository LICENSE file.
// -----------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq;

namespace Vena.Blockly
{

    /// <summary>
    /// Blockly 实例管理 (partial)。
    /// 统一管理 Source.Guid → Instance.InstanceID 映射。
    /// 所有创建/销毁都对接 IBlocklyHost.Pool / NodeFactory（预留 pool 化）。
    /// </summary>
    public abstract partial class Blockly
    {
        private Dictionary<object, ulong> _instance2Guid;

        private Dictionary<ulong, object> _guid2Instance;

        public T GetInstanceByGuid<T>(ulong guid) where T : class
        {
            // 在自己作用域查找实例
            if (null != _guid2Instance && _guid2Instance.TryGetValue(guid, out object instance))
            {
                return (T)instance;
            }
            // 沿作用域链向上查找
            return _parent?.GetInstanceByGuid<T>(guid);
        }

        protected void RegisterInstanceInternal(ulong guid, object instance)
        {
            if (null == _guid2Instance)
            {
                _guid2Instance = new Dictionary<ulong, object>();

                _instance2Guid = new Dictionary<object, ulong>();
            }

            _guid2Instance.Add(guid, instance);

            _instance2Guid.Add(instance, guid);
        }

        protected bool UnregisterInstanceInternal(object instance)
        {
            if(null != _guid2Instance && _instance2Guid.TryGetValue(instance, out ulong guid))
            {
                _guid2Instance.Remove(guid);

                _instance2Guid.Remove(instance);

                return true;
            }
            return false;
        }

        private void ClearAllInstances()
        {
            try
            {
                if (_guid2Instance != null && _guid2Instance.Count > 0)
                {
                    object[] array = _guid2Instance.Values.ToArray();

                    // back to pool
                    foreach (var instance in array)
                    {
                        if(instance is IBlock block)
                        {
                            DestroyBlock(block);
                        }
                        else if(instance is Blockly blockly)
                        {
                            DestroyBlockly(blockly);
                        }
                    }
                }
            }
            finally
            {
                _guid2Instance?.Clear();

                _instance2Guid?.Clear();
            }
        }

        #region Sub-Blockly Create / Destroy

        public LogicGraph.Blockly CreateBlockly(LogicGraph source)
        {
            var blockly = Host.Pool.Get<LogicGraph.Blockly>();

            RegisterInstanceInternal( source.Guid, blockly);

            blockly.SetParent(this);

            blockly.SetSource(source);

            return blockly;
        }



        #endregion

        public void DestroyBlock(IBlock block)
        {
            if (UnregisterInstanceInternal(block))
            {
                block.Destroy();

                Host.Pool.Return(block);
            }
        }

        public void DestroyBlockly(Blockly blockly)
        {
            if(UnregisterInstanceInternal(blockly))
            {
                blockly.Destroy();

                Host.Pool.Return(blockly);
            }
        }
    }
}
