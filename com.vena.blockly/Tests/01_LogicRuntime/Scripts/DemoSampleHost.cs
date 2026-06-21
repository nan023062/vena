// -----------------------------------------------------------------------------
// Vena Blockly
// Visual scripting block graph runtime for the Vena open-source Unity framework.
// Copyright (c) Nan Li.
// Licensed under the terms defined in the repository LICENSE file.
// -----------------------------------------------------------------------------

using System;

namespace Vena.Blockly.Tests.LogicRuntime
{
    /// <summary>
    /// Demo 用 host：override BlocklyHostBase.NodeFactory 注入 ReflectionNodeFactory。
    /// 默认 BlocklyHostBase.NodeFactory = ActivatorNodeFactory，其 Create&lt;T&gt; 直接
    /// Activator.CreateInstance(typeof(T))；T 为 ILogicNode/IBehaviorNode 接口时无法实例化。
    /// 本 demo 通过 [UgcSource(menuPath, typeof(NodeType))] 注解把源类与具体 NodeType 绑定，
    /// ReflectionNodeFactory 走 UgcSourceAttribute.GetObjectType 反射定位 NodeType 并实例化。
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

                Type nodeType = UgcSourceAttribute.GetObjectType(source.GetType());
                if (nodeType == null)
                {
                    throw new InvalidOperationException(
                        $"[ReflectionNodeFactory] Source type {source.GetType().FullName} is missing [UgcSource]; " +
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
