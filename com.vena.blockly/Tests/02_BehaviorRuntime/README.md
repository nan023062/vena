# 02 Behavior Runtime

由 `MonoBehaviour` 驱动的 `BehaviorGraph` 演示。共六个部分：

- **Part A —— Sequence + Leaf。** 打开场景，点 Play，在 Console 里观察
  `Sequence ( Hello("Hello"), Hello("World") )` 图 tick 到完成。
- **Part B —— Timeline 集成。** 驱动一条 `Timeline` clip，它的 `time`
  槽由一个 `LogicGraph` 求值出的 float 提供；演示
  `UClipSource<TestClip>` / `UClip<TestClipSource, TestClip>` 三件套。
- **Part C —— BranchNode（if-else）。** 一个 `BranchNode`，其
  `condition` 是常量 `LogicGraph<bool>`，在两个 `Hello` 叶子之间二选一。
- **Part D —— LoopNode。** 一个 `LoopNode`，其 `loopCount` 是常量
  `LogicGraph<int>`，把内部的 `Sequence` 跑 N 次。
- **Part E —— Parallel + 多帧 Running。** 一个 `ParallelNode` 并行运行
  两个 tick 预算不同的 `Countdown` 叶子；演示 `BehaviorResult.Running`
  跨多帧穿过 composite 的传播。
- **Part F —— Timeline ExpressionClip + Signal。** 一条 `Timeline`
  轨道，包含一个 `ExpressionClip`（onBegin / onFrame / onEnd 每个都由
  带副作用的 `LogicGraph` 驱动）加一个可配置帧数的 `Signal`。

## 用途

- 展示 `BehaviorGraph.Blockly` 的最小可用接线方式（host → source tree
  → `Set` / `SetSource` / `Start` / `Update` / `Destroy`）。
- 演示如何组合一个 composite (`SequenceNode`) 加一个自定义 leaf
  (`HelloBehaviorSource` / `HelloBehaviorImpl`)，且不必动 package core
  里的任何东西。
- 展示一种 `Timeline` clip 类型 (`TestClip` / `TestClipSource`) 如何
  作为行为图接入同一个 host。

## 如何打开

1. 从一个直接挂载了 `Tests/02_BehaviorRuntime` 的测试工程打开自带场景，
   或者把这个目录复制到工程的 `Assets/` 下。
2. 打开 `Scenes/BehaviorRuntimeDemo.unity`（如果 Unity 还没有重新生成
   场景，按下文 *Scene* 一节手动创建）。
3. 点 Play。

## Part A —— Sequence + Leaf

### 预期 Console 输出

```
[BehaviorDemo] graph started
[HelloBehavior] Start: Hello
[HelloBehavior] Tick: Hello
[HelloBehavior] Finish: Hello
[HelloBehavior] Start: World
[HelloBehavior] Tick: World
[HelloBehavior] Finish: World
[BehaviorDemo] tick 1, playing=True
[BehaviorDemo] tick 2, playing=False
[BehaviorDemo] graph done
```

（`[HelloBehavior]` 各行相对 `[BehaviorDemo] tick N` 的精确顺序
可能因引擎 flush 时机略有差异，但每一组 Start / Tick / Finish
都会按 greeting 顺序出现：先 `Hello` 后 `World`。）

### 可调参数

在 `BehaviorRuntimeDemo` 组件上：

- **Tick Count** —— 在示例停止驱动 graph 之前发送多少次 `Update`
  tick（默认 5；Sequence 在 2 个 tick 内就跑完）。
- **Fixed Delta Time** —— 传给 `_graph.Update(...)` 的 `deltaTime`
  值（默认 `0.016f`，约一帧 60Hz）。

两个只读字段 **Is Playing** 和 **Ticked Frames** 把实时的 graph
状态同步到 Inspector，便于一眼调试。

### 参考代码

- `Scripts/BehaviorRuntimeDemo.cs` —— `MonoBehaviour` 入口：构造
  source tree，持有 `BehaviorGraph.Blockly` 实例，驱动 `Update`。
- `Scripts/HelloBehaviorImpl.cs` —— 带 `greeting` 字段的 `IBehaviorImpl`
  叶子；配套的 `HelloBehaviorSource : BehaviorNodeSource<HelloBehaviorImpl>`
  是 source 侧包装，通过 `InitializeProperties` 把字段转发过去。
- `Scripts/DemoSampleHost.cs` —— `BlocklyHostBase` 子类，重写
  `NodeFactory` 改用 `ReflectionNodeFactory`，让演示里自定义的 source
  无需依赖生成的 `INodeMetadataProvider` 就能解析到。

## Part B —— Timeline TestClip

Part A 行为图在 `Timeline` 侧的表亲。clip 类
`TestClip : IUClip` 暴露一个 `time` 字段，clip 初始化时由
`LogicGraph` 求值出的 float 通过 `UClip<TestClipSource, TestClip>`
喂入。

### 预期行为

