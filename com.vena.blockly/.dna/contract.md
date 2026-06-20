## §0 命名空间与冻结范围

- 命名空间：`Vena.Blockly`（Runtime 主源已是此命名空间）。
- **第一阶段不改命名空间、不搬迁物理目录**（按 module.md Key Decision 10 ratchet）。
- **冻结对象**：以下 §1–§6 的接口签名、生命周期、值栈约定、调用方零修改承诺。
- **不冻结对象**：见 §7 列表。

## §1 双图同步性边界

- **逻辑图（Expression / LogicGraph）= 同步语义**：`Invoke / Call<TResult>` 单调用栈走完整树并返回。`ILogicNode` 仅 `Init / Evaluate` 两点。**禁止** `IEnumerator / Task / async / await / yield`。
- **行为图（Behavior / BehaviorGraph）= 跨帧语义、机制仍同步**：由 `Update(deltaTime)` 多帧驱动；单 Tick 同步返回 `BehaviorResult` 双态枚举（Running / Done），整图完成跨任意帧。**机制层面同样禁止**上述异步原语——「异步」指**跨帧推进**而非协程/任务。

**分支决策由 LogicGraph 条件表达式完成，节点返回值不参与分支**。本模块是 imperative 流图（参考 Blueprint Exec / Bolt Flow / Scratch 范式），不是 BT。Branch / Switch / Selector 等节点通过求值表达式条件决定走哪个分支，不通过子节点返回值。

## §2 ScopeChain 协议

- 父子链通过 `Blockly._parent`；变量在整链上唯一，重名抛 `InvalidOperationException`（沿用现状）。
- 公开 API 保留现有强类型签名：
  - `T GetVariable<T>(string name)`
  - `void SetVariable<T>(string name, T value)`
  - `bool HasVariable(string name)`
  - `void ClearVariables()`
- **第一阶段抽出 ScopeChain 独立类**——内部重构，**对外签名一行不改**。
- **不新增** `EnterScope() / ExitScope()` API（拍板：保持现状不加）。
- 第一阶段对作用域**嵌套深度不设 N 约束**（N=5 仅指 §4 参数 arity）。
- 变量后端：`IBlocklyVariableStorage` 由 `IBlocklyHost.VariableStorageFactory.Create(scope)` 提供。
- **承诺**：ScopeChain 抽出过程中**业务方调用代码零修改**——锁死。

## §3 值容器与值栈

- 值容器为 `BoxedValue<T>`（强类型泛型）+ 抽象 `IBoxedValue`。
- **禁止引入无类型 Variant**。
- 旁路值栈：
  - `Blockly.Push<T>(T)`
  - `T Blockly.Pop<T>()`

## §4 IProcedureImpl / IFunctionImpl 高 arity 补齐（N=5）

- **N=5 = 参数 arity 上限**（拍板，原 8 改 5；与作用域嵌套深度无关）。
- 补齐至 0..5 参，签名沿现有形态（`in T_i` 入参、`Evaluate` 方法名、协变 `TOutput`）：
  - `IProcedureImpl<T1..Tn>` n=0..5（共 6 个）
  - `IFunctionImpl<T1..Tn,TOutput>` n=0..5（共 6 个）
- 同步补基类包装 `Procedure<TImpl,T1..Tn>` / `Function<TImpl,T1..Tn,TOutput>`。
- **Pop 顺序**与现有 2 参一致：从右到左 Pop（`T_N` 先 Pop，`T_1` 最后 Pop）。
- 新增高 arity 时**不要求**同步给样例。

## §5 行为图 tick 协议（Tick 返回值双态枚举）

新增枚举 `BehaviorResult`：

- `Running` —— 未完成、下一帧再 tick。
- `Done` —— 已完成。

`IBehaviorNode` 五个生命周期点（**保持现状**，仅 Tick 返回类型改）：

- `void Start()` —— 节点首次进入活动态时调用一次。
- **`BehaviorResult Tick(float deltaTime)`** —— 每帧推进；返回 Running 表示下一帧继续，Done 表示本节点本次活动周期结束。
- `void LateTick(float deltaTime)` —— 同帧 Tick 之后的延后处理。
- `void Finish()` —— 节点退出活动态时调用一次。
- `void OnDestroy()` —— 节点资源回收。

