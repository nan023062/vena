# 01 Logic Runtime

`LogicGraph` 的实时演示。在 Inspector 里调整输入，观察每帧
`Add(int, int)` 和 `Const(float)` 的求值结果——底层走的是
`Function<TImpl, T1, T2, TOutput>` 加 `BoxedValue` Push/Pop 的栈帧机制。

现在打包了 **四个** 入口——原始的逐帧冒烟示例
(`LogicRuntimeDemo`)、一个一次性的深层嵌套表达式
(`NestedExpressionDemo`)、一个控制流 + 共享变量的示例
(`ControlFlowDemo`)，以及一个 `LogicWhile` 累加器
(`WhileLoopDemo`)。后两个示例通过一段显式的宿主侧变量预声明，
在 `LogicSequence` 的兄弟语句之间共享状态——具体机制和已知限制见下文
*Shared variables across `LogicSequence` statements* 一节。

## 用途

- 展示 `LogicGraph.Blockly` 的最小可用接线方式（host → source tree
  → `Set` / `SetSource` / `Call<TResult>` / `Destroy`）。
- 演示如何基于 `Function<TImpl, ...>` 基类编写一个 0 参函数
  (`ConstIntSource` / `ConstFloatSource`) 和一个 2 参函数
  (`AddIntSource`)——包括嵌套的 `Expression` 槽如何组合成一个可求值的整图。
- 演示更深的组合：多层 `Function` 嵌套加上一个 5 参函数
  (`Sum5IntsSource`)，统一通过一次 `Call<int>()` 求值
  (`NestedExpressionDemo`)。
- 演示控制流原语（`LogicSequence` + `LogicBranch` +
  `LogicSetVariable<int>` / `LogicGetVariable<int>`）在兄弟语句之间共享
  同一个变量 (`ControlFlowDemo`)。
- 演示 `LogicWhile` 用同样的共享变量模式在多次迭代之间累加
  (`WhileLoopDemo`)。

## 如何打开

1. 从一个直接挂载了 `Tests/01_LogicRuntime` 的测试工程打开自带场景，
   或者把这个目录复制到工程的 `Assets/` 下。
2. 打开 `Scenes/LogicRuntimeDemo.unity`（如果 Unity 还没有重新生成场景，
   按下文 *Scene* 一节手动创建）。
3. 点 Play。

## 预期行为

### `LogicRuntimeDemo`（逐帧冒烟）

使用默认值（`operandA = 3`，`operandB = 4`，`floatOperand = 1.5`），
第一帧上：

- `Last Int Sum` 显示 `7`
- `Last Float Value` 显示 `1.5`
- `Evaluation Count` 每帧加 1

在 Play 模式下把 `Operand A` 改成 `10`——下一帧 `Last Int Sum` 变为
`14`。`Operand B` 和 `Float Operand` 同理。

### `NestedExpressionDemo`（一次性深层嵌套）

在 `Start()` 时：

- `Last Result` 显示 `43`
- Console 打印
  `[NestedExpressionDemo] ((3+4)*(10-6)) + Sum5(1..5) = 43 (expected 43)`

### `ControlFlowDemo`（一次性 Sequence + Branch）

在 `Start()` 时：

- `Last Result` 显示 `20`
- Console 打印
  `[ControlFlowDemo] if (x>5) result = x*2 else result = x;  result = 20 (expected 20)`

该图求值的是 `int x = 10; int result; if (x > 5) result = x * 2;
else result = x;`——即 `result == 20`。

### `WhileLoopDemo`（一次性 While 累加器）

在 `Start()` 时：

- `Last Sum` 显示 `55`
- Console 打印
  `[WhileLoopDemo] sum 1..10 = 55 (expected 55)`

该图求值的是 `int sum = 0; int i = 1; int n = 10; while (i <= n)
{ sum = sum + i; i = i + 1; }`——即 `sum == 1 + 2 + ... + 10 == 55`。

## 可调参数

在 `LogicRuntimeDemo` 组件上：

- **Operand A / Operand B** —— `AddInt` 图的输入。
- **Float Operand** —— 独立 `ConstFloat` 图的输入。

