# 01 Logic Runtime

## 测试内容

四个 MonoBehaviour 验证 `LogicGraph` 不同能力：

- `LogicRuntimeDemo` — 基础求值，每帧重建 `Add(int,int)` + `Const(float)`；Inspector 实时显示 `Last Int Sum = 7`、`Last Float Value = 1.5`、`Evaluation Count++`
- `NestedExpressionDemo` — 深度嵌套表达式 `((3+4)*(10-6)) + Sum5(1..5)`，预期 `result = 43`
- `ControlFlowDemo` — `LogicSequence` + `LogicBranch` + 共享变量，`if (x>5) result=x*2 else result=x`（x=10），预期 `result = 20`
- `WhileLoopDemo` — `LogicWhile` 累加 `1..10`，预期 `sum = 55`

## 用法

1. Unity Project 视图找 `Packages/com.vena.blockly/Tests/01_LogicRuntime/`
2. 新建一个 scene，挂上述任一 MonoBehaviour 到空 GameObject
3. Play
4. Console 看预期输出，Inspector 看运行期字段更新

`LogicRuntimeDemo` 可在 Play 模式下改 Inspector 的 `Operand A` / `Operand B` / `Float Operand`，下一帧立即反映。

> 注：`ControlFlowDemo` / `WhileLoopDemo` 通过 host 预声明变量到 root scope（`LogicSequence` sibling scope 限制的 workaround；runtime UGC 阶段会改）。
