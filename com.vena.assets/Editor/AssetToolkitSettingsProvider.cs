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
using UnityEngine.UIElements;

namespace Vena.Assets.Editor
{
    /// <summary>
    /// Project Settings host that aggregates all Asset Toolkit editor tabs
    /// (Builder / Profiler / Pipeline / VersionControl) under a single page.
    /// </summary>
    internal sealed class AssetToolkitSettingsProvider : SettingsProvider
    {
        private static AssetToolkitTab _editorTab;

        private List<AssetToolkitTab> _editorTabList;
        private bool _activated;

        [SettingsProvider]
        public static SettingsProvider GetSettingsProvider()
        {
            try
            {
                return new AssetToolkitSettingsProvider(
                    "Project/Vena Assets",
                    SettingsScope.Project);
            }
            catch (Exception ex)
            {
                Debug.LogError($"[VenaAssets] Failed to create SettingsProvider: {ex}");
                return null;
            }
        }

        private AssetToolkitSettingsProvider(string path, SettingsScope scope) : base(path, scope)
        {
            // Ctor must do no work that touches project resources — Unity constructs every
            // SettingsProvider just to populate the Project Settings sidebar list, long before
            // the user actually clicks Vena Assets. Anything that loads ScriptableObjects,
            // walks AssetDatabase, or scans disk happens in OnActivate.
            keywords = new[] { "Vena", "Assets", "AssetBundle", "OSS", "Builder", "Profiler", "Pipeline" };
        }

        public override void OnActivate(string searchContext, VisualElement rootElement)
        {
            base.OnActivate(searchContext, rootElement);

            if (_activated) return;
            _activated = true;

            try
            {
                DiscoverTabs();

                if (_editorTabList != null && _editorTabList.Count > 0)
                {
                    SwitchSubTab(_editorTabList[0]);
                }
            }
            catch (Exception ex)
            {
                Debug.LogError($"[VenaAssets] OnActivate failed: {ex}");
            }
        }

        public override void OnDeactivate()
        {
            base.OnDeactivate();

            if (null != _editorTab)
            {
                try { _editorTab.ExitDraw(); }
                catch (Exception ex) { Debug.LogWarning($"[VenaAssets] ExitDraw threw: {ex.Message}"); }
                _editorTab = null;
            }
        }

        private void DiscoverTabs()
        {
            _editorTabList = new List<AssetToolkitTab>();

            Type[] types;
            try
            {
                types = typeof(AssetToolkitSettingsProvider).Assembly.GetTypes();
            }
            catch (System.Reflection.ReflectionTypeLoadException ex)
            {
                Debug.LogWarning($"[VenaAssets] ReflectionTypeLoadException while enumerating tabs: {ex.Message}");
                types = ex.Types.Where(t => t != null).ToArray();
            }

            foreach (var type in types.Where(t => t != null && t.IsSubclassOf(typeof(AssetToolkitTab)) && !t.IsAbstract))
            {
                try
                {
                    _editorTabList.Add(ScriptableObject.CreateInstance(type) as AssetToolkitTab);
                }
                catch (Exception ex)
                {
                    Debug.LogWarning($"[VenaAssets] Failed to instantiate tab '{type.FullName}': {ex.Message}");
                }
            }
        }

        public override void OnGUI(string searchContext)
        {
            base.OnGUI(searchContext);

            // Defensive: OnGUI may fire before OnActivate in degenerate cases; lazy-init then.
            if (!_activated)
            {
                try
                {
                    DiscoverTabs();
                    if (_editorTabList != null && _editorTabList.Count > 0)
                        SwitchSubTab(_editorTabList[0]);
                }
                catch (Exception ex)
                {
                    Debug.LogError($"[VenaAssets] Lazy init from OnGUI failed: {ex}");
                }
                _activated = true;
            }

            if (!AssetToolkitProvider.HasCustomFactory)
            {
                EditorGUILayout.HelpBox(
                    "Production OSS not configured — falling back to InMemoryOssClient. " +
                    "Call AssetToolkitProvider.SetOssClientFactory(...) from an [InitializeOnLoad] " +
                    "handler in your project to wire a real backend.",
                    MessageType.Warning);
            }

            if (AssetBuildSetting.Instance == null ||
                string.IsNullOrEmpty(AssetBuildSetting.Instance.assetRootPath))
            {
                EditorGUILayout.HelpBox(
                    "AssetBuildSetting is not configured. Create one via 'Assets > Create > AssetBuildSetting' " +
                    "and set 'Raw Asset Path' on the Asset Builder tab before building bundles.",
                    MessageType.Info);
            }

            if (_editorTabList == null || _editorTabList.Count == 0)
            {
                EditorGUILayout.HelpBox(
                    "No AssetToolkitTab implementations were discovered in this assembly. " +
                    "Verify Vena.Assets.Editor compiled successfully (check the Console for errors).",
                    MessageType.Info);
                return;
            }

            EditorGUILayout.BeginHorizontal();

            foreach (var editorTab in _editorTabList)
            {
                if (editorTab == null) continue;

                if (GUILayout.Toggle(editorTab == _editorTab, editorTab.TabName, EditorStyles.toolbarButton))
                {
                    SwitchSubTab(editorTab);
                }
            }

            EditorGUILayout.EndHorizontal();

            if (null != _editorTab)
            {
                try
                {
                    _editorTab.DrawGUI();
                }
                catch (Exception ex)
                {
                    EditorGUILayout.HelpBox($"Tab '{_editorTab.TabName}' threw: {ex.Message}", MessageType.Error);
                    Debug.LogException(ex);
                }
            }
        }

        private void SwitchSubTab(AssetToolkitTab editorTab)
        {
            if (editorTab != _editorTab)
            {
                if (null != _editorTab)
                {
                    try { _editorTab.ExitDraw(); }
                    catch (Exception ex) { Debug.LogWarning($"[VenaAssets] ExitDraw threw: {ex.Message}"); }
                }

                _editorTab = editorTab;

                if (null != _editorTab)
                {
                    try { _editorTab.EnterDraw(); }
                    catch (Exception ex)
                    {
                        Debug.LogError($"[VenaAssets] EnterDraw on '{_editorTab.TabName}' threw: {ex}");
                    }
                }
            }
        }
    }
}
