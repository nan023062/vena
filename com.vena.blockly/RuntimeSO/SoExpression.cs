// -----------------------------------------------------------------------------
// Vena Blockly
// Visual scripting block graph runtime for the Vena open-source Unity framework.
// Copyright (c) Nan Li.
// Licensed under the terms defined in the repository LICENSE file.
// -----------------------------------------------------------------------------

using UnityEngine;

namespace Vena.Blockly.SO
{
    /// <summary>
    /// SO 路径的 Expression 抽象基类。承担 IBlocklySource 接口实现、
    /// Editor 期 .asset 资产 + Inspector 编辑体验。
    ///
    /// 仅供 Editor 期开发者编辑配置使用；runtime UGC 玩家场景走 plain class 路径
    /// (Vena.Blockly.Expression)，因为玩家 game build 无 AssetDatabase 不能造 SO。
    ///
    /// InstanceId 派生：OnEnable 时由 Hash64(AssetGuid + fileID) 计算，确定性、跨会话稳定。
    /// </summary>
    public abstract class SoExpression : ScriptableObject, IBlocklySource
    {
        [SerializeField, HideInInspector] private ulong _instanceId;
        public ulong InstanceId => _instanceId;

        protected virtual void OnEnable()
        {
        }
    }
}
