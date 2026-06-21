// -----------------------------------------------------------------------------
// Vena Blockly
// Visual scripting block graph runtime for the Vena open-source Unity framework.
// Copyright (c) Nan Li.
// Licensed under the terms defined in the repository LICENSE file.
// -----------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;

namespace Vena.Blockly.Editor.UI
{

    /// <summary>
    /// GraphView Node 视图 —— 单节点的端口 + 标签呈现。
    /// 端口规则：
    ///   控制端口（如 "next" / "true" / "false"）：Color/Capacity = single（入度 1）。
    ///   值端口（[UgcSourceProperty] 槽位）：Capacity = single（值入度 1）。
    /// </summary>
    public sealed class NodeView : Node
    {
        public NodeIR NodeIR { get; }

        private readonly Dictionary<string, Port> _inputPorts = new Dictionary<string, Port>();
        private readonly Dictionary<string, Port> _outputPorts = new Dictionary<string, Port>();
        private Label _previewLabel;
        private VisualElement _highlightOverlay;

        public NodeView(NodeIR nodeIR)
        {
            NodeIR = nodeIR;
            title = ShortName(nodeIR.SourceType);

            // 默认端口：控制流入/出 + 每个属性槽位一个值入端口。
            // 后续可由 EdgeConnector / metadata 驱动更精细的端口集。
            var controlIn = InstantiatePort(Orientation.Horizontal, Direction.Input, Port.Capacity.Single, typeof(ControlFlow));
            controlIn.portName = "in";
            controlIn.userData = WireKind.Control;
            inputContainer.Add(controlIn);
            _inputPorts["in"] = controlIn;

            var controlOut = InstantiatePort(Orientation.Horizontal, Direction.Output, Port.Capacity.Multi, typeof(ControlFlow));
            controlOut.portName = "next";
            controlOut.userData = WireKind.Control;
            outputContainer.Add(controlOut);
            _outputPorts["next"] = controlOut;

            // 值端口：每个 NodeIR.Properties 一条值入端口（按 [UgcSourceProperty] 升序，与 IR 顺序一致）。
            if (nodeIR.Properties != null)
            {
                foreach (var p in nodeIR.Properties)
                {
                    var valueIn = InstantiatePort(Orientation.Horizontal, Direction.Input, Port.Capacity.Single, typeof(ValueFlow));
                    valueIn.portName = p.Key;
                    valueIn.userData = WireKind.Value;
                    inputContainer.Add(valueIn);
                    _inputPorts[p.Key] = valueIn;
                }
            }

            // 值出端口（用于 LogicGraph 节点）—— 默认提供单 "out"；后续可按 ExpressionSignature 锁返回型。
            var valueOut = InstantiatePort(Orientation.Horizontal, Direction.Output, Port.Capacity.Multi, typeof(ValueFlow));
            valueOut.portName = "out";
            valueOut.userData = WireKind.Value;
            outputContainer.Add(valueOut);
            _outputPorts["out"] = valueOut;

            _previewLabel = new Label("");
            _previewLabel.style.color = new Color(0.7f, 0.9f, 1f);
            _previewLabel.style.unityTextAlign = TextAnchor.MiddleCenter;
            extensionContainer.Add(_previewLabel);

            _highlightOverlay = new VisualElement
            {
                style =
                {
                    position = Position.Absolute,
                    left = 0, right = 0, top = 0, bottom = 0,
                    backgroundColor = new Color(1f, 1f, 0f, 0.15f),
                    display = DisplayStyle.None,
                },
                pickingMode = PickingMode.Ignore,
            };
            mainContainer.Add(_highlightOverlay);

            RefreshExpandedState();
            RefreshPorts();
        }

        public Port GetInputPort(string name, WireKind kind)
        {
            return _inputPorts.TryGetValue(name, out var p) ? p : null;
        }

        public Port GetOutputPort(string name, WireKind kind)
        {
            return _outputPorts.TryGetValue(name, out var p) ? p : null;
        }

        public void Refresh()
        {
            title = ShortName(NodeIR.SourceType);
        }

        public void SetHighlight(bool on)
        {
            _highlightOverlay.style.display = on ? DisplayStyle.Flex : DisplayStyle.None;
        }

        public void SetPreview(string text)
        {
            _previewLabel.text = text ?? string.Empty;
        }

        private static string ShortName(string aqn)
        {
            if (string.IsNullOrEmpty(aqn)) return "(empty)";
            int comma = aqn.IndexOf(',');
            string typeName = comma < 0 ? aqn : aqn.Substring(0, comma).Trim();
            int dot = typeName.LastIndexOf('.');
            return dot < 0 ? typeName : typeName.Substring(dot + 1);
        }

        // marker types for port-type compatibility
        public sealed class ControlFlow { }
        public sealed class ValueFlow { }
    }
}
