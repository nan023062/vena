using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

namespace Vena.Blockly.Editor.UI
{

    /// <summary>
    /// 节点调色板（Editor/UI 合约 §2 / Key Decision 4）。
    ///
    /// 数据源 = INodeMetadataProvider.All()（codegen 生成、Runtime 实现）。
    /// 菜单分层：[UgcSource.menuPath] 原值按 `/` 切分；UI 端自行树形展开。
    /// 当前 PR-7：双击触发 OnPick。PR-8 接拖拽落画布。
    /// </summary>
    public sealed class BlocklyToolbox : VisualElement
    {
        public event Action<NodeMetadata> OnPick;

        private readonly ScrollView _scroll;
        private INodeMetadataProvider _provider;

        public BlocklyToolbox()
        {
            style.flexGrow = 0;
            style.minWidth = 200;
            style.backgroundColor = new Color(0.16f, 0.16f, 0.16f);
            style.paddingLeft = 4;
            style.paddingTop = 4;

            var header = new Label("Toolbox") { style = { unityFontStyleAndWeight = FontStyle.Bold } };
            Add(header);

            _scroll = new ScrollView();
            _scroll.style.flexGrow = 1;
            Add(_scroll);
        }

        /// <summary>注入 INodeMetadataProvider；不传则尝试反射查找已加载的 provider 实现。</summary>
        public void SetProvider(INodeMetadataProvider provider)
        {
            _provider = provider;
            Refresh();
        }

        public void Refresh()
        {
            _scroll.contentContainer.Clear();
            if (_provider == null)
            {
                _scroll.contentContainer.Add(new Label("(no INodeMetadataProvider)"));
                return;
            }
            var grouped = GroupByMenu(_provider.All());
            BuildTree(_scroll.contentContainer, grouped, depth: 0);
        }

        // ---------------------------------------------------------------- internals

        private static MenuGroup GroupByMenu(IReadOnlyList<NodeMetadata> all)
        {
            var root = new MenuGroup { Name = "" };
            if (all == null) return root;
            foreach (var meta in all)
            {
                var path = (meta.MenuPath ?? string.Empty);
                var parts = path.Split('/');
                var cur = root;
                for (int i = 0; i < parts.Length - 1; i++)
                {
                    var seg = parts[i];
                    if (string.IsNullOrEmpty(seg)) continue;
                    if (!cur.Children.TryGetValue(seg, out var child))
                    {
                        child = new MenuGroup { Name = seg };
                        cur.Children[seg] = child;
                    }
                    cur = child;
                }
                cur.Items.Add(meta);
            }
            return root;
        }

        private void BuildTree(VisualElement parent, MenuGroup group, int depth)
        {
            foreach (var kv in group.Children)
            {
                var lbl = new Label(kv.Key)
                {
                    style =
                    {
                        unityFontStyleAndWeight = FontStyle.Bold,
                        marginLeft = depth * 8,
                        marginTop = 4,
                    }
                };
                parent.Add(lbl);
                BuildTree(parent, kv.Value, depth + 1);
            }
            foreach (var meta in group.Items)
            {
                var leaf = meta.Properties != null && meta.Properties.Count >= 0 ? meta : meta;
                var btn = new Button(() => OnPick?.Invoke(leaf))
                {
                    text = LeafLabel(leaf),
                    style = { marginLeft = depth * 8 + 4, marginTop = 1 }
                };
                parent.Add(btn);
            }
        }

        private static string LeafLabel(NodeMetadata meta)
        {
            var path = meta.MenuPath ?? string.Empty;
            int slash = path.LastIndexOf('/');
            return slash < 0 ? (string.IsNullOrEmpty(path) ? meta.SourceType?.Name ?? "(unnamed)" : path)
                             : path.Substring(slash + 1);
        }

        private sealed class MenuGroup
        {
            public string Name;
            public readonly Dictionary<string, MenuGroup> Children = new Dictionary<string, MenuGroup>(StringComparer.Ordinal);
            public readonly List<NodeMetadata> Items = new List<NodeMetadata>();
        }
    }
}