自带场景里串了一个 `TimelineRuntimeDemo` GameObject，它构造一个
带常量 `LogicGraph` source 的 `TestClipSource`，跑一次 `Begin` /
`OnFrame` / `End`，并把 clip 收到的 `time` 值打到 log 里。默认情况下
log 行是：

```
[TimelineDemo] TestClip.time after Begin = <value-from-LogicGraph>
```

### 参考代码

- `Scripts/TimelineRuntimeDemo.cs` —— `MonoBehaviour` 入口：构造一棵
  `TestClipSource` source tree，经由 host 串起来，跑一轮
  `Begin` / `OnFrame` / `End`。
- `Scripts/Timeline/TestClip.cs` —— `IUClip` 实现加上 `TestClipSource :
  UClipSource<TestClip>` 的 source/Node 三件套。（从 package 内部的
  `Samples/Timeline/TestClip.cs` 迁移过来；命名空间扁平化到 demo
  命名空间。）

## 场景

该示例需要一个挂了 `BehaviorRuntimeDemo` 组件的 `BehaviorDemoRunner`
GameObject，加一个 `Main Camera`，可选再加一个挂
`TimelineRuntimeDemo` 的 `TimelineDemoRunner` GameObject 用于 Part B。
如果自带场景不存在（例如 Unity 还没重新生成 `.meta`），手动创建一个：

1. `File → New Scene`（Basic 2D / 3D 都行）。
2. 创建一个空 GameObject，命名为 `BehaviorDemoRunner`。
3. `Add Component → Behavior Runtime Demo`。
4. （Part B）再加一个空 GameObject，命名为 `TimelineDemoRunner`，
   `Add Component → Timeline Runtime Demo`。
5. 保存为 `Scenes/BehaviorRuntimeDemo.unity`。

## Part C —— BranchNode（if-else）

一个 `BranchNode`，其 `condition` 是常量 `LogicGraph<bool>`
（由 demo 本地的 `DemoConstBoolSource` 驱动），在
`HelloBehaviorSource { greeting = "alive" }` 与
`HelloBehaviorSource { greeting = "dying" }` 之间二选一。

### 可调参数

在 `BehaviorBranchDemo` 组件上：

- **Condition Input** —— `bool`；`true` 选 `alive` 分支，`false`
  选 `dying` 分支。在 `Awake()` 读取并烘进 source tree。
- **Tick Count / Fixed Delta Time** —— 含义同 Part A。

### 预期 Console 输出（Condition Input = true）

```
[BranchDemo] graph started, conditionInput=True
[HelloBehavior] Start: alive
[HelloBehavior] Tick: alive
[HelloBehavior] Finish: alive
[BranchDemo] graph done after 1 ticks
```

`dying` 分支不会打印任何东西。把 Condition Input 切到 `false`，
就只有 `dying` 分支会打印。

### 参考代码

- `Scripts/BehaviorBranchDemo.cs` —— `MonoBehaviour` 入口。
- `Scripts/DemoBehaviorNodes.cs` —— `DemoConstBoolImpl` /
  `DemoConstBoolSource`（作为 condition 输入的 LogicGraph 常量）。

## Part D —— LoopNode

一个 `LoopNode`，其 `loopCount` 是常量 `LogicGraph<int>`（由 demo
本地的 `DemoConstIntSource` 驱动），把内部的
`Sequence(Hello("iter-A"), Hello("iter-B"))` 跑 N 次。

### 可调参数

在 `BehaviorLoopDemo` 组件上：

- **Loop Count Input** —— `int`（默认 3）；在 `Awake()` 读取。
- **Tick Count / Fixed Delta Time** —— 含义同 Part A。默认 20，
  给循环足够的时间跑完。

### 预期 Console 输出（Loop Count Input = 3）

`iter-A` 和 `iter-B` 的 `Start/Tick/Finish` 交替出现，重复三次——
总共六行 `[HelloBehavior] Tick:`，问候语交替——之后是
`[LoopDemo] graph done after N ticks`。

### 参考代码

- `Scripts/BehaviorLoopDemo.cs` —— `MonoBehaviour` 入口。
- `Scripts/DemoBehaviorNodes.cs` —— `DemoConstIntImpl` / `DemoConstIntSource`。

## Part E —— Parallel + 多帧 Running

一个 `ParallelNode` 并行运行两个 `CountdownBehaviorSource` 叶子：

```
ParallelNode
  ├─ CountdownBehaviorSource { ticksToRun = aTicksInput, label = "A" }
  └─ CountdownBehaviorSource { ticksToRun = bTicksInput, label = "B" }
```

`CountdownBehaviorImpl.Tick` 在剩余计数归零前返回
`BehaviorResult.Running`，归零后返回 `Done`。这是第一个真正
触发 `Running` 跨多帧穿过 composite 传播的部分——`ParallelNode`
会一直报 `Running`，直到 *两个* 子节点都完成。

### 可调参数

- **A Ticks Input**（默认 3）和 **B Ticks Input**（默认 5）——
  每个倒计时在报 `Done` 之前能撑过多少次 `Tick` 调用。
