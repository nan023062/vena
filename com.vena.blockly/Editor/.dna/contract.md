## §0 范围与冻结

- 子模块套入父模块命名空间 `Vena.Blockly`（注解类与运行期接口）；Editor 内部类于 `Vena.Blockly.Editor`。
- 冻结：§1、§2。待冻结：§3§4§5（Phase 2 PR-3 / Phase 2 第二刀进度锁）。不冻结：§6。

## §1 注解语义契约

公共约定：以下所有注解均 `sealed`、`Inherited=false, AllowMultiple=false`（Phase 2 ratchet；codegen 不接受被子类化的注解）。

本轮注解集收 1 件到 4 件：Source 族 2 件 + codegen 族 1 件（multi-target） + 编辑器校验族 1 件。

### `[BlocklySource(menuPath, nodeType)]`

| 属性 | 值 |
|------|------|
| AttributeUsage | `AttributeTargets.Class` |
| Inherited | `false` |
| AllowMultiple | `false` |
| sealed | 是 |
| 字段语义 | `MenuPath:string` = 编辑器调色板菜单路径，允许含 `/` 分层（如 `"测试对象/加法"`），也允许不含 `/` 作为顶层 displayName（如 `"表达式图"`）；**原值透传**至 NodeMetadata，UI 端自行切分。`NodeType:Type` = 该 source 对应的 `ILogicNode` / `IBehaviorNode` 实现类（通常是 source 类嵌套的 `Node`/`Block` 子类，可能是 codegen 产出产物）。 |
| codegen 处理 | 识别为 Blockly 源项、入扫描集；如该类需 codegen 产出三件套（§2），nodeType 填入生成产物的 `Node` 型。 |
| 提醒 | 字段名与 Runtime 源码 `BlocklySourceAttribute.MenuPath / NodeType` 对齐；Runtime 定义不动、契约措辞向源码看齐。 |

### `[BlocklySourceSlot(displayName, order:int)]`

| 属性 | 值 |
|------|------|
| AttributeUsage | `AttributeTargets.Property \| AttributeTargets.Field` |
| Inherited | `false` |
| AllowMultiple | `false` |
| sealed | 是 |
| 字段语义 | `displayName` = UI 字段名；`order:int` = **全项顺序、三者一致、不允许任何脱钩**——同时锁住（·order 升序·轴）：（1）参数 Push 顺序（`EvaluateChildren()` 逐项 `_field.Evaluate()` 依序 Push）、（2）IR 序列化字段顺序（存图字段排列）、（3）编辑器 UI 显示顺序（面板字段从上到下）。**Pop 顺序 ≠ 以上三者**：运行期 Pop = Push 顺序的反序（栈 LIFO，按 order 降序）。 |
| codegen 处理 | 按 `order` 升序输出三者一致顺序（UI / IR / Push）、Pop 反序；breaking。同一 source 内 `order` 重复 → codegen 报错、不产出。 |

### `[Blockly(displayName)]` / `[Blockly(displayName, isStatic, params parameterNames)]`

**本轮新增：合并原 `[BlocklyClass]` / `[BlocklyMethod]` / `[BlocklyProperty]` 为单个 multi-target attribute**。

| 属性 | 值 |
|------|------|
| AttributeUsage | `AttributeTargets.Class \| AttributeTargets.Method \| AttributeTargets.Property \| AttributeTargets.Field` |
| Inherited | `false` |
| AllowMultiple | `false` |
| sealed | 是 |
| 类名 | `BlocklyAttribute`（C# 中代码使用形式 `[Blockly(...)]`） |
| 构造函数 | `BlocklyAttribute(string displayName)` 与 `BlocklyAttribute(string displayName, bool isStatic, params string[] parameterNames)` 二重载 |
| 字段语义 | `DisplayName:string` = UI 显示名（所有 target 通用）；`IsStatic:bool` = 仅 Method target 有意义（生成静态 / 实例胶水分路）；`ParameterNames:string[]` = 仅 Method target 有意义（参数顺序与取名，必须与方法签名参数位置严格一致）。非 Method target 上 `IsStatic` / `ParameterNames` 由 scanner 忽略（不报错、不读取）。 |
| codegen 处理 | scanner 以 `MemberInfo.MemberType` 分支：<br/>• **Class** = 宿主类入 codegen 扫描集；`displayName` 参与产物 `*Source` 上 `[BlocklySource]` 的 `menuPath` 拼接（§2）。<br/>• **Method** = 生成三件套（§2）；Impl 采用 0-arity `IFunctionImpl<TOutput>` / `IProcedureImpl`（静态 N 参 → N 个字段 + 1 个 `instance` 字段只在 `IsStatic=false` 时生成）。Pop 路径语义以 §2 为准。<br/>• **Property** = 生成 getter/setter 两个三件套（按读写可访问性裁剪）；getter 走 `IFunctionImpl<TOutput>`、setter 走 `IProcedureImpl<T>`。<br/>• **Field** = 同 Property（读写都生）。 |
| 硬约束 Q1（§2 锁） | 同一类上 `[Blockly]` 不允许与 `[BlocklySource]` 同存；`[Blockly]` 不允许打在「runtime 节点源类」上（继承自 `Vena.Blockly.Expression` / `Vena.Blockly.BehaviorNodeSource` / `Vena.Blockly.Block<TSource>` / `Vena.Blockly.BehaviorNode<TSource, TImpl>` 的类不可携）。违者 scanner 抛 `InvalidOperationException` 中止整个 codegen run。 |

### `[ExpressionSignature]` / `[ExpressionSignature(returnType)]` / `[ExpressionSignature(returnType, params Type[])]`

（不动，字段与上轮一致。）

