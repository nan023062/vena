// -----------------------------------------------------------------------------
// Vena Blockly
// Visual scripting block graph runtime for the Vena open-source Unity framework.
// Copyright (c) Nan Li.
// Licensed under the terms defined in the repository LICENSE file.
// -----------------------------------------------------------------------------

using System;
using UnityEditor;
using UnityEngine;

namespace Vena.Blockly.Editor.UI
{

    /// <summary>
    /// 工具栏「运行」按钮。
    ///
    /// 行为：
    ///   - 根据当前 GraphIR.Kind 加载并 Tick 一帧（Behavior）或 Invoke 一次（Logic）。
    ///   - 注入 EditorDebugChannel —— 节点高亮 + 值预览 stub。
    ///   - 仅 Editor 模式驱动；不接 PlayMode。
    ///
    /// host 由调用方通过 EditorRunHostRegistry 提供；不进 IBlocklyHost 聚合门面。
    /// </summary>
    public sealed class RunButton
    {
        public void Draw(BlocklyEditorWindow owner)
        {
            if (GUILayout.Button("Run", EditorStyles.toolbarButton, GUILayout.Width(60)))
            {
                Run(owner);
            }
        }

        private static void Run(BlocklyEditorWindow owner)
        {
            var ir = owner.CurrentIR;
            if (ir == null) { Debug.LogWarning("[Blockly] No graph loaded."); return; }

            var host = EditorRunHostRegistry.Resolve();
            if (host == null)
            {
                Debug.LogWarning("[Blockly] No IBlocklyHost registered for editor run. Set EditorRunHostRegistry.Resolver.");
                return;
            }

            // 注入 debug channel：节点高亮 + 值预览 stub。
            BlocklyDebugChannelRegistry.Current = new EditorDebugChannel(owner);
            try
            {
                var loader = new GraphLoader();
                if (ir.Kind == GraphKind.Behavior)
                {
                    var graph = loader.LoadBehavior(ir);
                    var blockly = new BehaviorGraph.Blockly();
                    blockly.SetSource(graph);
                    blockly.Start(subject: owner, host: host);
                    blockly.Update(deltaTime: 0.016f);
                    blockly.Finish();
                    blockly.Destroy();
                }
                else
                {
                    var graph = loader.LoadLogic(ir);
                    var blockly = new LogicGraph.Blockly();
                    blockly.Set(subject: owner, host: host);
                    blockly.SetSource(graph);
                    blockly.Invoke();
                    blockly.Destroy();
                }
            }
            catch (Exception e)
            {
                Debug.LogError($"[Blockly] Run error: {e.Message}\n{e.StackTrace}");
            }
            finally
            {
                BlocklyDebugChannelRegistry.Current = null;
            }
        }
    }

    /// <summary>
    /// Editor 端注入 host 入口 —— 业务工程注册 resolver、编辑器运行按钮按需取用。
    /// 不进 IBlocklyHost 聚合门面，避免 Editor 工程绑死 Runtime 形状。
    /// </summary>
    public static class EditorRunHostRegistry
    {
        public static Func<IBlocklyHost> Resolver;
        public static IBlocklyHost Resolve() => Resolver?.Invoke();
    }
}