- **Tick Count / Fixed Delta Time** —— host 循环的节奏。

### 预期 Console 输出（A=3, B=5）

```
[ParallelDemo] graph started, A=3 ticks, B=5 ticks
[Countdown:A] Start, remaining=3
[Countdown:B] Start, remaining=5
[Countdown:A] Tick, remaining=2
[Countdown:B] Tick, remaining=4
[ParallelDemo] tick 1, playing=True
[Countdown:A] Tick, remaining=1
[Countdown:B] Tick, remaining=3
[ParallelDemo] tick 2, playing=True
[Countdown:A] Tick, remaining=0
[Countdown:A] Finish
[Countdown:B] Tick, remaining=2
[ParallelDemo] tick 3, playing=True
[Countdown:B] Tick, remaining=1
[ParallelDemo] tick 4, playing=True
[Countdown:B] Tick, remaining=0
[Countdown:B] Finish
[ParallelDemo] tick 5, playing=False
[ParallelDemo] graph done after 5 ticks
```

A 在第 3 帧之后退出；B 在第 5 帧退出。`Finish` 相对
`[ParallelDemo] tick N` 行的引擎 flush 顺序可能略有差异。

### 参考代码

- `Scripts/BehaviorParallelMultiTickDemo.cs` —— `MonoBehaviour` 入口。
- `Scripts/DemoBehaviorNodes.cs` —— `CountdownBehaviorImpl` /
  `CountdownBehaviorSource`（多帧 Running 的叶子节点）。

## Part F —— Timeline ExpressionClip + Signal

一条由 `BehaviorGraph` 驱动的 `Timeline`，其轨道组合了：

- 一个 `ExpressionClip`，其 `onBegin`、`onFrame`、`onEnd` 各自是一个
  包了 `LogSignalSource`（demo 本地的 `Procedure<...>`，
  `Debug.Log` 它的 `message` 字段）的 `LogicGraph`。
- 一个 `Signal`，位于可配置的帧数上，同样由 `LogSignalSource` 驱动。

`ExpressionClip.duration = clipDuration`（默认 `0.5s` @ `30fps` →
`15` 个 clip 帧）。`Timeline.Track` 在 clip 帧 1 上同时调用 `Begin`
和 `OnFrame`，在 clip 帧 2..N 上调用 `OnFrame`，并在 clip 的帧计数
到 `frameCount` 时调用 `End`。`Signal` 在指定帧上触发，
与 clip 状态无关。

### 可调参数

- **Clip Duration**（默认 `0.5s`）和 **Signal Frame**（默认 `5`）——
  决定 timeline 的形状。
- **Total Update Ticks**（默认 `30`）—— host 在停止调用 `Update`
  之前要泵多少次 tick。
- **Fixed Delta Time**（默认 `1f/30f`）—— 每次 Unity `Update` 对应
  timeline 帧率下的一帧。

### 预期 Console 输出（默认值）

```
[TimelineSignalDemo] graph started, clipDuration=0.5s, signalFrame=5
[Signal] clip begin                  # timeline frame 1
[Signal] clip frame                  # timeline frame 1 (clip Begin and OnFrame run together on the clip's first frame)
[Signal] clip frame                  # timeline frame 2
[Signal] clip frame                  # timeline frame 3
[Signal] clip frame                  # timeline frame 4
[Signal] signal at frame 5           # timeline frame 5 — Track runs signals before clip updates
[Signal] clip frame                  # timeline frame 5
[Signal] clip frame                  # timeline frame 6
... (clip frame ×9 more)
[Signal] clip frame                  # timeline frame 15
[Signal] clip end                    # timeline frame 15 — End fires together with the clip's last OnFrame
[TimelineSignalDemo] graph done after 16 ticks
```

具体 tick 数取决于 timeline 怎么把 clip 帧 15 (End) 对齐到 host
的 tick 边界，不过在 `fixedDeltaTime = 1/30s` 下落在 tick 16。

### 参考代码

- `Scripts/TimelineSignalDemo.cs` —— `MonoBehaviour` 入口。用反射构造
  `TimelineSource.TrackSource<,>`，因为 `ExpressionClip.Object` 是
  `private` 的嵌套类型（路径同 `TimelineRuntimeDemo`）。
- `Scripts/DemoBehaviorNodes.cs` —— `LogSignalImpl` / `LogSignalSource`
  （`Procedure<LogSignalImpl>`，`Evaluate` 时 `Debug.Log` `message`）。

## 后续

- 把 `SequenceNode` 换成 `ParallelNode`（定义在 `Vena.Blockly` 里），
  看两个叶子在同一帧 Start。
- 改写 `HelloBehaviorImpl.Tick`，让它在 `frame < N` 时返回
  `BehaviorResult.Running`，之后返回 `BehaviorResult.Done` ——
  这就把叶子变成一个多帧的活动。
- 同引擎的值求值侧请见同级示例 `01 Logic Runtime`。