入口：`BehaviorGraph.Blockly.Start / Update(float) / LateUpdate(float) / Finish / Restart`。

已知节点子类（仅记录现状、**不冻结**清单）：`BranchNode / SwitchNode / SelectorNode / LoopNode / ParallelNode / SequenceNode / LogicBehavior / Timeline`——这些节点的 Tick 实现需在第一阶段实施时全部迁移到双态返回。

### 叶子时长门控（与 §1 分支决策的边界）

双态语义下，叶子节点允许通过表达式求值结果决定**自己本次活动周期内的 Running 时长**——即「这帧完事还是再来一帧」的自我门控；这**不等于**「节点返回值参与分支」。§1 锁定的是**节点间的分支选择**（哪个子节点被激活）由 LogicGraph 条件表达式决定，与节点 Tick 返回值无关。叶子内部「再 tick 一帧 / 收尾」是**同一个节点对自己活动周期的时长控制**，与分支语义正交。

典型实现：`LogicBehavior` 通过 `onTick: LogicGraph` 表达式（`[ExpressionSignature(typeof(bool))]` 标注）求值——`true → Done`、`false → Running`。源数据结构 `LogicBehaviorSource.onTick` 字段的 bool 标注是本约定的物化形态，不在第一阶段拆除。其他叶子节点亦可采用同模式。

**反约束**：叶子不得通过 Tick 返回值向**父节点 / 兄弟节点**传递「分支应走哪条」「失败 / 成功」等语义信息——这类决策属 §1 范畴，必须由 LogicGraph 条件表达式承担。

## §6 Host 聚合门面

- 入口 API 仅接收 `IBlocklyHost`。
- 细粒度能力接口冻结清单：
  - `IBlocklyLogger`
  - `IBlocklyNodeFactory`
  - `IBlocklyPool`
  - `IBlocklySerializer`
  - `IBlocklyVariableStorage` + `IBlocklyVariableStorageFactory`
  - `IBlocklySource`

- **节点身份**：每个 `IBehaviorNode` / `IProcedureImpl` 实例由宿主分配 Guid，承诺通过 `IBlocklyHost` 暴露双向映射查询（Guid ↔ 节点实例）。具体接口签名 Phase 2 锁。
- **`IBlocklySerializer` 最小不变量**：(a) 序列化保留上述节点 Guid（Guid 不能在反序列化时重新生成）；(b) 序列化-反序列化对称——同一图 round-trip 后语义等价。具体 IR 格式（JSON / 二进制 / 版本号编排）Phase 2 锁。

## §7 错误处理与非冻结声明

### 错误处理

本契约对运行时异常类型不做枚举式冻结。已知现状：

- 作用域重名抛 `InvalidOperationException`。
- `Init` 类型不匹配抛 `ArgumentException`。
- 根作用域缺 host 抛 `InvalidOperationException`。

新引入异常由实施阶段侦察后补。

### 异常路径与 Tick 返回值的关系（双态收敛后精化）

**异常语义不承载在 Tick 返回值中**——Running / Done 仅表达「未完成 / 已完成」两种正常推进状态，不携带「失败」语义。

**异常处理策略锁定为：基类 try/catch + IBlocklyLogger.Error + 收敛为 Done**。即 `BehaviorNode` / `CompositeBehavior` 基类在 Tick 路径包 try/catch；捕获到异常时通过 `IBlocklyHost.Logger.Error` 记录详情，并以 `Done` 收尾本节点本次活动周期。调用方可通过订阅 IBlocklyLogger 观察异常事件，但**不**通过 Tick 返回值得知。

该策略的语义：异常 = 「本节点没法继续推进，但行为图不应整图崩溃」——以 Done 强制结束当前节点活动周期、把控制权交还父节点 / 调度层；详情走 Logger 旁路。

### 非冻结声明清单（7 项，全部保持不锁）

1. 反射识别规则。
2. `UgcSourceAttribute / UgcMethod / UgcProperty / ExpressionSignatureAttribute` 等元数据语义。
3. codegen 输出格式与目标产物。
4. 节点注册表数据结构。
5. 行为/逻辑节点的具体子类清单。
6. IR 序列化格式与 Editor ↔ Runtime 双向转换协议。
7. 编辑器工具。
