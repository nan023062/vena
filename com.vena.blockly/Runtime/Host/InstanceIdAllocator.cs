// -----------------------------------------------------------------------------
// Vena Blockly
// Visual scripting block graph runtime for the Vena open-source Unity framework.
// Copyright (c) Nan Li.
// Licensed under the terms defined in the repository LICENSE file.
// -----------------------------------------------------------------------------

using System.Threading;

namespace Vena.Blockly
{
    /// <summary>
    /// 运行期身份键分配器（语义类似 Unity <c>Object.GetInstanceID()</c>）：进程内
    /// 单调递增 <c>ulong</c>，仅保证单次会话内唯一，不跨进程稳定，不持久化。
    ///
    /// 用途：当 source 不经 IR 加载路径（demo 代码直接 <c>new Expression()</c> /
    /// <c>new BehaviorNodeSource()</c>）时，给 <see cref="IBlocklySource.InstanceId"/>
    /// 字段提供 fallback 占位值，避免多个 plain 实例共享 <c>InstanceId=0</c> 时
    /// 在 GraphLoader 字典上撞键。
    ///
    /// **不是持久 guid**：持久身份在 IR 端由 128-bit <c>System.Guid</c> 表示，
    /// 编辑器创建节点时分配、永久不变；走 IR 加载路径时
    /// <c>GraphLoader.TrySetInstanceId</c> 会用 IR 持久 guid 折叠值通过反射覆盖此处
    /// 分配的占位值。SO 路径走 <c>SoExpression.OnEnable</c> 派生，亦不经过此分配器。
    /// </summary>
    internal static class InstanceIdAllocator
    {
        private static long _nextId;

        public static ulong Next()
        {
            return unchecked((ulong)Interlocked.Increment(ref _nextId));
        }
    }
}
