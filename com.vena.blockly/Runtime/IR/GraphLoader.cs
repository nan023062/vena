// -----------------------------------------------------------------------------
// Vena Blockly
// Visual scripting block graph runtime for the Vena open-source Unity framework.
// Copyright (c) Nan Li.
// Licensed under the terms defined in the repository LICENSE file.
// -----------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Reflection;

namespace Vena.Blockly
{

    /// <summary>
    /// IR → Source 树加载器。
    ///
    /// 输入：<see cref="GraphIR"/>（已由 Editor 侧 <c>IBlocklyGraphSerializer.FromJson</c> 解码）。
    /// 输出：根 <see cref="IBlocklySource"/>（`BehaviorGraph` / `LogicGraph`），调用方再交给 NodeFactory 构造运行期节点。
    ///
    /// 不进 IBlocklyHost、不扩聚合门面；调用方持有引用即可。
    /// AOT 友好：source 实例化经 <see cref="ISourceActivator"/> 抽象（默认走反射；codegen 可注入静态产物）。
    /// 本身实现仅用 <see cref="System.Reflection"/>（mscorlib 自带），不引 UnityEngine。
    /// </summary>
    public sealed class GraphLoader
    {
        private readonly ISourceActivator _activator;

        /// <summary>使用反射 activator 的默认构造。</summary>
        public GraphLoader() : this(new ReflectionSourceActivator()) { }

        /// <summary>注入自定义 activator（AOT 时由 codegen 提供静态委托表）。</summary>
        public GraphLoader(ISourceActivator activator)
        {
            _activator = activator ?? throw new ArgumentNullException(nameof(activator));
        }

        /// <summary>加载 Behavior 图。kind 不匹配抛 <see cref="InvalidOperationException"/>。</summary>
        public BehaviorGraph LoadBehavior(GraphIR ir)
        {
            if (ir == null) throw new ArgumentNullException(nameof(ir));
            if (ir.Kind != GraphKind.Behavior)
                throw new InvalidOperationException($"GraphIR.Kind = {ir.Kind}; expected Behavior.");
            return (BehaviorGraph)Build(ir, expectRoot: typeof(BehaviorGraph));
        }

        /// <summary>加载 Logic 图。kind 不匹配抛 <see cref="InvalidOperationException"/>。</summary>
        public LogicGraph LoadLogic(GraphIR ir)
        {
            if (ir == null) throw new ArgumentNullException(nameof(ir));
            if (ir.Kind != GraphKind.Logic)
                throw new InvalidOperationException($"GraphIR.Kind = {ir.Kind}; expected Logic.");
            return (LogicGraph)Build(ir, expectRoot: typeof(LogicGraph));
        }

        // ---------------------------------------------------------------- internals

        private object Build(GraphIR ir, Type expectRoot)
        {
            // 1. 拆 nodes 字典 (guid -> NodeIR)。
            var byGuid = new Dictionary<Guid, NodeIR>(ir.Nodes.Count);
            foreach (var n in ir.Nodes)
            {
                if (byGuid.ContainsKey(n.Guid))
                    throw new InvalidOperationException($"Duplicate NodeIR guid in IR: {n.Guid}");
                byGuid.Add(n.Guid, n);
            }

            // 2. 拆 value-edges：to(nodeGuid+port) -> from(nodeGuid)。
            //    值入度上限 = 1，因此每个 (nodeGuid,port) 对应至多一条 value 入边。
            var valueEdges = new Dictionary<PortRef, Guid>(ir.Edges.Count);
            foreach (var e in ir.Edges)
            {
                if (e.WireKind != WireKind.Value) continue;
                if (valueEdges.ContainsKey(e.To))
                    throw new InvalidOperationException(
                        $"Multiple value edges target port ({e.To.NodeGuid},{e.To.Port}); 值入度上限 = 1.");
                valueEdges.Add(e.To, e.From.NodeGuid);
            }

            // 3. 实例化 source 缓存（per-Guid 单例，避免 DAG 多入但循环引用）。
            var instances = new Dictionary<Guid, IBlocklySource>(ir.Nodes.Count);

            // 4. 检测根节点。
            if (!byGuid.TryGetValue(ir.RootNodeGuid, out var rootIR))
                throw new InvalidOperationException($"GraphIR.rootNodeGuid not in nodes: {ir.RootNodeGuid}");

            // 5. 递归构造（深度优先）。环检测：当前递归栈集合。
            var visiting = new HashSet<Guid>();
            var rootSrc = Materialize(rootIR, byGuid, valueEdges, instances, visiting);

            if (!expectRoot.IsInstanceOfType(rootSrc))
                throw new InvalidOperationException(
                    $"Root source type mismatch: expected {expectRoot.Name}, got {rootSrc.GetType().Name}");