| 属性 | 值 |
|------|------|
| AttributeUsage | `AttributeTargets.Field \| AttributeTargets.Property` |
| Inherited | `false` |
| AllowMultiple | `false` |
| sealed | 是 |
| 字段语义 | 三重载：（1）无参 = **占位形态**，接受任意签名；（2）`returnType` = 锁返回型、参数任意；（3）`returnType + params Type[]` = 返回型 + 参数型列表全锁。 |
| codegen 处理 | 无参形态 = 跳过签名校验；有参形态 = 在连接期校验 LogicGraph 返回型 / 形参型，不匹配 → 报错。 |

### 废除注解清单

以下注解本轮本波变更中删除，不再出现于词汇表及任何 codegen 路径：

| 废除 | 取代 |
|------|------|
| `[BlocklyCodeGen]` | `[Blockly]`（Class target） |
| `[BlocklyCodeGenMethod]` | `[Blockly]`（Method target） |
| `[BlocklyCodeGenMember]` | `[Blockly]`（Property / Field target） |
| 上轮 ContextPack 中的 `[BlocklyClass]` | `[Blockly]`（Class target）——未及落盘、本轮直接跳过中间态 |
| 上轮 ContextPack 中的 `[BlocklyMethod]` | `[Blockly]`（Method target）——同上 |
| 上轮 ContextPack 中的 `[BlocklyProperty]` | `[Blockly]`（Property / Field target）——同上 |
| `[BlocklyGenerated]` | 无取代。产物身份走**命名空间隔离**（`<SourceNs>.Generated`，§2）、不再使用 attribute marker。 |

## §2 codegen 输出格式契约

范围锁：针对携 `[Blockly]` 且 target = Method / Property / Field 的被扫描成员，codegen 产出「三件套」。`[BlocklySource]` 顶层被扫描类不独立产生三件套（其本身即为 Source）。`[Blockly]` 贴在 Class target 上作为扫描入口、不独立产出（其下成员逐个产出）。

### scanner 硬约束（Q1，hard fail）

扫描阶段、在收集到 `[Blockly]` 的类后、`CollectMembers` 之前，`AnnotationScanner` 必须进行以下两项检查，任一命中即抛 `InvalidOperationException` 中止整个 codegen run：

1. **双标检**：`type.GetCustomAttribute<BlocklySourceAttribute>() != null` → 违例，信息 = `"[Vena.Blockly] {type.FullName}: [Blockly] 不允许与 [BlocklySource] 同存"`。
2. **runtime 节点源类检**：检查 `type` 的继承链上是否出现以下任一「runtime 节点根类」——命中即违例：
   - `Vena.Blockly.Expression`（LogicGraph 节点根）
   - `Vena.Blockly.BehaviorNodeSource`（BehaviorGraph 节点根）
   - `Vena.Blockly.Block`（嵌套 Node 实现根——注意 `Block<TSource>` 是泛型类，检查时需某 base 是该泛型定义的造型）
   - `Vena.Blockly.BehaviorNode`（嵌套 BehaviorNode 实现根，同后二同为泛型类）

   错误信息 = `"[Vena.Blockly] {type.FullName}: [Blockly] 不允许打在 runtime 节点源类上（继承自 Expression / BehaviorNodeSource / Block<> / BehaviorNode<,>）"`。

检查实现点 = `AnnotationScanner.Scan` 主循环内「读取 classAttr 后、调用 CollectMembers 前」那一段（现有代码第 57–60 行区间）；runtime 根类集合走常量 `static readonly Type[] s_RuntimeNodeRootTypes = { typeof(Expression), typeof(BehaviorNodeSource), typeof(Block<>), typeof(BehaviorNode<,>) }`、靠 `while (baseType != null)` 递推比对（泛型造型以 `IsGenericType && GetGenericTypeDefinition()` 比对）。

### 产物三件套（每个 [Blockly] Method / Property getter / Property setter / Field getter/setter 产一套）

| 件 | 类型 | 职责 | 生成 |
|---|------|------|------|
| `*Impl` | `class : IFunctionImpl<TOutput>` 或 `IProcedureImpl`（**均 0-arity**） | 纯逻辑容器：按参数/接收者顺序列出 `public` 字段，`Evaluate()` 调目标方法。 | codegen |
| `*Source` | `sealed class : Function<*Impl, TOutput>` 或 `Procedure<*Impl>`（**均 0 泛型 arity**） | 编辑期可见节点源：有 `[BlocklySource(menuPath, typeof(*Source.Node))]` + N 个 `[BlocklySourceSlot(name, order)] public Expression 槽位` 字段。 | codegen |
| `*Source.Node` | `sealed class : Block<*Source>`，嵌套在 `*Source` 内 | 运行期手动 Pop 接线，**严格遵循父合约 §4 的 5 步协议**：`EvaluateChildren()` override 触发所有子节点 `_field.Evaluate()`（按 `[BlocklySourceSlot.order]` **升序**、每项内部 Push 出栈值）；`InitializeProperties(*Impl impl)` **只做字段拷贝**（按 `[BlocklySourceSlot.order]` **降序**、栈 LIFO 反向逐项 `impl.field = Blockly.Pop<T>();`），**不得**含任何 `_child.Evaluate()` 副作用；`Initialize()` 调 `Blockly.CreateBlock(source.槽)` 创子节点；`CleanProperties` 清 impl 引用、`OnDestroy` 释放子节点。Procedure 形态省 Push（Function 形态由基类 `Function<*Impl,TOutput>` 在第 5 步统一 Push 返回值）。 | codegen |

> **5 步铁律**（与父合约 §4 对齐）：`EvaluateChildren → Pop → InitializeProperties → Impl.Evaluate → Push`（Procedure 无 Push）。codegen 产出 `*Source.Node` 模板必须按此 5 步切分；`InitializeProperties` 写入 `_field.Evaluate()` = 破坏合约（撞空栈）。**Push 与 Pop 顺序必须相反**（Push 升序、Pop 降序，栈 LIFO），同序 = 撞类型 / 撞错位、同样是合约违反。执行点 = `Editor/Codegen/CodeWriter.cs` 内 `EmitSourceNode`（或同义产出方法）的 Node body 模板。

