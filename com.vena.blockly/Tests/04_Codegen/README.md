# 04 Codegen

## 测试内容

Editor 期 demo，验证 UGC codegen pipeline 能在 demo 本地 source 类 (`InstanceMethod`) 上产出三件套 (Impl + Source + Source.Node) 到 `Tests/04_Codegen/Generated/`。无 scene、无 MonoBehaviour。

- 菜单 `Setup Demo D Config` — 创建 `Assets/VenaBlocklyDemos/04_Codegen/DemoD_CodegenConfig.asset`，`OutputRoot = Packages/com.vena.blockly/Tests/04_Codegen/Generated`，`AssemblyWhitelist = ["Vena.Blockly.Tests.Codegen"]`
- 菜单 `Run Demo D Codegen` — 委托 `UgcCodegenMenu.RunCodegen()` 扫白名单 asmdef，预期产出 `InstanceMethodTestMethod.g.cs`、`InstanceMethodPrintMessage.g.cs`、`GeneratedNodeMetadataProvider.g.cs`（命名空间 `Vena.Blockly.Generated`）

## 用法

1. Unity 工程挂载 `com.vena.blockly`（通常 `Packages/com.vena.blockly/`）
2. 走菜单 `Tools → Vena → Blockly → Demos → 04 Codegen → Setup Demo D Config`
3. 走菜单 `Tools → Vena → Blockly → Demos → 04 Codegen → Run Demo D Codegen`
4. 检查 `Tests/04_Codegen/Generated/` 下三个 `.g.cs` 文件

重跑幂等（内容相同 → 文件不动）。想干净重跑：删 `Generated/` 再走一遍第 3 步。