            return rootSrc;
        }

        private IBlocklySource Materialize(
            NodeIR nodeIR,
            Dictionary<Guid, NodeIR> byGuid,
            Dictionary<PortRef, Guid> valueEdges,
            Dictionary<Guid, IBlocklySource> instances,
            HashSet<Guid> visiting)
        {
            if (instances.TryGetValue(nodeIR.Guid, out var cached)) return cached;
            if (!visiting.Add(nodeIR.Guid))
                throw new InvalidOperationException(
                    $"Cycle detected at NodeIR guid={nodeIR.Guid}; IR must be acyclic.");

            var sourceType = ResolveType(nodeIR.SourceType);
            var instance = _activator.Activate(sourceType);
            if (instance == null)
                throw new InvalidOperationException(
                    $"ISourceActivator returned null for type {sourceType.FullName}.");
            instances[nodeIR.Guid] = instance;

            // 把 NodeIR.Guid 投影到 IBlocklySource.Guid（ulong 域）—— 取低 64 位。
            // 仅作为运行期身份占位；IR 端 Guid（128 位）才是稳定身份。
            // 设置 Guid 走反射 setter（IBlocklySource.Guid 接口仅 readonly，但具体类有 set；
            // 这里允许具体类没有 setter 时静默跳过）。
            TrySetGuid(instance, nodeIR.Guid);

            // 字段填充：每个 [UgcSourceProperty] 槽位一条 NodePropertyIR。
            // 槽位类型：
            //   - 值流子节点（如 BehaviorNodeSource / Expression 引用）→ properties[].value 应为 nodeRef。
            //   - 字面值（int / string / bool / float / ...）            → properties[].value 应为 literal。
            // 顺序按 IR 给出。
            foreach (var prop in nodeIR.Properties)
            {
                var fi = ResolvePropertySlot(sourceType, prop.Key);
                AssignSlot(instance, fi, prop, nodeIR, byGuid, valueEdges, instances, visiting);
            }

            // 子节点引用：除字段直填外，IR 中可能用 ValueWire 表达连线。
            // valueEdges[(nodeGuid=nodeIR.Guid, port=<key>)] = sourceGuid → 等价覆写槽位为该子节点。
            // 这条路径是 Editor.UI 写图的标准形态（ValueWire 显式连线）；
            // 与 properties[].value=nodeRef（内嵌引用）等价且可共存。优先 valueEdges 表达。
            if (valueEdges.Count > 0)
            {
                foreach (var kv in valueEdges)
                {
                    if (kv.Key.NodeGuid != nodeIR.Guid) continue;
                    var fi = ResolvePropertySlot(sourceType, kv.Key.Port);
                    if (!byGuid.TryGetValue(kv.Value, out var srcIR))
                        throw new InvalidOperationException(
                            $"ValueEdge source nodeGuid {kv.Value} not in IR.nodes.");
                    var child = Materialize(srcIR, byGuid, valueEdges, instances, visiting);
                    if (!fi.FieldType.IsInstanceOfType(child))
                        throw new InvalidOperationException(
                            $"ValueEdge type mismatch: slot {sourceType.Name}.{fi.Name} expects {fi.FieldType.Name}, got {child.GetType().Name}.");
                    fi.SetValue(instance, child);
                }
            }

            visiting.Remove(nodeIR.Guid);
            return instance;
        }

        private void AssignSlot(
            IBlocklySource owner,
            FieldInfo fi,
            NodePropertyIR prop,
            NodeIR ownerIR,
            Dictionary<Guid, NodeIR> byGuid,
            Dictionary<PortRef, Guid> valueEdges,
            Dictionary<Guid, IBlocklySource> instances,
            HashSet<Guid> visiting)
        {
            if (prop.Value == null) return;

            if (prop.Value.IsNodeRef)
            {
                if (!(prop.Value.Value is Guid g))
                    throw new InvalidOperationException(
                        $"PropertyValueIR.nodeRef.value should be Guid (slot {fi.Name}).");
                if (!byGuid.TryGetValue(g, out var srcIR))
                    throw new InvalidOperationException(
                        $"NodeRef target {g} not in IR.nodes (slot {fi.Name}).");
                var child = Materialize(srcIR, byGuid, valueEdges, instances, visiting);
                if (!fi.FieldType.IsInstanceOfType(child))
                    throw new InvalidOperationException(
                        $"NodeRef type mismatch: slot {owner.GetType().Name}.{fi.Name} expects {fi.FieldType.Name}, got {child.GetType().Name}.");
                fi.SetValue(owner, child);
                return;
            }

            // literal：依目标字段类型转换原始 JSON 标量。
            object literal = ConvertLiteral(prop.Value.Value, fi.FieldType, fi.Name);
            fi.SetValue(owner, literal);
        }