### 产物命名空间（hard rule）

- **三件套命名空间**：对源类 `T`（`T.Namespace = <SourceNs>`），所有产物三件落于 `namespace <SourceNs>.Generated { ... }`。例源类 `Vena.Blockly.Tests.Codegen.InstanceMethod` → 产物 `namespace Vena.Blockly.Tests.Codegen.Generated`；产物类全限定名始于 `Vena.Blockly.Tests.Codegen.Generated.`。
- **Provider 产物命名空间**：GeneratedNodeMetadataProvider 跨源类聚合、不属于任一 `<SourceNs>`；固定命名空间 `Vena.Blockly.Generated`（根 ns，不接后缀）。仅 Provider 使用该根。
- **产物身份判据**：人类 / IDE / grep / stack trace 完全依赖「命名空间末段 ≡ `.Generated`」辨识；**不再贴 `[BlocklyGenerated]` attribute**（该 attribute 本轮废除）。
- **跨产物引用**：业务代码引用产物需 `using <SourceNs>.Generated;` 或全限定名引用。

### 产物顶部头部

> 产物文件头由本节「输出位置与文件划分」段尾 hard rule 锁定（精确 7 行 prelude + 1 行空行），本节仅约束产物 body 形态。

- `using` 区：`using Vena.Blockly;` + 必要的原名名字空间。
- `namespace` 区：`namespace <SourceNs>.Generated { ... }`（本节「产物命名空间」段锁定）。
- 三件均 `sealed`；`*Source` 贴 `[BlocklySource]`、`*Source.Node` 嵌套在 `*Source` 内部。**三件均不贴 `[BlocklyGenerated]`**（该 attribute 本轮废除）。

### 命名规则

- `*Impl` 名：`<源类简名><成员名>Impl`，**字面拼接、不做 trailing 裁剪**。
  - 例：源类 `InstanceMethod` 的 `TestMethod` → `InstanceMethodTestMethodImpl`。
  - Property setter / getter 划分：`<源类简名><属性名>Getter` / `<源类简名><属性名>Setter`（同样字面拼接、不裁剪）。
- `*Source` 名：`<源类简名><成员名>`，字面拼接。
  - 例：源类 `InstanceMethod` 的 `TestMethod` → `InstanceMethodTestMethod`。全限定名 `<SourceNs>.Generated.InstanceMethodTestMethod`。
- 嵌套 `Node` 不变：`*Source.Node`（嵌在 `*Source` 内、名字字面为 `Node`）。
- `[BlocklySource]` 第一参 `menuPath` = `$"{Blockly(class).DisplayName}/{Blockly(method).DisplayName}"`（宿主类 `[Blockly]` 未标 → 取源类名）。Property / Field 同样拼接。顶层手写项走原生 `"表达式图"` 这种不带 `/` 的 displayName，不受 codegen 控制。
- **命名冲突**：同一程序集已有同名类型 → codegen **报错、不产出**；不静默追加后缀 / 不重命名。跨源类的同名类型由「命名空间隔离」（`<SourceNs>.Generated`）自动消解、不触发 codegen 报错。
- 用户在 `[Blockly("...")]` 给出的 `displayName` **不参与类型名**；仅用于 UI 显示与 `menuPath` 拼接。

### Pop 顺序（§1 `BlocklySourceSlot.order` 实例化规则）

- N 个槽位 → N 个 `_field` 节点 → 在 `EvaluateChildren()` 中 **按 order 升序** 逐项 `_field.Evaluate()`（每项内部 Push 一次值到栈）；`InitializeProperties(impl)` 中 **按 order 降序**（栈 LIFO 反向） `impl.field = Blockly.Pop<T>();`（**仅字段拷贝、无 Evaluate 副作用**）。
- **栈语义**：`EvaluateChildren()` 按 order 升序 Push（最后入栈的是最高 order 的字段值）；`InitializeProperties` 按 order 降序 Pop（最后 Push 的最先 Pop）。两个方向相反才能让 Pop 出来的值 / 类型对齐字段——Push 与 Pop 同序 = 撞类型 / 撞错位、合约违反。
- order 全项锁住三者一致（§1）：**Push 顺序 ≡ IR 字段顺序 ≡ UI 顺序**（按 `[BlocklySourceSlot.order]` 升序）；**Pop 顺序 = Push 顺序的反序**（栈 LIFO，按 order 降序）。
- order 重复 / 跨 source 跨序 → codegen 报错、不产出。

### `isStatic` 分路（仅 [Blockly] Method target）

- `isStatic=true` → `*Impl` 仅含 N 个参数字段；Source 仅含 N 个 `Expression` 槽；order = 参数顺序。
- `isStatic=false` → `*Impl` 额外含首字段 `public <SourceType> instance;`；Source 额外含首槽 `[BlocklySourceSlot("实例", 1)] public Expression instance;`；用户的参数 order 从 2 起计。
- **Push / Pop 顺序**：Push 按 order 升序（`EvaluateChildren()` 逐项：instance 首 Push、参数按名次依序 Push）；Pop 按 order **降序**（栈 LIFO 反向：最后 Push 的参数先 Pop、instance 最后 Pop）。`isStatic=false` 下 instance 是最先 Push、最后 Pop。

### 输出位置与文件划分

