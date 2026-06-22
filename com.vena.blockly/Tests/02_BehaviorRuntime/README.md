# 02 Behavior Runtime

## 测试内容

六个 MonoBehaviour 验证 `BehaviorGraph` 控制节点 + `Timeline` 集成：

- `BehaviorRuntimeDemo` — `Sequence(Hello("Hello"), Hello("World"))`，预期 Console 按序打出两组 `Start/Tick/Finish`，2 个 tick 后 `graph done`
- `TimelineRuntimeDemo` — `TestClipSource` 一轮 `Begin/OnFrame/End`，`time` 槽由常量 `LogicGraph<float>` 提供，预期 `TestClip.time after Begin = <value>`
- `BehaviorBranchDemo` — `BranchNode` 上 `LogicGraph<bool>` condition 二选一，预期 `Condition Input = true` 时只打 `alive` 那条
- `BehaviorLoopDemo` — `LoopNode` 上 `LogicGraph<int>` count 把内部 `Sequence` 跑 N 次，`Loop Count Input = 3` 预期 `iter-A` / `iter-B` 交替 3 轮
- `BehaviorParallelMultiTickDemo` — `ParallelNode` 并行两个 `Countdown` 叶子，`BehaviorResult.Running` 跨多帧传播，A=3 / B=5 预期 5 tick 后 `graph done`
- `TimelineSignalDemo` — `ExpressionClip`（onBegin / onFrame / onEnd 各驱动 `LogSignalSource`）+ 指定帧 `Signal`，默认 `clipDuration = 0.5s @ 30fps`，预期 `clip begin → clip frame ×15 → signal at frame 5 → clip end`，16 tick 完成

## 用法

1. Unity Project 视图找 `Packages/com.vena.blockly/Tests/02_BehaviorRuntime/`
2. 新建一个 scene，挂上述任一 MonoBehaviour 到空 GameObject
3. Play
4. Console 看预期输出；Inspector 看 `Is Playing` / `Ticked Frames`

Inspector 关键字段：`Tick Count`（host 泵几次 `Update`）、`Fixed Delta Time`（默认 `0.016f`）；各 demo 另有 `Condition Input` / `Loop Count Input` / `A Ticks Input` / `B Ticks Input` / `Clip Duration` / `Signal Frame`，于 `Awake()` 读取并烘入 source tree。
