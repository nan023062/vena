# 03 Graph Editor UI

## 测试内容

Editor 期 demo，验证 `GraphAsset` (`Vena.Blockly.Editor.GraphAsset`) ScriptableObject 可被菜单创建并写入占位 JSON。无 scene、无 MonoBehaviour。

- 菜单项 `Tools → Vena → Blockly → Demos → 03 GraphEditorUI → Generate Demo Graph Asset` — 在 `Assets/VenaBlocklyDemos/DemoC_GraphAsset.asset` 产出一个 `GraphAsset`，`_json` 为占位 payload：
  ```json
  {"version":1,"kind":"placeholder","note":"Demo C placeholder — not produced by IBlocklyGraphSerializer."}
  ```

## 用法

1. Unity 工程挂载 `com.vena.blockly`（UPM `file:` 依赖或复制到 `Assets/`）
2. 走菜单 `Tools → Vena → Blockly → Demos → 03 GraphEditorUI → Generate Demo Graph Asset`
3. Inspector 自动 ping 出 `Assets/VenaBlocklyDemos/DemoC_GraphAsset.asset`

> 占位 payload 不是规范 IR JSON，仅供"资产已创建且可写"自检；真正的 IR 由 `IBlocklyGraphSerializer` 基于 `EditorIR.Graph` 产出，超出本 demo 范围。
