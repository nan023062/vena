// -----------------------------------------------------------------------------
// Vena Assets
// Unity asset pipeline toolkit (AssetBundle / VersionControl / OSS / Compression).
// Copyright (c) Nan Li.
// Licensed under the terms defined in the repository LICENSE file.
// -----------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace Vena.Assets.Editor
{
    /// <summary>
    /// Project Settings host that aggregates all Asset Toolkit editor tabs
    /// (Builder / Profiler / Pipeline / VersionControl) under a single page.
    /// </summary>
    internal sealed class AssetToolkitSettingsProvider : SettingsProvider
    {
        private static AssetToolkitTab _editorTab;

        private readonly List<AssetToolkitTab> _editorTabList;

        [SettingsProvider]
        private static SettingsProvider GetSettingsProvider()
        {
            return new AssetToolkitSettingsProvider(
                $"Vena/Asset Toolkit-[{Utility.GetPlatformName()}]",
                SettingsScope.Project);
        }

        private AssetToolkitSettingsProvider(string path, SettingsScope scope) : base(path, scope)
        {
            Type[] types = typeof(AssetToolkitSettingsProvider).Assembly.GetTypes();

            _editorTabList = new List<AssetToolkitTab>();

            foreach (var type in types.Where(t => t.IsSubclassOf(typeof(AssetToolkitTab))))
            {
                _editorTabList.Add(ScriptableObject.CreateInstance(type) as AssetToolkitTab);
            }

            if (_editorTabList.Count > 0)
            {
                SwitchSubTab(_editorTabList[0]);
            }
        }

        public override void OnGUI(string searchContext)
        {
            base.OnGUI(searchContext);

            if (!AssetToolkitProvider.HasCustomFactory)
            {
                EditorGUILayout.HelpBox(
                    "Production OSS not configured — falling back to InMemoryOssClient. " +
                    "Call AssetToolkitProvider.SetOssClientFactory(...) from an [InitializeOnLoad] " +
                    "handler in your project to wire a real backend.",
                    MessageType.Warning);
            }

            EditorGUILayout.BeginHorizontal();

            foreach (var editorTab in _editorTabList)
            {
                if (GUILayout.Toggle(editorTab == _editorTab, editorTab.TabName, EditorStyles.toolbarButton))
                {
                    SwitchSubTab(editorTab);
                }
            }

            EditorGUILayout.EndHorizontal();

            if (null != _editorTab)
            {
                _editorTab.DrawGUI();
            }
        }

        private void SwitchSubTab(AssetToolkitTab editorTab)
        {
            if (editorTab != _editorTab)
            {
                if (null != _editorTab) _editorTab.ExitDraw();

                _editorTab = editorTab;

                if (null != _editorTab) _editorTab.EnterDraw();
            }
        }
    }
}
