using System;
using UnityEditor;

namespace Vena.Blockly.Editor.UI
{

    /// <summary>
    /// Editor 端 IBlocklyDebugChannel 实现 v0（Editor/UI 合约 §4）。
    /// 三事件 → BlocklyEditorWindow 端的节点高亮 / 值预览 stub。
    /// 单线程、同步回调；不做时间轴 / 历史回放 / 远程调试。
    /// </summary>
    public sealed class EditorDebugChannel : IBlocklyDebugChannel
    {
        private readonly BlocklyEditorWindow _window;

        public EditorDebugChannel(BlocklyEditorWindow window)
        {
            _window = window;
        }

        public void OnNodeEnter(Guid nodeGuid)
        {
            // 由于 IR Guid (128bit) → Runtime IBlocklySource.Guid (ulong) 折叠，
            // 这里收到的 nodeGuid 来自 Runtime，需要由 host 注入回 IR Guid。
            // PR-9 v0：仅记录到 console；UI 节点高亮的精确映射延后。
            UnityEngine.Debug.Log($"[BlocklyDebug] Enter {nodeGuid:D}");
        }

        public void OnNodeExit(Guid nodeGuid, BehaviorResult result)
        {
            UnityEngine.Debug.Log($"[BlocklyDebug] Exit  {nodeGuid:D} result={result}");
        }

        public void OnValueProduced(Guid nodeGuid, IBoxedValue value)
        {
            UnityEngine.Debug.Log($"[BlocklyDebug] Value {nodeGuid:D} = {value}");
        }
    }
}
