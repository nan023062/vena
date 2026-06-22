# 04 Codegen

Editor 期 demo。把 package 自带的 UGC codegen pipeline 跑在一个
demo 本地的 source 类 (`InstanceMethod`) 上，把生成的三件套
(Impl + Source + Source.Node) 输出到 `Tests/04_Codegen/Generated/`。

KD9 强制反例合规：该 demo 完全活在 Editor 侧（菜单项 + ScriptableObject
资产 + `.g.cs` 输出）。没有 sample scene，也没有 MonoBehaviour。

## 如何使用

1. 从一个挂载了 `com.vena.blockly` package 的 Unity 工程打开该 demo
   （通常路径是 `Packages/com.vena.blockly/`）。
2. 菜单 **Tools → Vena → Blockly → Demos → 04 Codegen → Setup Demo D Config**：
   创建 `Assets/VenaBlocklyDemos/04_Codegen/DemoD_CodegenConfig.asset`，
   其中
   - `OutputRoot = Packages/com.vena.blockly/Tests/04_Codegen/Generated`
     （由占位符 `${PackagePath}/Tests/04_Codegen/Generated` 解析得到）
   - `AssemblyWhitelist = ["Vena.Blockly.Tests.Codegen"]`
   - `TypeWhitelist = []`（接受白名单 asmdef 里所有带 `[BlocklyNode]`
     的 source 类）
3. 菜单 **Tools → Vena → Blockly → Demos → 04 Codegen → Run Demo D Codegen**：
   委托给 package 的 `UgcCodegenMenu.RunCodegen()`，它会扫描白名单
   asmdef，为每个 source 类输出 `<SourceName>.g.cs`，并往 Demo D 的
   `OutputRoot` 里输出一个 `GeneratedNodeMetadataProvider.g.cs` 聚合器。

## 你会看到什么

跑完第 3 步之后，`Tests/04_Codegen/Generated/` 里应有：

- `InstanceMethodTestMethod.g.cs` —— `InstanceMethod.TestMethod(int, int)`
  的 Impl + Source + Source.Node 三件套
- `InstanceMethodPrintMessage.g.cs` —— 同上，对应
  `InstanceMethod.PrintMessage(string)`
- `GeneratedNodeMetadataProvider.g.cs` —— 聚合后的
  `INodeMetadataProvider` 实现，注册在命名空间
  `Vena.Blockly.Generated` 下

`InstanceMethod.cs` 本身只保留带 `[BlocklyExportMethod]` /
`[BlocklyExportMember]` 注解的 source 类——之前手写的 `#region Impl`
和 `#region Source` 块（方案 P）已在迁移中移除；codegen 现在会在
`Generated/InstanceMethod*.g.cs` 下再产出那两块。重跑 codegen 是
幂等的（内容相同 → 文件不动）。

## 占位符约定

demo 资产里的 `OutputRoot` 存的是解析后的具体路径。模板形式
`${PackagePath}/Tests/04_Codegen/Generated` 在 `DemoD_CodegenMenu.cs`
里有文档说明，并在 Setup 时解析出来，目的是让该资产可以跨机器分发：
任何人都可以重跑 Setup 来刷新路径。package 里的字段
(`UgcCodegenConfig.OutputRoot`) 是一个纯字符串，本身不原生支持
占位符替换。

## 查看 / 清理生成产物

- `Tests/04_Codegen/Generated/` 存放 codegen 的输出；生成的 `.g.cs`
  文件在文件系统 / git 工作区里都可见。
- 想干净地重跑，删掉 `Generated/` 目录再跑第 3 步即可。
