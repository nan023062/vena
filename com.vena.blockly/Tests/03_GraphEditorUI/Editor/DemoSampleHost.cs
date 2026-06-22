// -----------------------------------------------------------------------------
// Vena Blockly
// Visual scripting block graph runtime for the Vena open-source Unity framework.
// Copyright (c) Nan Li.
// Licensed under the terms defined in the repository LICENSE file.
// -----------------------------------------------------------------------------

using System;

namespace Vena.Blockly.Tests.GraphEditorUI
{
    /// <summary>
    /// Demo 用 host：override BlocklyHostBase.NodeFactory 注入 ReflectionNodeFactory。
    ///
    /// Demo C 当前流程（Editor 菜单 + GraphAsset placeholder JSON）实际上不调用任何
    /// NodeFactory.Create；本类的存在仅为「四 demo 一致的 host 形态」记号——若 Demo C
    /// 后续扩展为真实 runtime 跑图（即把 GraphAsset 反序列化回 BehaviorGraph/LogicGraph 并 Tick），
    /// 直接 new DemoSampleHost() 即可，与 Demo 01/02 同形。
    /// </summary>
    public sealed class DemoSampleHost : BlocklyHostBase
    {
        private IBlocklyNodeFactory _nodeFactory;

        public override IBlocklyNodeFactory NodeFactory => _nodeFactory ??= new ReflectionNodeFactory();

        private sealed class ReflectionNodeFactory : IBlocklyNodeFactory
        {
            public T Create<T>(IBlocklySource source) where T : class
            {
                if (source == null) throw new ArgumentNullException(nameof(source));

                Type nodeType = BlocklySourceAttribute.GetNodeType(source.GetType());
                if (nodeType == null)
                {
                    throw new InvalidOperationException(
                        $"[ReflectionNodeFactory] Source type {source.GetType().FullName} is missing [BlocklySource]; " +
                        $"cannot resolve target node type.");
                }

                var instance = Activator.CreateInstance(nodeType);
                if (instance is T typed) return typed;

                throw new InvalidOperationException(
                    $"[ReflectionNodeFactory] Resolved node type {nodeType.FullName} for source " +
                    $"{source.GetType().FullName} is not assignable to {typeof(T).FullName}.");
            }

            public void Initialize() { }
        }
    }
}