三个只读字段 **Last Int Sum**、**Last Float Value** 和
**Evaluation Count** 把实时求值结果同步显示到 Inspector。

## 参考代码

- `Scripts/LogicRuntimeDemo.cs` —— `MonoBehaviour` 入口：持有 host，
  每帧重建并求值一个 int 图和一个 float 图。
- `Scripts/NestedExpressionDemo.cs` —— 一次性的 `Start()` 示例，
  求值 `((3 + 4) * (10 - 6)) + Sum5(1, 2, 3, 4, 5) = 43`，用来验证
  多层 `Function` 嵌套加 5 参函数走一次 `Call<int>()`。
- `Scripts/ControlFlowDemo.cs` —— 一次性 `Start()` 示例，把
  `LogicSequence`、`LogicBranch`、`LogicSetVariableInt` /
  `LogicGetVariableInt` 组合起来求值 `if (x > 5) result = x * 2; else
  result = x;`，其中 `x = 10`。通过宿主侧变量预声明（见下文）
  在两个兄弟语句之间共享 `x` / `result`。
- `Scripts/WhileLoopDemo.cs` —— 一次性 `Start()` 示例，用 `LogicWhile`
  加上 `LogicSequence` body 累加 `sum = 1 + 2 + ... + 10 = 55`。
  循环计数器 `i`、上界 `n` 和累加器 `sum` 走的是和 `ControlFlowDemo`
  一样的宿主侧预声明模式。
- `Scripts/DemoExprNodes.cs` —— 演示用到的节点族：
  - `ConstIntImpl` + `ConstIntSource : Function<ConstIntImpl, int>`
  - `ConstFloatImpl` + `ConstFloatSource : Function<ConstFloatImpl, float>`
  - `AddIntImpl` + `AddIntSource : Function<AddIntImpl, int, int, int>`
  - `SubtractIntImpl` + `SubtractIntSource`（int×int→int）
  - `MultiplyIntImpl` + `MultiplyIntSource`（int×int→int）
  - `GreaterThanIntImpl` + `GreaterThanIntSource`（int×int→bool）
  - `LessThanOrEqualIntImpl` + `LessThanOrEqualIntSource`（int×int→bool）

  新增的四个算术/比较 source 与 `AddIntSource` 的 5 步协议布局一致
  （`Initialize` → `EvaluateChildren` → `InitializeProperties`
  → `_impl.Evaluate(...)` → `Push(result)`，由
  `Function<TImpl, T1, T2, TOutput>` 基类驱动）。返回 `bool` 的 source
  完全靠 `Function` 基类的 `TOutput = bool`——不需要额外的签名 attribute，
  因为 `[ExpressionSignature(typeof(bool))]` 只作用于 `LogicGraph`
  的 *槽* 字段（例如 `LogicBranch.condition`），并在 Editor 侧检查。
- `Scripts/Arity5Smoke.cs` —— `NestedExpressionDemo` 复用的 `Sum5IntsSource`。
- `Scripts/DemoSampleHost.cs` —— `BlocklyHostBase` 子类，重写
  `NodeFactory` 改用 `ReflectionNodeFactory`，让演示里自定义的 source
  无需依赖生成的 `INodeMetadataProvider` 就能解析到。

## 场景

该示例需要一个挂了 `LogicRuntimeDemo` 组件的 `LogicDemoRunner`
GameObject。如果自带场景不存在（例如 Unity 还没重新生成 `.meta`），
手动创建一个：

1. `File → New Scene`（Basic 2D / 3D 都行）。
2. 创建一个空 GameObject，命名为 `LogicDemoRunner`。
3. `Add Component → Logic Runtime Demo`。
4. 保存为 `Scenes/LogicRuntimeDemo.unity`。

要同时跑 `NestedExpressionDemo`：

1. 再加一个空 GameObject，命名为 `NestedExpressionRunner`。
2. `Add Component → Nested Expression Demo`。
3. 点 Play —— 结果会在 `Start()` 时打印一次到 Console：
   `[NestedExpressionDemo] ((3+4)*(10-6)) + Sum5(1..5) = 43 (expected 43)`。
   组件本身也把 `lastResult` 显示到 Inspector。

