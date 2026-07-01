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
    /// 统一管理 Source.InstanceId → Instance 映射。
    /// 所有创建/销毁都对接 IBlocklyHost.Pool / NodeFactory（预留 pool 化）。
    /// </summary>
    public abstract partial class Blockly
    {
        private Dictionary<object, ulong> _instance2Id;

        private Dictionary<ulong, object> _id2Instance;

        public T GetInstanceById<T>(ulong instanceId) where T : class
        {
            // 在自己作用域查找实例
            if (null != _id2Instance && _id2Instance.TryGetValue(instanceId, out object instance))
            {
                return (T)instance;
            }
            // 沿作用域链向上查找
            return _parent?.GetInstanceById<T>(instanceId);
        }

        protected void RegisterInstanceInternal(ulong instanceId, object instance)
        {
            if (null == _id2Instance)
            {
                _id2Instance = new Dictionary<ulong, object>();

                _instance2Id = new Dictionary<object, ulong>();
            }

            _id2Instance.Add(instanceId, instance);

            _instance2Id.Add(instance, instanceId);
        }

        protected bool UnregisterInstanceInternal(object instance)
        {
            if(null != _id2Instance && _instance2Id.TryGetValue(instance, out ulong instanceId))
            {
                _id2Instance.Remove(instanceId);

                _instance2Id.Remove(instance);

                return true;
            }
            return false;
        }

        private void ClearAllInstances()
        {
            try
            {
                if (_id2Instance != null && _id2Instance.Count > 0)
                {
                    object[] array = _id2Instance.Values.ToArray();

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
                _id2Instance?.Clear();

                _instance2Id?.Clear();
            }
        }

        #region Sub-Blockly Create / Destroy

        public ExpressionBlockly.Blockly CreateBlockly(ExpressionBlockly source)
        {
            var blockly = Host.Pool.Get<ExpressionBlockly.Blockly>();

            RegisterInstanceInternal( source.InstanceId, blockly);

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
