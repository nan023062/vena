// -----------------------------------------------------------------------------
// Vena Blockly
// Visual scripting block graph runtime for the Vena open-source Unity framework.
// Copyright (c) Nan Li.
// Licensed under the terms defined in the repository LICENSE file.
// -----------------------------------------------------------------------------

using System;

namespace Vena.Blockly.Tests.Codegen
{
    /// <summary>
    /// Demo 用 host：override BlocklyHostBase.NodeFactory 注入 ReflectionNodeFactory。
    ///
    /// Demo D 当前流程（Editor 菜单触发 codegen 写盘 .g.cs）实际上不调用任何
    /// NodeFactory.Create——它 Setup + Run 都在 Editor 期，扫源类、emit 字符串。
    /// 本类的存在仅为「四 demo 一致的 host 形态」记号——若用户随后把生成的
    /// InstanceMethodTestMethod*.g.cs 拉到一张 LogicGraph 里 Tick，
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
