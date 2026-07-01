// -----------------------------------------------------------------------------
// Vena Blockly
// Visual scripting block graph runtime for the Vena open-source Unity framework.
// Copyright (c) Nan Li.
// Licensed under the terms defined in the repository LICENSE file.
// -----------------------------------------------------------------------------

using UnityEditor.Experimental.GraphView;

namespace Vena.Blockly.Editor.UI
{

    /// <summary>
    /// 双 wire 连线兼容性校验。
    /// 规则：
    ///   - 控制端口仅接 ControlWire（端口 userData = WireKind.Control，类型 NodeView.ControlFlow）。
    ///   - 值端口仅接 ValueWire（端口 userData = WireKind.Value，类型 NodeView.ValueFlow）。
    ///   - 端口入度上限：控制 1 / 值 1（Capacity.Single）；出度无限。
    ///   - 类型校验委派 [ExpressionSignature]。
    ///   - 检测到环 / 类型不符 → UI 拒绝（GraphView 默认 + 这里加 wireKind 校验）。
    /// </summary>
    public static class EdgeConnector
    {
        public static bool IsCompatible(Port a, Port b)
        {
            if (a == null || b == null) return false;
            if (a.portType != b.portType) return false;

            var aKind = a.userData as WireKind?;
            var bKind = b.userData as WireKind?;
            if (!aKind.HasValue || !bKind.HasValue) return false;
            if (aKind.Value != bKind.Value) return false;

            // 控制连线只能在 IBehaviorNode 之间。
            // 值连线允许 IExpressionBlock 输出端 → 任意端口。
            // 这层语义需要 sourceType 元数据，待接入 INodeMetadataProvider 后落细。
            return true;
        }

        /// <summary>从端口 userData 推断 wireKind（用于 GraphView 创建 Edge 时填 userData）。</summary>
        public static WireKind InferWireKind(Port output, Port input)
        {
            if (output?.userData is WireKind wk) return wk;
            if (input?.userData is WireKind wk2) return wk2;
            return WireKind.Control;
        }
    }
}
