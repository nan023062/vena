## §0 命名空间与冻结范围

- 命名空间：`Vena.Blockly`。
- 冻结：§1–§5。§6 默认冻结，本文§6 内已明示追加项除外。不冻结：§7。

## §1 双图同步性边界

- **逻辑图（LogicGraph）= 同步**：`Invoke / Call<TResult>` 单调用栈走完整树并返回。`ILogicNode` 仅 `Init / Evaluate`。
- **行为图（BehaviorGraph）= 跨帧、机制同步**：`Update(deltaTime)` 多帧驱动；单 Tick 同步返回 `BehaviorResult`。
- **两图机制均禁止** `IEnumerator / Task / async / await / yield`。

## §2 ScopeChain 协议

- 父子链通过 `Blockly._parent`；变量在整链上唯一，重名抛 `InvalidOperationException`。
- 公开 API：
  - `T GetVariable<T>(string name)`
  - `void SetVariable<T>(string name, T value)`
  - `bool HasVariable(string name)`
  - `void ClearVariables()`
- 不提供 `EnterScope() / ExitScope()`。
- 作用域嵌套深度无 N 约束（N=5 仅指 §4 arity）。
- 变量后端：`IBlocklyVariableStorage` 由 `IBlocklyHost.VariableStorageFactory.Create(scope)` 提供。
- **业务方调用代码零修改**。
- **变量写语义（lexical-by-default）**：`SetVariable<T>(name, value)` 沿父链向上查找最近一层已持有该变量的 scope 并写入；全链均无该变量时，在当前 scope 创建。整链唯一性约束（重名抛 `InvalidOperationException`）保持不变——「整链唯一」与「lexical 写穿透」不冲突：lexical 写命中已有变量 = 更新而非声明，不触发重名。

## §3 值容器与值栈

- 值容器：`BoxedValue<T>` + 抽象 `IBoxedValue`。**禁止无类型 Variant**。
- 旁路值栈：`Blockly.Push<T>(T)` / `T Blockly.Pop<T>()`。

## §4 IProcedureImpl / IFunctionImpl 高 arity（N=5）

- N=5 = 参数 arity 上限。
- 0..5 参签名形态：`in T_i` 入参、`Evaluate` 方法名、协变 `TOutput`。
  - `IProcedureImpl<T1..Tn>` n=0..5
  - `IFunctionImpl<T1..Tn,TOutput>` n=0..5
- 基类包装：`Procedure<TImpl,T1..Tn>` / `Function<TImpl,T1..Tn,TOutput>`。
- **Pop 顺序**：从右到左（`T_N` 先 Pop，`T_1` 最后）。
- **Block.Evaluate 求值协议（铁律，顺序不可换）**：
  1. `EvaluateChildren()` —— 触发子节点 `Evaluate`（每个内部 Push）。
  2. `Pop<T_N>() ... Pop<T_1>()` —— §4 右→左规则。
  3. `InitializeProperties(_impl)` —— **只**做字段拷贝，**不得**触发任何 `_child.Evaluate()` 副作用。
  4. `_impl.Evaluate(...)` —— 调实现。
  5. `Blockly.Push(result)` —— 仅 `Function` 形态。
  - 子节点求值副作用必须落在 `EvaluateChildren()` 中；放进 `InitializeProperties` = 撞空栈。
  - 0-arity Function：`EvaluateChildren()` 与 Pop 段为空；其余四步顺序不变。

## §5 行为图 Tick 协议

枚举 `BehaviorResult { Running, Done }`：

- `Running` —— 未完成、下一帧再 Tick。
- `Done` —— 已完成。

`IBehaviorNode` 生命周期：

- `void Start()` —— 首次进入活动态调用一次。
- `BehaviorResult Tick(float deltaTime)` —— 每帧推进；Running 下帧继续，Done 本次活动周期结束。
- `void LateTick(float deltaTime)` —— 同帧 Tick 后延后处理。
- `void Finish()` —— 退出活动态调用一次。
- `void OnDestroy()` —— 资源回收。

