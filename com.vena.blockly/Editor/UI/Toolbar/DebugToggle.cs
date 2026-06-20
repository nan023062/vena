using UnityEditor;
using UnityEngine;

namespace Vena.Blockly.Editor.UI
{

    /// <summary>
    /// Debug Toggle（Editor/UI 合约 §2 顶部工具条）。
    /// 启用时把 EditorDebugChannel 接入 BlocklyDebugChannelRegistry；运行节点时将命中可视化。
    /// </summary>
    public sealed class DebugToggle
    {
        private bool _on;

        public bool IsOn => _on;

        public void Draw()
        {
            bool newOn = GUILayout.Toggle(_on, "Debug", EditorStyles.toolbarButton, GUILayout.Width(50));
            if (newOn != _on)
            {
                _on = newOn;
            }
        }
    }
}