- **三件套落点（per-source-class）**：`<SourceDir>/Generated/<源类名>.g.cs`。`<SourceDir>` = 源类 `.cs` 文件所在物理目录、由 scanner 从 `Type.Assembly.Location` + asmdef 路径解析出。例：`Tests/04_Codegen/Scripts/InstanceMethod.cs` → `Tests/04_Codegen/Scripts/Generated/InstanceMethod.g.cs`。
- **文件划分**：**一个被扫源类**（如 `InstanceMethod`）产出**一个 `<源类名>.g.cs`**，包含该源类所有成员的三件套。
- **Provider 产物落点**：`GeneratedNodeMetadataProvider.g.cs` 独立一个文件、跨源类聚合；落点 = `CodegenConfig.OutputRoot`（默认 `${PackagePath}/Runtime/Generated/GeneratedNodeMetadataProvider.g.cs`）。Provider 不随源类走 `<SourceDir>/Generated/`。
- **`CodegenConfig.OutputRoot` 路径语义**（退化为 Provider 专用）：该字段接受带占位符的路径模板，codegen 运行期解析。
  - 占位符 `${PackagePath}` = **包根绝对路径**（`BlocklyCodegenConfig.asset` 所在 UPM 包的根目录绝对路径）。占位符仅限在 `OutputRoot` 字段使用，不进入产物文本。
  - 默认语义：未填 `OutputRoot` 或填空 → Provider 落点 `${PackagePath}/Runtime/Generated/GeneratedNodeMetadataProvider.g.cs`。
  - 允许另存：`OutputRoot` 可被包内其他消费者指向非默认位置，需以 `${PackagePath}/` 开头以保包内锁定；codegen **拒绝**任何解析后发生包外的路径——报错、不产出。
  - 占位符只开一个：不接受 `${ProjectPath}` / `${AssetPath}` / `${UnityDataPath}` 等任何其他变量；如需扩展另升 §2 版本。
- **三件套不再使用 OutputRoot**：原「三件套落 `OutputRoot/<源类名>.g.cs`」废除；三件套落点完全由 `<SourceDir>` 推导。

**.g.cs 产物文件头**（hard rule）：

每个 `.g.cs` 文件首部必须有以下精确 7 行注释 + 1 行空行 + 文件正文（共8 行 prelude）：

```csharp
// -----------------------------------------------------------------------------
// <auto-generated>
//   Generated by the com.vena.blockly Blockly codegen pipeline.
//   Do not edit manually. Changes will be overwritten on next codegen run.
// </auto-generated>
// -----------------------------------------------------------------------------
```

执行点 = `Editor/Codegen/CodeWriter.cs` 内 `EmitSourceFile` / `EmitProviderFile` 等所有产物方法的首行写入器；偏离此格式（缺标签、改字面、改边线长度） = 破坏合约。

理由：
1. `<auto-generated>` 标签是 .NET / Roslyn 分析器约定，让 IDE 与 CI 静态检查跳过 `.g.cs`，避免假阳性 lint 警告污染开发期反馈。
2. "com.vena.blockly Blockly codegen pipeline" 显式标注产出方（包名 + 工具名），业务方 grep 即可定位。**不提 `UGC`**：`UGC` 在本项目语义中是稳定产品概念（KD#2 runtime UGC 玩家编辑器）、不同时作为 codegen 工具身份字样。
3. "Do not edit manually" 提醒手改无效（codegen 重跑会覆盖）；与 `WriteIfChanged` 幂等机制配套。
4. 7 行边线包注释格式与包内 .cs 版权头视觉对齐，不引入第二种风格。

`.g.cs` **不带** com.vena.blockly 版权头（`// Vena Blockly` / `// Visual scripting...`），因为 codegen 产物形态与手写代码身份分离：手写代码挂版权头声明所有权；产物挂 `<auto-generated>` 标签声明“这不是手写、勿当源码维护”。两类头不叠加。

### 重生产 SOP（hard rule）

codegen 变更（套锁合约调整 / 命名空间调整 / 三件套模板调整 / 注解名合并等）后重跑 codegen 前必须执行：

1. 全局检索包内 `**/Generated/*.g.cs` 产物文件。
2. 全部删除（同时删 `.g.cs.meta`）。
3. 重跑 `Tools/Vena/Blockly/Run Codegen`。

理由：不先删可能出现「旧 ns 产物 + 新 ns 产物」共存，同名类型在两个 ns 中同时存在会导致 Unity 编译失败且修复路径不直观。本轮同时发生 attribute 名合并（`[BlocklyCodeGen*]` → `[Blockly]`），旧产物仍引用旧 attribute、也会导致编译失败——同样必须清理后重跑。`WriteIfChanged` 仅保证同路径幂等、不清理路径变更后的遗留产物。


## §2.5 Path B / Path C 分支产物契约（PR-2 锁 Path B；PR-3 补 Path C）

本节锁 Path B / Path C 二件套产物契约。PR-2 锁 Path B（`IBehaviorImpl`）、PR-3 锁 Path C（`IClip`）。两路径镜像对称，共享 scanner 字段镜像逻辑、`ComputeDefaultMenuPath` 、字段类型双轨锁；emitter 字面独立。

### scanner 分支识别契约

`AnnotationScanner.Scan` 主循环在同一个 `Type` 上按如下顺序交互互斥判断（实现处 = `AnnotationScanner.cs` 主循环内部，Path B 已在 PR-2 落地、Path C 在 PR-3 补）：

1. **预过滤**：`type == null || !type.IsClass || type.IsAbstract` → 跳过；`typeFilter` 不命中 → 跳过；`type.GetCustomAttribute<BlocklySourceAttribute>() != null` → 跳过。
2. **Path A 识别**：`type.GetCustomAttribute<BlocklyAttribute>() != null` → 走 Path A 原有分支（Q1 检查 + `CollectMembers` 三件套产出）。未命中 Path A → 继续判 Path B。
3. **Path B 识别**：`typeof(IBehaviorImpl).IsAssignableFrom(type)` → 走 Path B 分支：调 `CollectImplSlots(type)` 收 Impl 类上的 `[BlocklySourceSlot]` 字段集（含字段类型校验）、调 `ComputeDefaultMenuPath(type)` 推 menuPath、产 `ScannedSource` 二件套输入项（`Kind = Behavior`）。`continue` 本轮 Type。
4. **Path C 识别**（PR-3）：`typeof(IClip).IsAssignableFrom(type)` → 走 Path C 分支：调 `CollectImplSlots(type)`（与 Path B 共享）收 Impl 类上的 `[BlocklySourceSlot]` 字段集、调 `ComputeDefaultMenuPath(type)` 推 menuPath、产 `ScannedSource` 二件套输入项（`Kind = Timeline`）。`continue` 本轮 Type。
5. **都未命中** → 该 type 不是 codegen 输入、`continue`。

