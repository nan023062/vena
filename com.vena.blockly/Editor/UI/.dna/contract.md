## §0 范围与冻结

- 子模块套入父模块命名空间 `Vena.Blockly.Editor.UI`（编辑器窗口与画布类型）。
- 待冻结：§1§2§3§4（Phase 2 第二刀进度锁）。不冻结：与父 §6 聚合门面的关系（明确不进 `IBlocklyHost`）。

## §1 顶层菜单

- 编辑器窗口入口：`Window/Vena/Blockly Editor` —— 单入口、打开 `BlocklyEditorWindow`。
- 资产创建：
  - `Assets/Create/Vena/Blockly/Behavior Graph` → 新建 `GraphAsset`，`kind = "Behavior"`、`_json` 初始化为最小骨架（`schema / kind / rootNodeGuid / nodes:[] / edges:[]`）。
  - `Assets/Create/Vena/Blockly/Logic Graph` → 同上、`kind = "Logic"`。
- 不提供右键菜单 / Inspector 自定义按钮入口；以上两条为唯一编辑器入口。

## §2 主窗口构成

- `BlocklyEditorWindow` 内三区：
  - **Toolbox**（左，可折叠）：节点调色板。数据源 = `INodeMetadataProvider.All()`；菜单分层 = `[UgcSource.menuPath]` 原值按 `/` 切分；拖拽 / 双击落入画布。
  - **GraphView**（中，主画布）：单画布、双 wire（`ControlWire` / `ValueWire`，§3 锁规则）。`LoadIR(GraphIR)` 入图、`DumpIR() : GraphIR` 出图，`Save()` 触发 `_json` 回写。
  - **Inspector**（右，可折叠）：当前选中 `NodeIR` 的属性面板，绑 `NodeIR.Properties` kv；编辑回写 IR、不直接持有运行期实例。
- 顶部工具条：Save / Reload / Layout（自动布局）/ Debug Toggle（启用 §4 调试通道 v0 时可视化命中）。

## §3 连线规则

- 单画布双 wire：
  - **ControlWire**（控制流）：连接 `IBehaviorNode` 输出端 → `IBehaviorNode` 输入端；表达「下一步执行」。
  - **ValueWire**（值流）：连接 `ILogicNode` 输出端 → 任意端口（`IBehaviorNode` 参数槽 / `ILogicNode` 参数槽）；表达「值求值依赖」。
- `EdgeIR.WireKind` 显式标记 `Control` 或 `Value`、**无默认值**（§4.4 锁）。混用同一 `EdgeIR` 列表、不分两组。
- 连接合法性：
  - 控制端口仅接 ControlWire；值端口仅接 ValueWire。
  - 类型校验委派 `[ExpressionSignature]` 形参 / 返回型规则（父 Editor 合约 §1）。
  - 端口入度上限：控制入度 1（树形）；值入度 1。出度无限。
  - 检测到环 / 类型不符 → UI 拒绝、不写入 `EdgeIR`。
- 拖线交互：从端口拖出 → 落空 → 弹 Toolbox 过滤同类节点（待 PR-8）。

## §4 调试通道 v0

- 接口 `IBlocklyDebugChannel`（Editor 期定义、Runtime 侧实现注入）：
  - `OnNodeEnter(Guid nodeGuid)` —— 节点开始执行。
  - `OnNodeExit(Guid nodeGuid, BehaviorResult result)` —— 节点结束、附结果。
  - `OnValueProduced(Guid nodeGuid, IBoxedValue value)` —— 表达式节点产值。
- 与 `IBlocklyLogger` 分离原则：
  - `IBlocklyLogger` = 文本流目标（`Info` / `Warn` / `Error`），变更原因 = 日志聚合 / 转发策略。
  - `IBlocklyDebugChannel` = 编辑器可视化目标（节点高亮 / 端口数值 tooltip），变更原因 = 编辑器调试 UX。
  - 两者不同变更原因、不同消费方、独立接口；不复用、不继承、不合并。
- **不进父 §6 聚合门面**：`IBlocklyHost` 不暴露 `IBlocklyDebugChannel` 字段；调试通道由 `BlocklyEditorWindow` 在编辑器期通过 host 之外的注入路径（`IBlocklyHost` 之外的扩展点，PR-9 锁定形态）挂入 Runtime 节点执行链。
- v0 范围：仅三事件、单线程、同步回调；不做时间轴 / 历史回放 / 远程调试。
