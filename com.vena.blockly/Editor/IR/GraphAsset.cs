// -----------------------------------------------------------------------------
// Vena Blockly
// Visual scripting block graph runtime for the Vena open-source Unity framework.
// Copyright (c) Nan Li.
// Licensed under the terms defined in the repository LICENSE file.
// -----------------------------------------------------------------------------

using UnityEngine;

namespace Vena.Blockly.Editor
{

    /// <summary>
    /// 图资产 —— Unity 序列化载体。
    /// ScriptableObject + 单字段 [SerializeField] string _json；Unity 序列化只触发整串 _json 字段写盘 / round-trip。
    /// 不参与字段级 diff、不依赖 Unity 对象引用图。
    /// 一个 GraphAsset = 一张图（Behavior 或 Logic）；不同图独立资产、独立 _json。
    /// </summary>
    public sealed class GraphAsset : ScriptableObject
    {
        [SerializeField] private string _json;

        /// <summary>读取 IR JSON 串原文（canonical 形态）。</summary>
        public string GetJson() => _json ?? string.Empty;

        /// <summary>
        /// 写入 IR JSON 串原文。
        /// 写盘前的 canonical 化由 <see cref="IBlocklyGraphSerializer"/> 实现保证。
        /// 这里不做格式校验、不做 round-trip 验证。
        /// </summary>
        public void SetJson(string json)
        {
            _json = json ?? string.Empty;
        }
    }
}