**互斥语义**：Path A 与 Path B / Path C 同一 type 不可能同时命中——Path A 要求贴 `[Blockly]` Class target 且不是 runtime 节点根类；Path B 要求实现 `IBehaviorImpl`；Path C 要求实现 `IClip`。`IBehaviorImpl` 与 `IClip` 在 `Runtime/Behavior/BehaviorNode.cs:54` 与 `Runtime/Behavior/Timeline/Clip.cs:101` 独立定义、互不继承 → Path B 与 Path C 同一 type 也不可能同时命中。**Path B / Path C 不读 `[Blockly]` attribute**：Impl 类上贴 `[Blockly]` 被分支完全忽略。

**Q1.b 互斥硬检验位留**：业务类同时从 runtime 根类派生（`BehaviorNode<,>` / `Clip<,>`）且实现 `IBehaviorImpl` / `IClip` 是未定义语义，未来 PR-γ 以 `InvalidOperationException` 收口。本 PR-3 silent ignore。

### lifecycle 方法 silent ignore

- **Path B (`IBehaviorImpl`) lifecycle 名锁**：`Start` / `Tick` / `LateTick` / `Finish`（`Runtime/Behavior/BehaviorNode.cs:54-63`）。Impl 类上「成员级 `[Blockly]` 并存」场景下，lifecycle 名称上的 `[Blockly]` silent ignore。名单锁点 = `AnnotationScanner.s_BehaviorLifecycleMethodNames`。
- **Path C (`IClip`) lifecycle 名锁**：`Begin` / `OnFrame` / `End`（`Runtime/Behavior/Timeline/Clip.cs:101-106`）。Impl 类上「成员级 `[Blockly]` 并存」场景下，lifecycle 名称上的 `[Blockly]` silent ignore。名单锁点 = `AnnotationScanner.s_ClipLifecycleMethodNames`（PR-γ 需要则补；本 PR-3 不必落代码，仅设计位占）。Q1 hard fail 升级留给 PR-γ。

### Path B / Path C 二件套产物模板契约（Scenario Y）

#### 字段类型双轨锁（Path B 与 Path C 共享）

- **Impl 类字段（业务侧手写）**：**必须**实际值类型——基元值型（`bool` / `int` / `float` / `string` / `enum` 等）、结构体（`Vector3` / `Quaternion` / `FrameInfo` 等）、或 `UnityEngine.Object` 派生引用（`GameObject` / `Transform` / `Material` / `Component` 等）。**禁止** `Vena.Blockly.LogicGraph`、**禁止** `Vena.Blockly.Expression`。
- **`*Source` 类字段（codegen 产物）**：**统一为** `Vena.Blockly.LogicGraph`，与 Impl 字段类型无关、无例外。Path B 位于 `BehaviorNodeSource<*Impl>`、Path C 位于 `ClipSource<*Impl>`。

#### scanner 字段类型校验（hard fail，Path B / Path C 共享）

实现点 = `AnnotationScanner.CollectImplSlots(Type)`（PR-3 将 PR-2 落地的 `CollectBehaviorSlots` rename）。该方法返回 `IReadOnlyList<ImplSlotInfo>`（PR-3 将 PR-2 `BehaviorSlotInfo` rename），每项含 `FieldName / DisplayName / Order / FieldValueType`。收集 Impl 类上贴 `[BlocklySourceSlot]` 的字段集时，对每个字段做校验，任一命中即抛 `InvalidOperationException` 中止整个 codegen run：

1. `field.FieldType == typeof(Vena.Blockly.LogicGraph)` → `"[Vena.Blockly] {Impl.FullName}.{field.Name}: Impl 字段不允许声明 LogicGraph 类型；请声明实际值类型（codegen 自动生成 LogicGraph 槽位、Init 时 Call<T>() 求值）。"`
2. `typeof(Vena.Blockly.Expression).IsAssignableFrom(field.FieldType)` → `"[Vena.Blockly] {Impl.FullName}.{field.Name}: Impl 字段不允许声明 Expression 类型（Behavior / Timeline 侧无 LogicGraph 5 步协议栈，依赖其求值会撞空栈）。"`

错误信息中「Impl 字段」字样保持不变——Path B / Path C 实体语义仍是「Impl 类上的字段」，重命名仅限 scanner / `ScannedSource` 级别、不浪费新词汇。

#### Path B 产物主体模板（不动，PR-2 锁）

```csharp
[BlocklySource("<computed-menuPath>", typeof(<SourceName>.Node))]
public sealed class <SourceName> : BehaviorNodeSource<<ImplName>>
{
    [BlocklySourceSlot("<字段显示名>", <order>)]
    public LogicGraph <fieldName>;
    // … 其余 slot 镜像字段，全部 LogicGraph 类型 …

    sealed class Node : BehaviorNode<<SourceName>, <ImplName>>
    {
        private LogicGraph.Blockly _<fieldName>;
        // … 每 slot 一个 …

        protected override void Initialize()
        {
            _<fieldName> = blockly.CreateBlockly(source.<fieldName>);
            // … 逐 slot …
        }

        protected override void InitializeProperties(<ImplName> impl)
        {
            impl.<fieldName> = _<fieldName>.Call<<FieldValueType>>();
            // … 逐 slot …
        }

        protected override void CleanProperties(<ImplName> impl)
        {
            impl.<fieldName> = null;  // 仅引用型
            // … 值型跳过不输出 …
        }

        protected override void OnBeforeDestroy()
        {
            blockly.DestroyBlockly(_<fieldName>);
            _<fieldName> = null;
            // … 逐 slot …
        }
    }
}
```