要同时跑 `ControlFlowDemo`：

1. 加一个空 GameObject，命名为 `ControlFlowRunner`。
2. `Add Component → Control Flow Demo`。
3. 点 Play —— Console：
   `[ControlFlowDemo] if (x>5) result = x*2 else result = x;  result = 20 (expected 20)`。
   组件把 `lastResult` 显示到 Inspector。

要同时跑 `WhileLoopDemo`：

1. 加一个空 GameObject，命名为 `WhileLoopRunner`。
2. `Add Component → While Loop Demo`。
3. 点 Play —— Console：
   `[WhileLoopDemo] sum 1..10 = 55 (expected 55)`。
   组件把 `lastSum` 显示到 Inspector。

## 关于每帧重建

为了示例的清晰，这里每帧都会重建 source tree 和 `LogicGraph.Blockly`
实例，然后 `Destroy`。生产环境应当复用同一个 `LogicGraph.Blockly`，
只通过变量 / 参数喂入新的输入——具体的变量通道模式见 package core 的
`LogicSetVariable<T>` / `LogicGetVariable<T>`。

## 后续

- 组合一个更深的表达式：`Add ( Add(a, b), c )`，即嵌套两个
  `AddIntSource` 实例。
- 把 `ConstIntSource` 换成 `LogicGetVariableInt`（定义在
  `Vena.Blockly`），通过 `Variables` 而不是字段喂入数值。
- 同引擎的时间步进侧请见同级示例 `02 Behavior Runtime`。

## Shared variables across `LogicSequence` statements

`ControlFlowDemo` 和 `WhileLoopDemo` 都依赖多条兄弟语句读写同一组
变量（例如 `x`、`result`、`sum`、`i`、`n`）。它们共享状态的方式
值得单独说明，因为这是一个 **刻意为之的 workaround**，并不是该 API
的长期形态。

**机制（option A —— 宿主预声明）。** 在 `SetSource(...)` 之后、
`Invoke()` 之前，demo host 会针对每一个兄弟语句需要共享的变量，在
**根** `LogicGraph.Blockly` 上调用
`graph.SetVariable<int>(name, initialValue)`。这些条目落在根
`ScopeChain.Variables` 里。运行时，兄弟语句生活在 **子** Blockly
scope 中；它们的 `LogicSetVariable<T>` 会调用
`ScopeChain.ResolveWriteTarget`，沿父链向上找，在根 scope 找到这个
预先声明好的名字——于是写入 **穿透** 到根。兄弟 scope 后续的读取
同样沿父链解析到同一个根条目。结果就是：所有兄弟语句对同一个名字
看到的是同一份共享值。

**为什么这是 workaround。** `Runtime/Expression/LogicControl.cs` 的
当前实现里，`LogicSequence.Node` 会为每条语句构造一个 *子*
`LogicGraph.Blockly`（走 `Blockly.CreateBlockly(...)`）。兄弟 Blockly
互相不在对方的父链上，所以在某条语句内部声明的状态对其它兄弟语句
是不可见的。如果不做宿主侧预声明，lookup 会一路掉到 `default(T)`，
示例就会安静地返回 `0` 而不是 `20` / `55`。

**限制。** 这个模式要求 C# 宿主代码提前知道所有共享变量的名字。
玩家在运行时编写的纯 UGC 图没有这种钩子——除了图本身之外没有所谓
的"host"。要补上这个缺口，得改 `LogicSequence`，让它的各条语句
共享同一个宿主 scope，而不是各自拥有一个私有的子 Blockly（等价地，
让 `LogicSetVariable<T>` 声明到宿主 scope 而不是局部语句 scope）。
这是一个 runtime 架构改动，作为 **task #19（LogicSequence shared
host scope）** 记录在项目 backlog 里，对这些示例而言 **明确属于
out-of-scope**——这些示例存在的目的就是用当下的 runtime 形态去
跑通 `LogicGraph` 的控制流原语。