入口：`BehaviorGraph.Blockly.Start / Update(float) / LateUpdate(float) / Finish / Restart`。

已知节点子类：`BranchNode / SwitchNode / SelectorNode / LoopNode / ParallelNode / SequenceNode / LogicBehavior / Timeline`。

**分支决策由 LogicGraph 条件表达式完成，Tick 返回值不参与分支。**

### 叶子时长门控

- 叶子可通过表达式求值决定自身本次活动周期的 Running 时长（自我门控）。
- 典型：`LogicBehavior.onTick: LogicGraph`（`[ExpressionSignature(typeof(bool))]`）——`true → Done`、`false → Running`。
- **叶子不得通过 Tick 返回值向父/兄弟传递分支 / 成败语义**。

## §6 Host 聚合门面

- 入口 API 仅接收 `IBlocklyHost`。
- 能力接口冻结清单：`IBlocklyLogger` / `IBlocklyNodeFactory` / `IBlocklyPool` / `IBlocklySerializer` / `IBlocklyVariableStorage` + `IBlocklyVariableStorageFactory` / `IBlocklySource`。
- **节点身份（二层）**：
    - **runtime 端 = `ulong InstanceId`（会话内身份键）**：`IBlocklySource` 携带 `InstanceId`，`IBehaviorNode` / `IProcedureImpl` 实例经此键由宿主登记；`IBlocklyHost` 提供 `InstanceId` ↔ 节点双向映射查询。`InstanceId` 仅在当前 host 生命周期内唯一、跨会话不稳定（类比 Unity `InstanceID`）。具体签名 Phase 2 锁。
    - **IR 端 = `System.Guid`（128-bit 持久身份）**：编辑器分配、入盘、跨 round-trip 永不变；runtime 加载 IR 时由 `GraphLoader` 折叠为 `ulong InstanceId` 覆盖会话内键。
    - **二者不互换**：`InstanceId` 不入盘、不跨进程；`System.Guid` 不参与 runtime 注册查询。
- **`IBlocklySerializer` 不变量**：(a) 序列化保留节点 `System.Guid`，反序列化不重新生成；(b) round-trip 语义等价。IR 格式 Phase 2 锁。
- **`IBlocklyNodeFactory` 扩展（Phase 2 解冻追加）**：
    - `void Initialize()` —— per-host 一次性反射注册入口；多次调用幂等。
    - 与之配套的 `INodeMetadataProvider` 由 Editor 子模块定义并锁，工厂内部消费（运行期接口、Editor → Runtime 单向落点）。父合约只锁「工厂多了个 `Initialize()`」与「工厂依赖 `INodeMetadataProvider` 抽象」。
- **解冻范围声明**：本节自 Phase 2 起对 `IBlocklyNodeFactory` 一项放开，仅允许「追加非破坏成员 + 追加抽象依赖」；其余冻结接口仍冻。

## §7 错误处理与非冻结声明

### 错误处理

- 作用域重名抛 `InvalidOperationException`。
- `Init` 类型不匹配抛 `ArgumentException`。
- 根作用域缺 缺 host 抛 `InvalidOperationException`。
- **异常不走 Tick 返回值**。基类（`BehaviorNode` / `CompositeBehavior`）Tick 路径 try/catch → `IBlocklyHost.Logger.Error` 记录 → 以 `Done` 收尾。调用方订阅 `IBlocklyLogger` 观察。

### 非冻结声明

1. 反射识别规则。
2. `BlocklySourceAttribute / BlocklySourcePropertyAttribute / BlocklyAttribute / ExpressionSignatureAttribute` 元数据语义。
3. codegen 输出格式与目标产物。
4. 节点注册表数据结构。
5. 行为/逻辑节点的具体子类清单。
6. IR 序列化格式与 Editor ↔ Runtime 双向转换协议。
7. 编辑器工具。