#### Path C 产物主体模板（PR-3 锁）

```csharp
[BlocklySource("<computed-menuPath>", typeof(<SourceName>.Node))]
public sealed class <SourceName> : ClipSource<<ImplName>>
{
    [BlocklySourceSlot("<字段显示名>", <order>)]
    public LogicGraph <fieldName>;
    // … 其余 slot 镜像字段，全部 LogicGraph 类型 …

    sealed class Node : Clip<<SourceName>, <ImplName>>
    {
        private LogicGraph.Blockly _<fieldName>;
        // … 每 slot 一个 …

        protected override void Initialize()
        {
            _<fieldName> = timeline.blockly.CreateBlockly(source.<fieldName>);
            // … 逐 slot …
        }

        protected override void InitializeProperties(<ImplName> impl)
        {
            impl.<fieldName> = _<fieldName>.Call<<FieldValueType>>();
            // … 逐 slot …
        }

        protected override void CleanProperties(<ImplName> impl)
        {
            impl.<fieldName> = null;  // 仅引用型
            // … 值型跳过不输出 …
        }

        protected override void OnBeforeDestroy()
        {
            timeline.blockly.DestroyBlockly(_<fieldName>);
            _<fieldName> = null;
            // … 逐 slot …
        }
    }
}
```

#### Path C 与 Path B 的字面差异锁（三点）

1. **基类**：Path C `*Source : ClipSource<*Impl>` / `Node : Clip<*Source, *Impl>`；Path B `*Source : BehaviorNodeSource<*Impl>` / `Node : BehaviorNode<*Source, *Impl>`。
2. **`timeline.blockly` 属性 hop**：Path C 的 `Initialize` / `OnBeforeDestroy` 中 `blockly` 访问走 `timeline.blockly.CreateBlockly(...)` / `timeline.blockly.DestroyBlockly(...)`。原因：`Clip<TSource>` 基类（`Runtime/Behavior/Timeline/Clip.cs:112-171`）只暴露 `public Timeline timeline { get; private set; }` 属性、无直接的 `blockly` 字段；`Timeline` 本身继承自 `CompositeBehavior<TimelineSource>` → `BehaviorNode<,>`，`public BehaviorGraph.Blockly blockly { get; }` 属性从该层补供。Path B 的 `blockly` 是 `BehaviorNode<,>` 的直接字段、无需 hop。
3. **生命周期调用点**：Path B `InitializeProperties / CleanProperties` 由 `BehaviorNode<,>.Tick` 驱动、每调 Tick 重 reinit；Path C `InitializeProperties / CleanProperties` 由 `Clip<TSource, TImpl>` sealed override `OnBegin / OnEnd` 驱动（`Runtime/Behavior/Timeline/Clip.cs:179-194`），仅在 Clip Begin / End 召一次。产物字面相同、仅调用时机不同（对 codegen 透明，无需在产物中体现）。

#### Path C 调用点锁

- **`Initialize`** 调用点 = `Clip<TSource>.ITimelineClip.OnCreate`（`Runtime/Behavior/Timeline/Clip.cs:122-132`）。调用时事件顺序 = `OnCreate` 赋 `timeline` / `source` 后最后调 `Initialize()`。
- **`InitializeProperties(impl)`** 调用点 = `Clip<TSource, TImpl>.OnBegin`（`Clip.cs:179-183`），调用时事件顺序 = `InitializeProperties(_impl)` → `_impl.Begin()`。
- **`CleanProperties(impl)`** 调用点 = `Clip<TSource, TImpl>.OnEnd`（`Clip.cs:190-194`），调用时事件顺序 = try `_impl.End(frameInfo)` finally `CleanProperties(_impl)`。
- **`OnBeforeDestroy`** 调用点 = `Clip<TSource>.ITimelineObject.OnDestroy`（`Clip.cs:159-164`），调用时事件顺序 = `OnBeforeDestroy()` → `timeline = null; source = null;`。
- **`OnFrame`** 不出现于产物中。`Clip<TSource, TImpl>.OnFrame` sealed override 直接调 `_impl.OnFrame(frameInfo)`（`Clip.cs:185-188`）。Path C 产物 Node 如误写 `protected override void OnFrame(...)` 会因「sealed」编译失败。

#### 不产件 / 不出现（Path B / Path C 共同）

- 不产 `*Impl`（`*Impl` = 业务侧手写的 Impl 类本身）。
- 不出现 `EvaluateChildren` / `Blockly.Pop<T>` / `Blockly.CreateBlock` / `Blockly.DestroyBlock` 调用。Behavior / Timeline 侧 lifecycle 与 LogicGraph Push/Pop 栈完全无关。`CreateBlockly` / `DestroyBlockly` / `Call<T>()` 是 `BehaviorGraph.Blockly` 自洽 API。
- 不实现 Path C 产物中的 `OnFrame`（`Clip<TSource, TImpl>` 已 sealed）。

#### 黄金样本

- Path B：`Tests/02_BehaviorRuntime/Scripts/HelloBehaviorImpl.cs` 与 `Tests/02_BehaviorRuntime/Scripts/Generated/HelloBehaviorImpl.g.cs`。Impl `greeting: string`、Source `greeting: LogicGraph`、Node `_greeting: LogicGraph.Blockly`。
- Path C（PR-3 补）：`Tests/02_BehaviorRuntime/Scripts/HelloClipImpl.cs` 与 `Tests/02_BehaviorRuntime/Scripts/Generated/HelloClipImpl.g.cs`。Impl `greeting: string`、Source `greeting: LogicGraph`、Node `_greeting: LogicGraph.Blockly`。Path C 黄金样本补齐后，emitter 输出与该样本字面一致即正确。

#### Provider 聚合

