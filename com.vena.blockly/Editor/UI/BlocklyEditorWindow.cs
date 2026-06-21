using System;
using UnityEditor;
using UnityEditor.Callbacks;
using UnityEngine;
using UnityEngine.UIElements;

namespace Vena.Blockly.Editor.UI
{

    /// <summary>
    /// Blockly 编辑器主窗口。
    ///
    /// 三区布局：
    ///   左：Toolbox（折叠）—— 节点调色板，数据源 = INodeMetadataProvider.All()。
    ///   中：BlocklyGraphView —— 单画布双 wire（Control / Value）。
    ///   右：Inspector（折叠）—— 当前选中 NodeIR 的属性面板。
    /// 顶部工具条：Save / Reload / Layout / Debug Toggle。
    ///
    /// 单入口菜单：Window/Vena/Blockly Editor。
    /// 双击 GraphAsset → 通过 OnOpenAsset 进入。
    /// </summary>
    public sealed class BlocklyEditorWindow : EditorWindow
    {
        private const string MenuPath = "Window/Vena/Blockly Editor";

        private GraphAsset _asset;
        private GraphIR _ir;
        private BlocklyGraphView _graphView;
        private BlocklyToolbox _toolbox;
        private NodeInspector _inspector;
        private RunButton _runButton;
        private DebugToggle _debugToggle;
        private bool _dirty;

        [MenuItem(MenuPath)]
        public static void OpenWindow()
        {
            var w = GetWindow<BlocklyEditorWindow>();
            w.titleContent = new GUIContent("Blockly Editor");
            w.minSize = new Vector2(800, 480);
            w.Show();
        }

        public static void Open(GraphAsset asset)
        {
            var w = GetWindow<BlocklyEditorWindow>();
            w.titleContent = new GUIContent("Blockly Editor");
            w.LoadAsset(asset);
            w.Show();
        }

        [OnOpenAsset(0)]
        public static bool HandleOpenAsset(int instanceID, int line)
        {
            var obj = EditorUtility.InstanceIDToObject(instanceID) as GraphAsset;
            if (obj == null) return false;
            Open(obj);
            return true;
        }

        private void OnEnable()
        {
            BuildLayout();
            if (_asset != null) LoadAsset(_asset);
        }

        private void BuildLayout()
        {
            rootVisualElement.Clear();

            var toolbar = new IMGUIContainer(DrawToolbar) { style = { height = 24 } };
            rootVisualElement.Add(toolbar);

            var split = new TwoPaneSplitView(0, 220, TwoPaneSplitViewOrientation.Horizontal);
            rootVisualElement.Add(split);

            _toolbox = new BlocklyToolbox();
            _toolbox.OnPick += OnToolboxPick;
            split.Add(_toolbox);

            var center = new TwoPaneSplitView(1, 280, TwoPaneSplitViewOrientation.Horizontal);
            split.Add(center);

            _graphView = new BlocklyGraphView();
            _graphView.OnSelectionChanged += OnGraphSelectionChanged;
            _graphView.OnGraphChanged += () => _dirty = true;
            _graphView.style.flexGrow = 1;
            center.Add(_graphView);

            _inspector = new NodeInspector();
            _inspector.OnEdit += OnInspectorEdit;
            center.Add(_inspector);

            _runButton = new RunButton();
            _debugToggle = new DebugToggle();
        }

        private void DrawToolbar()
        {
            using (new EditorGUILayout.HorizontalScope(EditorStyles.toolbar))
            {
                if (GUILayout.Button("Save", EditorStyles.toolbarButton, GUILayout.Width(60))) Save();
                if (GUILayout.Button("Reload", EditorStyles.toolbarButton, GUILayout.Width(60))) Reload();
                if (GUILayout.Button("Layout", EditorStyles.toolbarButton, GUILayout.Width(60))) AutoLayout();
                GUILayout.FlexibleSpace();
                _debugToggle?.Draw();
                _runButton?.Draw(this);
                GUILayout.Label(_asset != null ? _asset.name + (_dirty ? "*" : "") : "(no asset)",
                    EditorStyles.toolbarButton);
            }
        }

        public void LoadAsset(GraphAsset asset)
        {
            _asset = asset;
            if (asset == null) { _ir = null; return; }
            try
            {
                var json = asset.GetJson();
                _ir = string.IsNullOrEmpty(json) ? new GraphIR() : new JsonGraphSerializer().FromJson(json);
            }
            catch (BlocklyIRSchemaException ex)
            {
                Debug.LogError($"[Blockly] Failed to load {asset.name}: {ex.Message}");
                _ir = new GraphIR();
            }
            _graphView?.LoadIR(_ir);
            _toolbox?.Refresh();
            _dirty = false;
        }

        public void Save()
        {
            if (_asset == null || _ir == null) return;
            _ir = _graphView.DumpIR();
            _asset.SetJson(new JsonGraphSerializer().ToJson(_ir));
            EditorUtility.SetDirty(_asset);
            AssetDatabase.SaveAssets();
            _dirty = false;
        }

        public void Reload()
        {
            if (_asset != null) LoadAsset(_asset);
        }

        public void AutoLayout()
        {
            _graphView?.AutoLayout();
            _dirty = true;
        }

        public GraphAsset CurrentAsset => _asset;
        public GraphIR CurrentIR => _ir;

        private void OnToolboxPick(NodeMetadata meta)
        {
            _graphView?.AddNodeFromMetadata(meta, new Vector2(40, 40));
            _dirty = true;
        }

        private void OnGraphSelectionChanged(NodeIR selected)
        {
            _inspector?.Bind(selected);
        }

        private void OnInspectorEdit()
        {
            _dirty = true;
            _graphView?.RefreshSelected();
        }
    }
}