        private static FieldInfo ResolvePropertySlot(Type sourceType, string key)
        {
            var fi = sourceType.GetField(key, BindingFlags.Public | BindingFlags.Instance);
            if (fi != null) return fi;
            // 父类继承字段也允许（静态匹配优先，但向上回退至基类）。
            for (var t = sourceType.BaseType; t != null; t = t.BaseType)
            {
                fi = t.GetField(key, BindingFlags.Public | BindingFlags.Instance);
                if (fi != null) return fi;
            }
            throw new InvalidOperationException(
                $"Property key '{key}' not found on source type {sourceType.FullName}.");
        }

        private static object ConvertLiteral(object raw, Type target, string slotName)
        {
            if (raw == null)
            {
                if (target.IsValueType && Nullable.GetUnderlyingType(target) == null)
                    throw new InvalidOperationException(
                        $"Cannot assign null to non-nullable slot {slotName}: {target.Name}");
                return null;
            }
            if (target.IsInstanceOfType(raw)) return raw;

            // 枚举：接受字符串名 / 整数。
            if (target.IsEnum)
            {
                if (raw is string s) return Enum.Parse(target, s, ignoreCase: false);
                return Enum.ToObject(target, raw);
            }

            // 数值类型转换。
            try
            {
                return Convert.ChangeType(raw, target, System.Globalization.CultureInfo.InvariantCulture);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException(
                    $"Cannot convert literal '{raw}' ({raw.GetType().Name}) to {target.Name} for slot {slotName}.", ex);
            }
        }

        private static void TrySetGuid(IBlocklySource instance, Guid g)
        {
            // IBlocklySource.Guid 是 ulong；把 IR Guid 折叠到 ulong（取前 8 字节大端）。
            var pi = instance.GetType().GetProperty("Guid",
                BindingFlags.Public | BindingFlags.Instance);
            if (pi == null || !pi.CanWrite) return;
            if (pi.PropertyType != typeof(ulong)) return;
            ulong folded = FoldGuidToUlong(g);
            pi.SetValue(instance, folded);
        }

        private static ulong FoldGuidToUlong(Guid g)
        {
            var bytes = g.ToByteArray(); // 16 bytes
            ulong hi = 0, lo = 0;
            for (int i = 0; i < 8; i++) hi = (hi << 8) | bytes[i];
            for (int i = 8; i < 16; i++) lo = (lo << 8) | bytes[i];
            return hi ^ lo;
        }

        private static Type ResolveType(string aqn)
        {
            if (string.IsNullOrEmpty(aqn))
                throw new InvalidOperationException("NodeIR.sourceType is empty.");
            // 走全程序集扫描（Type.GetType + 已加载程序集逐个 GetType）。
            var t = Type.GetType(aqn, throwOnError: false, ignoreCase: false);
            if (t != null) return t;
            foreach (var asm in AppDomain.CurrentDomain.GetAssemblies())
            {
                t = asm.GetType(StripAssemblySuffix(aqn), throwOnError: false, ignoreCase: false);
                if (t != null) return t;
            }
            throw new InvalidOperationException($"Cannot resolve sourceType: {aqn}");
        }

        private static string StripAssemblySuffix(string aqn)
        {
            int comma = aqn.IndexOf(',');
            return comma < 0 ? aqn : aqn.Substring(0, comma).Trim();
        }
    }

    /// <summary>
    /// Source 实例化抽象 —— 默认走反射；AOT 场景由 codegen 注入静态委托表替换实现。
    /// </summary>
    public interface ISourceActivator
    {
        IBlocklySource Activate(Type sourceType);
    }

    /// <summary>反射默认实现：通过公共无参构造创建实例。</summary>
    public sealed class ReflectionSourceActivator : ISourceActivator
    {
        public IBlocklySource Activate(Type sourceType)
        {
            if (sourceType == null) throw new ArgumentNullException(nameof(sourceType));
            if (!typeof(IBlocklySource).IsAssignableFrom(sourceType))
                throw new InvalidOperationException(
                    $"Type {sourceType.FullName} does not implement IBlocklySource.");
            var ctor = sourceType.GetConstructor(
                BindingFlags.Public | BindingFlags.Instance,
                binder: null,
                types: Type.EmptyTypes,
                modifiers: null);
            if (ctor == null)
                throw new InvalidOperationException(
                    $"Type {sourceType.FullName} has no public parameterless constructor.");
            return (IBlocklySource)ctor.Invoke(null);
        }
    }
}
