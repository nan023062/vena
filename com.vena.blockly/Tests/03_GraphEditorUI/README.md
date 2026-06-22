# 03 Graph Editor UI

Editor 期 demo。生成一个最小化的 `GraphAsset`
(`Vena.Blockly.Editor.GraphAsset`)，里面带一段占位 JSON，
演示 KD9 强制反例要求的资产形态落点：Editor demo 用 Editor 菜单 + 资产，
**不用** sample scene + MonoBehaviour。

## 如何使用

1. 从一个挂载了 `Tests/03_GraphEditorUI/` 的 Unity 工程打开该 demo
   （把 package 作为 `file:` UPM 依赖挂载，或把目录复制到 `Assets/` 下）。
2. 走菜单：**Tools → Vena → Blockly → Demos → 03 GraphEditorUI →
   Generate Demo Graph Asset**。
3. Inspector 会 ping 新创建的
   `Assets/VenaBlocklyDemos/DemoC_GraphAsset.asset`。

## 你会得到什么

- 一个位于 `Assets/VenaBlocklyDemos/DemoC_GraphAsset.asset` 的
  `GraphAsset` ScriptableObject（文件夹按需创建）。
- 该资产的 `_json` 是一段占位字符串：

  ```json
  {"version":1,"kind":"placeholder","note":"Demo C placeholder — not produced by IBlocklyGraphSerializer."}
  ```

  这 **不是** 规范的 IR JSON ——它只是一段可见的 payload，
  供该 demo 做"资产已创建且可写"的检查。真正的 IR JSON 由
  `IBlocklyGraphSerializer` 基于一个 `EditorIR.Graph` 实例产出，
  超出本 demo 的范围。

## 后续

- 等序列化器接好之后，把占位 payload 换成真正的 `EditorIR.Graph`
  round-trip。
- 加一个继承自 `Editor` 的自定义 inspector，把 JSON 暴露出来，
  并提供 pretty-print 和 validate 按钮。
- 接一个基于 `GraphView` 的编辑器窗口，读写这份资产
  （今天的 UI 在 `Editor/UI/` 下）。