Path B / Path C 的 `*Source` 同贴 `[BlocklySource]` → 被 `GeneratedNodeMetadataProvider` 聚合：`nodeType: typeof(<SourceName>.Node)`、`properties: NodePropertyMetadata[]`（每个 slot 一项，`fieldName / order / displayName` 镜像自 `[BlocklySourceSlot]`、`Type` = `LogicGraph`）、`menuPath: "<默认 menuPath>"`。Path C `case ScannedSourceKind.Timeline:` 在 Provider 迭代中位位对称 Path B、调 `AppendTimelineNodeMetadataCtor`。

### `ComputeDefaultMenuPath(Type)` 锁点（不动）

`AnnotationScanner.ComputeDefaultMenuPath(Type type)` 静态方法，供 Path B / Path C 共享调用。Path A 不调。算法不变（已在 PR-2 落地）：去 `Impl` 后缀 → 剥 `Vena.Blockly.` 前缀 → `.` 折叠为 `/`。

**例证**：
- `Vena.Blockly.Tests.BehaviorRuntime.HelloBehaviorImpl` → `Tests.BehaviorRuntime/HelloBehavior`（Path B 黄金样本验证）。
- `Vena.Blockly.Tests.BehaviorRuntime.HelloClipImpl` → `Tests.BehaviorRuntime/HelloClip`（Path C 黄金样本预期）。

**不提供覆盖**：业务侧无法通过 attribute / config / hook 覆盖 menuPath。本 PR-3 锁「默认 menuPath 由命名空间推」。

### `ScannedSource.Kind` 与镜像字段命名

本 PR-3 以 PR-2 落地的 `ScannedSource` 为基础，做如下修订：

- **`Kind` 枚举不变**：`Logic` / `Behavior` / `Timeline`。Path A → Logic；Path B → Behavior；Path C → Timeline。
- **镜像字段 rename**：Path B / Path C 共享镜像字段集，PR-2 落地名 `BehaviorSlots / BehaviorMenuPath` · `BehaviorSlotInfo` rename 为 `ImplSlots / MenuPath` · `ImplSlotInfo`。重命名为一次性动作、以适配 Path C 同名镜像语义（C5 复用：Path B / Path C 字段类型校验、`[BlocklySourceSlot]` 读取、order 升序逻辑完全同构，不应为每个 Path 重设一套同构字段）。`ImplSlotInfo` 四字段不变：`FieldName / DisplayName / Order / FieldValueType`。
- **`ScannedMember` 集**：仅 Path A 填；Path B / Path C 不填。
- **`SourceDirectory` 字段**：三路径共享，走 `ResolveSourceDirectory(type)`。

**场景 ζ 产物合并规则**：同一 Impl 类同时命中 Path B / Path C（接口实现）与「成员级 `[Blockly]` 标」时，`ScannedSource` 产出两项：Kind=`Behavior`/`Timeline` 产 二件套 + Kind=`Logic` 产 Path A 三件套（以成员为颗粒）。两者同资源 .g.cs、产物列表拼接输出。`CodeWriter.Emit` 迭代 `ScannedSource[]` 时以 `SourceType` 分组；同一 `SourceType` 下多 `Kind` 顺序决定：先 Behavior / Timeline（二件套主体）后 Logic（三件套、多成员）。

### CodeWriter 路由与 Provider 聚合

`CodeWriter.Emit` 主循环的 `switch (src.Kind)` 补 Path C 分支：

```csharp
switch (src.Kind)
{
    case ScannedSourceKind.Logic:    content = EmitSourceFile(src); break;
    case ScannedSourceKind.Behavior: content = EmitBehaviorSourceFile(src); break;
    case ScannedSourceKind.Timeline: content = EmitTimelineSourceFile(src); break;
    default: throw new InvalidOperationException($"Unknown Kind: {src.Kind}");
}
```

Provider 聚合 `EmitProviderFile` 内部迭代 `sources` 的 switch 补 Path C：

```csharp
switch (src.Kind)
{
    case ScannedSourceKind.Logic:
        foreach (var m in src.Members) AppendNodeMetadataCtor(sb, src, m, "                ");
        break;
    case ScannedSourceKind.Behavior:
        AppendBehaviorNodeMetadataCtor(sb, src, "                ");
        break;
    case ScannedSourceKind.Timeline:
        AppendTimelineNodeMetadataCtor(sb, src, "                ");
        break;
}
```

Path C `AppendTimelineNodeMetadataCtor` 与 Path B `AppendBehaviorNodeMetadataCtor` 字面几乎同构（Provider 只关心 sourceType / nodeType / menuPath / properties四项、不关心基类）。为避免语义混淆、PR-3 独立命名，不强行合并 helper。`TimelineSourceName(Type)` 与 `BehaviorSourceName(Type)` 同算法、亦独立命名。

## §3 反射识别规则契约

待 Phase 2 PR-3 落地后填。

## §4 IR 序列化格式契约

### §4.1 载体

- IR 落地形态 = `GraphAsset`：`ScriptableObject` + 单字段 `[SerializeField] string _json`。
- Unity 序列化只触发整串 `_json` 字段写盘 / round-trip；不参与字段级 diff、不依赖 Unity 的对象引用图。
- 一个 `GraphAsset` = 一张图（`Behavior` 或 `Logic`）；不同图独立资产、独立 `_json`。

### §4.2 顶层 Schema

JSON 顶层对象固定 5 字段（顺序无关、键名图定）：

| 键 | 类型 | 语义 |
|------|------|------|
| `schema` | `int` | IR 版本号；当前 = 1。breaking 变更必须升版。 |
| `kind` | `"Behavior"` \| `"Logic"` | 图类型；Behavior = 控制图、Logic = 逻辑图（表达式图）。其他取值 → 反序列化报错。 |
| `rootNodeGuid` | `Guid` (string) | 根节点 GUID；Behavior 图 = 入口节点、Logic 图 = 求值终点节点。 |
| `nodes` | `NodeIR[]` | 节点列表，元素结构见 §4.3。 |
| `edges` | `EdgeIR[]` | 边列表，元素结构见 §4.4。 |

