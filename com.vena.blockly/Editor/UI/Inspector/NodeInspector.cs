using System;
using UnityEngine;
using UnityEngine.UIElements;

namespace Vena.Blockly.Editor.UI
{

    /// <summary>
    /// 节点属性面板（Editor/UI 合约 §2 / Key Decision 5）。
    ///
    /// 数据源 = NodeIR.Properties（kv 直绑）；编辑回写 IR、不直接持有运行期实例。
    /// 字段顺序按 [UgcSourceProperty.order] 升序（顺序锁三者一致）；这里按 IR 列表顺序。
    /// PR-9：当 DebugChannel 触发 OnValueProduced 时叠加 value 预览。
    /// </summary>
    public sealed class NodeInspector : VisualElement
    {
        public event Action OnEdit;

        private NodeIR _bound;
        private readonly ScrollView _scroll;

        public NodeInspector()
        {
            style.flexGrow = 0;
            style.minWidth = 240;
            style.backgroundColor = new Color(0.18f, 0.18f, 0.18f);
            style.paddingLeft = 4;
            style.paddingTop = 4;

            Add(new Label("Inspector") { style = { unityFontStyleAndWeight = FontStyle.Bold } });

            _scroll = new ScrollView();
            _scroll.style.flexGrow = 1;
            Add(_scroll);
        }

        public void Bind(NodeIR node)
        {
            _bound = node;
            Rebuild();
        }

        private void Rebuild()
        {
            _scroll.contentContainer.Clear();
            if (_bound == null)
            {
                _scroll.contentContainer.Add(new Label("(no selection)"));
                return;
            }

            _scroll.contentContainer.Add(new Label($"sourceType: {ShortName(_bound.SourceType)}")
            {
                style = { marginBottom = 4 }
            });
            _scroll.contentContainer.Add(new Label($"guid: {_bound.Guid}") { style = { marginBottom = 8, color = new Color(0.6f, 0.6f, 0.6f) } });

            if (_bound.Properties == null) return;
            foreach (var prop in _bound.Properties)
            {
                _scroll.contentContainer.Add(BuildPropertyRow(prop));
            }
        }

        private VisualElement BuildPropertyRow(NodePropertyIR prop)
        {
            var row = new VisualElement
            {
                style =
                {
                    flexDirection = FlexDirection.Row,
                    marginTop = 2,
                }
            };
            row.Add(new Label(prop.Key) { style = { width = 90 } });

            if (prop.Value == null || prop.Value.IsNodeRef)
            {
                row.Add(new Label(prop.Value == null ? "(null)" : $"→ {((Guid)prop.Value.Value)}")
                {
                    style = { color = new Color(0.6f, 0.8f, 1f) }
                });
                return row;
            }

            // literal 编辑器 —— 简化为 TextField；真实形态按目标字段类型做精细化（PR-8 锁）。
            var tf = new TextField { value = prop.Value.Value?.ToString() ?? "" };
            tf.style.flexGrow = 1;
            tf.RegisterValueChangedCallback(ev =>
            {
                prop.Value = PropertyValueIR.Literal(ev.newValue);
                OnEdit?.Invoke();
            });
            row.Add(tf);
            return row;
        }

        private static string ShortName(string aqn)
        {
            if (string.IsNullOrEmpty(aqn)) return "(empty)";
            int comma = aqn.IndexOf(',');
            string typeName = comma < 0 ? aqn : aqn.Substring(0, comma).Trim();
            int dot = typeName.LastIndexOf('.');
            return dot < 0 ? typeName : typeName.Substring(dot + 1);
        }
    }
}
