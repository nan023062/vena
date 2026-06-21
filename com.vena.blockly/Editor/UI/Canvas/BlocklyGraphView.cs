using System;
using System.Collections.Generic;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;

namespace Vena.Blockly.Editor.UI
{

    /// <summary>
    /// Blockly 画布 —— 单 GraphView 双 wire（Editor/UI 合约 §2 / §3）。
    ///
    /// LoadIR(GraphIR) → 绘出节点+连线；DumpIR() : GraphIR → 反射出当前画布形态。
    /// 双 wire：
    ///   ControlWire（实色粗）— 控制流（IBehaviorNode 之间，§3）。
    ///   ValueWire  （虚线细）— 值流（ILogicNode → 任意端口，§3）。
    /// 入度上限：控制 1 / 值 1，出度无限；环 / 类型不符 → UI 拒绝（PR-8 走 EdgeConnector 校验）。
    /// </summary>
    public sealed class BlocklyGraphView : GraphView
    {
        private GraphIR _currentIR;
        private readonly Dictionary<Guid, NodeView> _nodeViews = new Dictionary<Guid, NodeView>();

        public event Action<NodeIR> OnSelectionChanged;
        public event Action OnGraphChanged;

        public BlocklyGraphView()
        {
            style.flexGrow = 1;
            this.AddManipulator(new ContentZoomer());
            this.AddManipulator(new ContentDragger());
            this.AddManipulator(new SelectionDragger());
            this.AddManipulator(new RectangleSelector());

            var grid = new GridBackground();
            Insert(0, grid);
            grid.StretchToParentSize();

            graphViewChanged = OnGraphViewChanged;
        }

        public override List<Port> GetCompatiblePorts(Port startPort, NodeAdapter nodeAdapter)
        {
            var compat = new List<Port>();
            ports.ForEach(p =>
            {
                if (p == startPort) return;
                if (p.node == startPort.node) return;
                if (p.direction == startPort.direction) return;
                if (!EdgeConnector.IsCompatible(startPort, p)) return;
                compat.Add(p);
            });
            return compat;
        }

        public void LoadIR(GraphIR ir)
        {
            _currentIR = ir ?? new GraphIR();
            DeleteElements(graphElements.ToList());
            _nodeViews.Clear();

            if (_currentIR.Nodes != null)
            {
                foreach (var n in _currentIR.Nodes)
                {
                    var view = new NodeView(n);
                    AddElement(view);
                    view.SetPosition(new Rect(n.Position.X, n.Position.Y, 200, 100));
                    _nodeViews[n.Guid] = view;
                }
            }

            if (_currentIR.Edges != null)
            {
                foreach (var e in _currentIR.Edges)
                {
                    if (!_nodeViews.TryGetValue(e.From.NodeGuid, out var src)) continue;
                    if (!_nodeViews.TryGetValue(e.To.NodeGuid, out var dst)) continue;
                    var srcPort = src.GetOutputPort(e.From.Port, e.WireKind);
                    var dstPort = dst.GetInputPort(e.To.Port, e.WireKind);
                    if (srcPort == null || dstPort == null) continue;
                    var edge = srcPort.ConnectTo(dstPort);
                    AddElement(edge);
                    EdgeStyle.Apply(edge, e.WireKind);
                }
            }
        }

        public GraphIR DumpIR()
        {
            var ir = _currentIR ?? new GraphIR();
            ir.Nodes = new List<NodeIR>();
            ir.Edges = new List<EdgeIR>();
            foreach (var nv in _nodeViews.Values)
            {
                var pos = nv.GetPosition();
                nv.NodeIR.Position = new Vec2(pos.x, pos.y);
                ir.Nodes.Add(nv.NodeIR);
            }
            edges.ForEach(edge =>
            {
                if (!(edge.output?.node is NodeView fromNV) || !(edge.input?.node is NodeView toNV)) return;
                var wireKind = (WireKind)(edge.userData ?? WireKind.Control);
                ir.Edges.Add(new EdgeIR
                {
                    From = new PortRef(fromNV.NodeIR.Guid, edge.output.portName),
                    To   = new PortRef(toNV.NodeIR.Guid, edge.input.portName),
                    WireKind = wireKind,
                });
            });
            return ir;
        }