顶层不含 `version` / `meta` / `comment` 等附加字段；如需扩展走 §4.6 AOT 不变量与版本升级路径。

### §4.3 Node 字段

`NodeIR` 固定 4 字段：

| 键 | 类型 | 语义 |
|------|------|------|
| `guid` | `Guid` (string) | 节点稳定身份；新建时分配、跨 round-trip 保留（§4.5）。 |
| `sourceType` | `string` (AQN) | 节点 Source 类的程序集限定名（`Type.AssemblyQualifiedName` 的稳定子集：`Namespace.TypeName, AssemblyName`，不含版本/文化/公钥）。 |
| `properties` | `KV[]`（`{key:string, value:json}`） | `[BlocklySourceSlot]` 槽位的字面值或子节点引用；顺序按 `[BlocklySourceSlot.order]` 升序、与 §1 / §2 锁的三者一致原则对齐。 |
| `position` | `Vector2` (`{x:float, y:float}`) | 编辑器画布坐标；运行期忽略、仅供 Editor.UI 复原布局。 |

`properties[].value` 表达子节点引用时 = `{nodeGuid: Guid}` 单字段对象；表达字面值时 = 原始 JSON 标量 / 对象。

### §4.4 Edge 字段

`EdgeIR` 固定 3 字段：

| 键 | 类型 | 语义 |
|------|------|------|
| `from` | `PortRef` (`{nodeGuid:Guid, port:string}`) | 出端。 |
| `to` | `PortRef` (`{nodeGuid:Guid, port:string}`) | 入端。 |
| `wireKind` | `"Control"` \| `"Value"` | **显式无默认**；缺字段 / 其他取值 → 反序列化报错。 |

`port` 字段语义：Control 端口 = 节点声明的具名出/入口（如 `"next"`、`"true"`、`"false"`）；Value 端口 = `[BlocklySourceSlot]` 槽位名（与 §4.3 `properties[].key` 对齐）。

### §4.5 round-trip 不变量

1. **Guid 保留**：`FromJson(json) → ToJson` 后所有 `NodeIR.guid` 与原 json 完全一致；新建节点时分配的 Guid 不因 round-trip 改变。
2. **字节级对称**：`ToJson(FromJson(json)) ≡ canonicalize(json)`。canonical 形态 = 顶层与 NodeIR / EdgeIR 字段固定顺序（§4.2 / §4.3 / §4.4 表内顺序）+ 紧凑分隔符 + UTF-8 无 BOM + 无尾随换行。Editor 写盘前调用 canonicalize；非 canonical 输入允许读、写出必为 canonical。
3. **数组顺序保留**：`nodes[]` / `edges[]` / `properties[]` 顺序 round-trip 等价。`properties[]` 顺序由 `[BlocklySourceSlot.order]` 决定（§4.3）。
4. **未知字段拒绝**：节点 / 边 / 顶层出现表外字段 → 反序列化报错；不静默丢弃。

### §4.6 AOT 不变量

1. `sourceType` 解析仅限 codegen 产物 `*Source`（携 `[BlocklySource]` 且命名空间末段 ≡ `.Generated`）与手写 Source（携 `[BlocklySource]` 且命名空间末段 ≠ `.Generated`）；不允许运行期反射构造任意 `Type`。原「+ `[BlocklyGenerated]`」词汇废除、产物身份走命名空间隔离（§2）。
2. `properties[].key` 必须能在 `sourceType` 的 `[BlocklySourceSlot]` 槽位集合内静态匹配；未匹配 → 反序列化报错（不容错回填默认）。
3. `properties[].value` 字面值类型必须能静态推断（由 `[BlocklySourceSlot]` 字段类型锁定），不允许 `object` / 多态运行期分发。
4. `EdgeIR.wireKind` 与 `from/to.port` 类型在静态期可推断（Control 端口集 / Value 端口集 codegen 期已知），AOT 不引入运行期端口类型字典查找。
5. 反序列化路径不调 `Activator.CreateInstance(Type)`；走 `IBlocklyNodeFactory.Create<T>(IBlocklySource)` + codegen 产出的具型构造，AOT 平台可静态裁剪。

### §4.7 接口

```
interface IBlocklyGraphSerializer
{
    string ToJson(GraphIR ir);
    GraphIR FromJson(string json);
}
```

- 归属：Editor 子模块（`Vena.Blockly.Editor`）、IR 编解码原语；Editor.UI 与 Runtime IR 加载器均消费。
- **不复用** Runtime `IBlocklySerializer`：`IBlocklySerializer` = 字节流原语（host 通用持久化），`IBlocklyGraphSerializer` = IR 结构化编解码，二者输入域 / 输出域 / 变更原因均不同。
- **不进父 §6 聚合门面**：`IBlocklyHost` 不持有 `IBlocklyGraphSerializer` 字段；Runtime 加载器从 `GraphAsset._json` 字符串通过本接口反序列化为 `GraphIR`、再交节点工厂构造。
- 错误模式：所有 §4.2–§4.6 校验失败抛 `BlocklyIRSchemaException`（Editor 子模块定义）；调用方负责定位与上报，不静默修复。

## §5 编辑器入口契约

待 Phase 2 第二刀落地后填。

## §6 错误处理与非冻结声明

### 错误处理

待 Phase 2 PR-2/PR-3 落地后填。

### 非冻结声明

1. codegen 输出文件名与内部类型名（§2）。
2. 反射扫描顺序与缓存策略（§3）。
3. IR 序列化格式与版本字段（§4）。
4. 编辑器菜单路径与提示 UI（§5）。
5. Phase 3 AOT 产物形态与驱动入口（未开始，锁定品后附）。
