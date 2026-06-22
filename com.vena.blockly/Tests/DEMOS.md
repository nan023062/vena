# Vena Blockly Demos 清单

## Tests/01 LogicRuntime

| Demo | 类型 | 演示内容 | 预期输出 |
|---|---|---|---|
| `LogicRuntimeDemo` | MonoBehaviour | 每帧重建源 → `AddInt(ConstInt(a), ConstInt(b))` + `ConstFloat`，Inspector 联动 | `lastIntSum / lastFloatValue` 回写 Inspector |
| `NestedExpressionDemo` | MonoBehaviour | 深度嵌套表达式 `((3+4)*(10-6)) + Sum5(1..5)` | `result = 43` |
| `ControlFlowDemo` | MonoBehaviour | `Sequence + Branch + SetVariable/GetVariable`（if-else over `x > 5`） | `result = 20` |
| `WhileLoopDemo` | MonoBehaviour | `LogicWhile` 累加 1..10（共享变量 `sum / i / n`） | `sum = 55` |
| `Arity5Smoke` | 节点定义 | 5-arity `IFunctionImpl` / `IProcedureImpl` 接口与基类包装编译对齐 | smoke 用例，无独立入口 |
| `DemoExprNodes` | 节点定义 | Const / Add / Subtract / Multiply / GreaterThan / LessThanOrEqual 等 Int 节点源 | 供上面 demo 装配使用 |
| `DemoSampleHost` | 辅助 | 注入 `ReflectionNodeFactory`，按 `[BlocklySource]` 反射定位 NodeType | 供 demo 共用 |

## Tests/02 BehaviorRuntime

| Demo | 类型 | 演示内容 | 预期输出 |
|---|---|---|---|
| `BehaviorRuntimeDemo` | MonoBehaviour | `Sequence(Hello("Hello"), Hello("World"))`，Inspector 控制 tick 步数 | Console 日志 `[HelloBehavior] Start/Tick/Finish`，graph 数帧后退出 playing |
| `BehaviorBranchDemo` | MonoBehaviour | `BranchNode (if-else)` 以常量 bool 选边，两支各挂 HelloBehavior | conditionInput=true → `[HelloBehavior] ... alive`；false → `dying` |
| `BehaviorLoopDemo` | MonoBehaviour | `LoopNode(loopCount=3)` 跑 `Sequence(Hello("iter-A"), Hello("iter-B"))` | 6 次 Hello tick（A/B 各 3） |
| `BehaviorParallelMultiTickDemo` | MonoBehaviour | `Parallel(Countdown("A", 3), Countdown("B", 5))`，多帧 Running 叶子 | 前 3 帧 A+B 同 Tick，4-5 帧仅 B，第 5 帧末 Parallel Done |
| `BehaviorResultSmoke` | 辅助类 | `RunSequence / RunParallel / RunSelector / RunExceptionPath` 4 个 smoke 接口 | 各返回 bool，可接 NUnit/XUnit；异常路径走 Logger.Error + Done |
| `TimelineSignalDemo` | MonoBehaviour | `LogicClip(duration=0.5s @ 30fps)` + frame=5 `Signal`，全程 LogSignalSource 副作用 | Console 顺序：clip begin → clip frame ×N → signal at frame 5 → clip frame ×N → clip end |
| `TimelineImplDemo` | MonoBehaviour | IClip codegen demo：`HelloClipSource` 接入 Timeline，greeting 走 LogicGraph 槽 | Console：`[HelloClip] Begin: TimelineHello` → `[HelloClip] End: TimelineHello` |
| `HelloBehaviorImpl` | 节点定义 | 最小叶子 IBehavior，Tick 当帧 Done；codegen 生成 `HelloBehaviorSource` | 供 BehaviorRuntime / Branch / Loop demo 使用 |
| `HelloClipImpl` | 节点定义 | 最小叶子 IClip，Begin/End 打日志；codegen 生成 `HelloClipSource` | 供 TimelineImplDemo 使用 |
| `TestBehaviorImpl` | 节点定义 | 手写 BehaviorSource 参考样例（`SampleBehaviorImpl1/2`，1 无槽位、2 含 LogicGraph 字符串槽） | 供 BehaviorResultSmoke 使用 |
| `DemoBehaviorNodes` | 节点定义 | Demo 用 Const bool / int / string、Countdown 多帧叶子、LogSignal 副作用源 | 供 02 内 demo 装配 |
| `DemoSampleHost` | 辅助 | 注入 `ReflectionNodeFactory` | 供 demo 共用 |

## Tests/03 GraphEditorUI

| Demo | 类型 | 入口 | 演示内容 |
|---|---|---|---|
| `DemoC_GraphAssetGenerator.Generate` | Editor Menu | `Tools/Vena/Blockly/Demos/03 GraphEditorUI/Generate Demo Graph Asset` | 在 `Assets/VenaBlocklyDemos/` 下创建（或复用）`DemoC_GraphAsset.asset`，写入 placeholder JSON，Ping + 选中 |
| `DemoSampleHost` | 辅助 | — | 形态记号，Demo C 当前不调用 NodeFactory |

## Tests/04 Codegen

| Demo | 类型 | 入口 | 演示内容 |
|---|---|---|---|
| `DemoD_CodegenMenu.SetupConfig` | Editor Menu | `Tools/Vena/Blockly/Demos/04 Codegen/Setup Demo D Config` | 创建 `Assets/VenaBlocklyDemos/04_Codegen/DemoD_CodegenConfig.asset`，OutputRoot 解析 `${PackagePath}/Tests/04_Codegen/Generated`，AssemblyWhitelist=`Vena.Blockly.Tests.Codegen` |
| `DemoD_CodegenMenu.RunCodegen` | Editor Menu | `Tools/Vena/Blockly/Demos/04 Codegen/Run Demo D Codegen` | 委托包心 `CodegenMenu.RunCodegen()`，扫白名单 asmdef，将 `InstanceMethod.cs` 等源类的三件套写到 `Generated/` |
| `InstanceMethod` | 节点定义 | — | 实例方法 codegen 示例：`[Blockly("测试对象")]` 类带 `TestMethod` / `PrintMessage` 两个 `[Blockly]` 方法 |
| `DemoSampleHost` | 辅助 | — | 形态记号，Demo D 当前不调用 NodeFactory |
