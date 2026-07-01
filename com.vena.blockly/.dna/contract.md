## §0 命名空间与冻结范围

- 命名空间：`Vena.Blockly`。
- 冻结：§1–§5。§6 默认冻结，本文§6 内已明示追加项除外。不冻结：§7。

## §1 双图同步性边界

- **逻辑图（ExpressionBlockly）= 同步**：`Invoke / Call<TResult>` 单调用栈走完整树并返回。`ILogicNode` 仅 `Init / Evaluate`。
- **行为图（BehaviorBlockly）= 跨帧、机制同步**：`Update(deltaTime)` 多帧驱动；单 Tick 同步返回 `BehaviorResult`。
- **两图机制均禁止** `IEnumerator / Task / async / await / yield`。
- **命名历史**：原 `LogicGraph` 已重命名为 `ExpressionBlockly`；原 `BehaviorGraph` 已重命名为 `BehaviorBlockly`（去 `Graph` 词汇歧义、统一 `Blockly` 后缀）。后文出现的一律采用新名。

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
- **值栈 = 静态共享调用约定**（KD#18）：
  - 栈实体：`Expression.Block` 类内 `[ThreadStatic]` `Stack<IBoxedValue>`；存储职责下沉到基类、不再挂在 `ExpressionBlockly.Blockly` 实例上。
  - Push/Pop 接口：`internal static void Expression.Block.Push<T>(in T)` / `internal static T Expression.Block.Pop<T>()`；派生 `Block<TSource>` 直接继承调用，无需 `Blockly.` 前缀。
  - `ExpressionBlockly.Blockly` 上**不再提供** `Push` / `Pop` 实例方法，也不再持有 `_valueStack` 字段。原实例方法已删除。
  - **异常兜底**：`ExpressionBlockly.Blockly.Invoke / Call<TResult>` 入口先 `int stackDepth = Expression.Block.CurrentStackDepth();`、`try { Push(参数); 入口节点.Evaluate(); [return Pop<TResult>();] } finally { while (Expression.Block.CurrentStackDepth() > stackDepth) Expression.Block.PopBoxed().Dispose(); }`——确保异常报错后栈回到入口时的深度，不影响外层/内层嵌套调用。`stackDepth` 快照必须先于参数 `Push` 之前拍定。
  - `Expression.Block.CurrentStackDepth()` / `PopBoxed()` 为同类内部 `internal static` 服务方法，包内可见。

## §4 IProcedureImpl / IFunctionImpl 高 arity（N=5）

**本节作废**。`IProcedureImpl` / `IFunctionImpl` 接口族（0..5 arity）与 `Procedure<TImpl,T1..Tn>` / `Function<TImpl,T1..Tn,TOutput>` 12 类基类包装**已随 Expression 值传递重构删除**（KD#18）。

接替契约（锁定）：

- **arity 展开不再由类型系统承载**——交给 codegen emitter：每个 `[Blockly]` 成员 target（method / property / field）产出一个 `*Source.Node : Block<*Source>`，其 `Evaluate()` 根据具体 arity 直接展开 Pop 序列与目标调用。
- **Pop 顺序（铁律，不变）**：从右到左（`T_N` 先 Pop、`T_1` 最后）。
- **`Block.Evaluate` 求值协议（铁律，顺序不可换，从 5 步精简为 4 步，因为 `InitializeProperties(_impl)` 的「注入到 impl」阶段已因 KD#18 删 impl 而消失）**：
  1. `EvaluateChildren()` —— 触发子节点 `Evaluate`（每个内部 `Push`）。
  2. `Pop<T_N>() ... Pop<T_1>()` —— 右→左、存为本地变量 `arg1..argN`。
  3. 目标调用：Method = `receiver.Method(arg1, ..., argN)`；Property getter = `receiver.Prop`；Property setter = `receiver.Prop = arg1`；Field 同理。
  4. `Push(result)` —— 仅非-void 返回形态。
