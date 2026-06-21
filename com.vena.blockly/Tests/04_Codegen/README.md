# 04 Codegen

Editor-期 demo. Drives the package's UGC codegen pipeline against a
demo-local source class (`InstanceMethod`) and emits the generated three-piece
set (Impl + Source + Source.Node) into `Tests/04_Codegen/Generated/`.

KD9 强制反例 compliance: this demo lives entirely on the Editor side
(menu items + ScriptableObject asset + .g.cs emission). No sample scene, no
MonoBehaviour.

## How to use

1. Open this demo from a Unity project that mounts the `com.vena.blockly`
   package (typically `Packages/com.vena.blockly/`).
2. Menu **Tools → Vena → Blockly → Demos → 04 Codegen → Setup Demo D Config**:
   creates `Assets/VenaBlocklyDemos/04_Codegen/DemoD_CodegenConfig.asset` with
   - `OutputRoot = Packages/com.vena.blockly/Tests/04_Codegen/Generated`
     (resolved from the placeholder `${PackagePath}/Tests/04_Codegen/Generated`)
   - `AssemblyWhitelist = ["Vena.Blockly.Tests.Codegen"]`
   - `TypeWhitelist = []` (accept all `[UgcSource]` source classes in the
     whitelisted asmdef)
3. Menu **Tools → Vena → Blockly → Demos → 04 Codegen → Run Demo D Codegen**:
   delegates to the package's `UgcCodegenMenu.RunCodegen()`, which scans the
   whitelisted asmdef, emits `<SourceName>.g.cs` per source class, and emits
   one `GeneratedNodeMetadataProvider.g.cs` aggregator into the Demo D
   `OutputRoot`.

## What you'll see

After running step 3, `Tests/04_Codegen/Generated/` should contain:

- `InstanceMethodTestMethod.g.cs` — Impl + Source + Source.Node 三件套 for
  `InstanceMethod.TestMethod(int, int)`
- `InstanceMethodPrintMessage.g.cs` — same, for `InstanceMethod.PrintMessage(string)`
- `GeneratedNodeMetadataProvider.g.cs` — aggregated `INodeMetadataProvider`
  implementation registered under namespace `Vena.Blockly.Generated`

`InstanceMethod.cs` itself only carries the source class with `[UgcMethod]` /
`[UgcProperty]` annotations — the previously-handwritten `#region Impl` and
`#region Source` blocks (scheme P) have been removed in the migration; the
codegen produces exactly those two blocks again under
`Generated/InstanceMethod*.g.cs`. Re-running codegen is idempotent (same
content → file untouched).

## Placeholder convention

`OutputRoot` in the demo asset is stored as the resolved concrete path. The
template form `${PackagePath}/Tests/04_Codegen/Generated` is documented in
`DemoD_CodegenMenu.cs` and resolved at Setup time to keep the asset shippable
across machines: anyone can re-run Setup to refresh the path. The package
field (`UgcCodegenConfig.OutputRoot`) is a plain string and does not natively
support placeholder substitution.

## Inspecting / clearing generated artefacts

- `Tests/04_Codegen/Generated/` holds the codegen output; generated `.g.cs`
  files are visible in the file system / git working tree.
- To regenerate cleanly, delete the `Generated/` directory and run step 3 again.