        public void AddNodeFromMetadata(NodeMetadata meta, Vector2 position)
        {
            if (meta == null) return;
            var nodeIR = new NodeIR
            {
                Guid = Guid.NewGuid(),
                SourceType = AqnOf(meta.SourceType),
                Position = new Vec2(position.x, position.y),
            };
            nodeIR.Properties = new List<NodePropertyIR>();
            foreach (var p in meta.Properties)
            {
                nodeIR.Properties.Add(new NodePropertyIR(p.FieldName, PropertyValueIR.Literal(DefaultLiteral(p.FieldType))));
            }
            var view = new NodeView(nodeIR);
            AddElement(view);
            view.SetPosition(new Rect(position.x, position.y, 200, 100));
            _nodeViews[nodeIR.Guid] = view;
            OnGraphChanged?.Invoke();
        }

        public void RefreshSelected()
        {
            foreach (var nv in _nodeViews.Values) nv.Refresh();
        }

        public void AutoLayout()
        {
            float x = 0, y = 0;
            foreach (var nv in _nodeViews.Values)
            {
                nv.SetPosition(new Rect(x, y, 200, 100));
                x += 240;
                if (x > 1200) { x = 0; y += 140; }
            }
        }

        public void HighlightNode(Guid guid, bool on)
        {
            if (_nodeViews.TryGetValue(guid, out var nv)) nv.SetHighlight(on);
        }

        public void ShowValuePreview(Guid guid, string text)
        {
            if (_nodeViews.TryGetValue(guid, out var nv)) nv.SetPreview(text);
        }

        // ---------------------------------------------------------------- helpers

        private GraphViewChange OnGraphViewChanged(GraphViewChange change)
        {
            if (change.elementsToRemove != null && change.elementsToRemove.Count > 0)
                OnGraphChanged?.Invoke();
            if (change.edgesToCreate != null)
            {
                foreach (var e in change.edgesToCreate)
                {
                    EdgeStyle.Apply(e, EdgeConnector.InferWireKind(e.output, e.input));
                }
                OnGraphChanged?.Invoke();
            }
            if (change.movedElements != null && change.movedElements.Count > 0)
                OnGraphChanged?.Invoke();
            return change;
        }

        public override void AddToSelection(ISelectable selectable)
        {
            base.AddToSelection(selectable);
            if (selectable is NodeView nv) OnSelectionChanged?.Invoke(nv.NodeIR);
        }

        public override void RemoveFromSelection(ISelectable selectable)
        {
            base.RemoveFromSelection(selectable);
            OnSelectionChanged?.Invoke(null);
        }

        public override void ClearSelection()
        {
            base.ClearSelection();
            OnSelectionChanged?.Invoke(null);
        }

        private static string AqnOf(Type t)
            => t == null ? string.Empty : $"{t.FullName}, {t.Assembly.GetName().Name}";

        private static object DefaultLiteral(Type t)
        {
            if (t == typeof(string)) return string.Empty;
            if (t == typeof(bool)) return false;
            if (t == typeof(int) || t == typeof(long) || t == typeof(short) || t == typeof(byte)) return 0;
            if (t == typeof(float) || t == typeof(double)) return 0.0f;
            return null;
        }
    }

    /// <summary>EdgeStyle —— 控制 wire 实色粗 / 值 wire 虚线细（Editor/UI 合约 §3）。</summary>
    internal static class EdgeStyle
    {
        public static void Apply(Edge edge, WireKind kind)
        {
            edge.userData = kind;
            switch (kind)
            {
                case WireKind.Control:
                    edge.edgeControl.inputColor = new Color(0.9f, 0.9f, 0.9f);
                    edge.edgeControl.outputColor = new Color(0.9f, 0.9f, 0.9f);
                    edge.edgeControl.edgeWidth = 4;
                    break;
                case WireKind.Value:
                    edge.edgeControl.inputColor = new Color(0.6f, 0.8f, 1f);
                    edge.edgeControl.outputColor = new Color(0.6f, 0.8f, 1f);
                    edge.edgeControl.edgeWidth = 2;
                    break;
            }
        }
    }
}