- 子节点求值副作用必须落在 `EvaluateChildren()` 中；放进中间步骤 = 撞空栈。
- 0-arity 形态：`EvaluateChildren()` 与 Pop 段为空；其余两步顺序不变。
- **不再存在**：`_impl` 字段、`InitializeProperties(TImpl)` / `CleanProperties(TImpl)` 抽象方法、「Impl 层」概念——包心 Path A 不再区分 Impl / Node，直接在 Node 中调用目标。


## §5 叶子门控与嵌套规则

**ExpressionBlockly 是唯一的作用域边界。**

- 一个 `ExpressionBlockly` = 一套变量作用域（`IBlocklyVariableStorage` + scope chain）。控制流内部（if / while / sequence 的分支/体）**不创建子作用域、不实例化新 `ExpressionBlockly`**。
- `LogicControl` 系列（`LogicSequence` / `LogicBranch` / `LogicWhile` / `LogicSetVariable` / `LogicGetVariable`）的子槽（`statements` / `condition` / `trueBranch` / `falseBranch` / `body` / `value`）直接持有 `Expression`，**不是** `ExpressionBlockly`。
- `LogicControl.Node.Evaluate()` 直接调用子 `Expression` 节点的 `Evaluate()`，并通过静态栈 `Pop<T>()` 取得结果——**不使用** `Call<T>()` / `Invoke()`。无 `CreateBlockly` / `DestroyBlockly`。
- 控制流节点是 expression 树内的递归节点，不是图嵌套图。

**ExpressionBlockly 的实例化落点**（唯一合法位置）：
- `LogicBehavior`（行为叶子嵌图接线）：onStart / onTick / onLateTick / onFinish 四个 `ExpressionBlockly` 字段，各占一套作用域。
- `LogicClip`（timeline 叶子嵌图接线）：onBegin / onFrame / onEnd 三个 `ExpressionBlockly` 字段，同上。
- **禁止**：在 `LogicControl` 内部为控制流子槽（condition / body / branch 等）创建 `ExpressionBlockly`；禁止在 `Expression.Block<TSource>.Evaluate()` 内部 `CreateBlockly`。

**入口（`ExpressionBlockly.Blockly.Invoke / Call<TResult>`）**：
- 内部实现换为静态栈约定（§3）：`int depth = CurrentStackDepth(); try { Push(args...); _entry?.Evaluate(); [return Pop<TResult>();] } finally { while (CurrentStackDepth() > depth) PopBoxed().Dispose(); }`。

## §6 Host 聚合门面

- 入口 API 仅接收 `IBlocklyHost`。
- 能力接口冻结清单：`IBlocklyLogger` / `IBlocklyNodeFactory` / `IBlocklyPool` / `IBlocklySerializer` / `IBlocklyVariableStorage` + `IBlocklyVariableStorageFactory` / `IBlocklySource`。
- **节点身份（二层）**：
    - **runtime 端 = `ulong InstanceId`（会话内身份键）**：`IBlocklySource` 携带 `InstanceId`；`IBehaviorNode` / `ILogicNode` 实例经此键由宿主登记；`IBlocklyHost` 提供 `InstanceId` ↔ 节点双向映射查询。`InstanceId` 仅在当前 host 生命周期内唯一、跨会话不稳定（类比 Unity `InstanceID`）。具体签名 Phase 2 锁。
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
2. `BlocklySourceAttribute / BlocklySourceSlotAttribute / BlocklyAttribute / ExpressionSignatureAttribute` 元数据语义。
3. codegen 输出格式与目标产物。
4. 节点注册表数据结构。
5. 行为/逻辑节点的具体子类清单。
6. IR 序列化格式与 Editor ↔ Runtime 双向转换协议。
7. 编辑器工具。

## §6 节点实例身份

`ILogicNode` / `IBehaviorNode` 实例经 `ulong InstanceId`（由 `IBlocklySource.InstanceId` 派生）键由孿主登记。同一 `ExpressionBlockly.Blockly` / `BehaviorBlockly.Blockly` 作用域内 InstanceId 唯一。跨作用域 InstanceId 通过 scope chain `GetInstanceById<T>` 向上查找（`BlocklyInstanceAccessor.cs:34`）。InstanceId = 0 为未初始化 sentinel，注册时应抛 `InvalidOperationException`（详 KD#14）。
