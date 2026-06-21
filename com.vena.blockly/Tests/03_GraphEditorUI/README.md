# 03 Graph Editor UI

Editor-期 demo. Generates a minimal `GraphAsset` (`Vena.Blockly.Editor.GraphAsset`)
with a placeholder JSON payload, demonstrating the asset-shape落点 mandated by
KD9 强制反例: Editor demos use Editor菜单 + 资产, **not** sample scene +
MonoBehaviour.

## How to use

1. Open this demo from a Unity project that mounts `Tests/03_GraphEditorUI/`
   (mount the package as a `file:` UPM dep or copy the directory under
   `Assets/`).
2. Use menu: **Tools → Vena → Blockly → Demos → 03 GraphEditorUI →
   Generate Demo Graph Asset**.
3. Inspector will ping the newly created `Assets/VenaBlocklyDemos/DemoC_GraphAsset.asset`.

## What you get

- A `GraphAsset` ScriptableObject at `Assets/VenaBlocklyDemos/DemoC_GraphAsset.asset`
  (folder created on demand).
- The asset's `_json` is a placeholder string:

  ```json
  {"version":1,"kind":"placeholder","note":"Demo C placeholder — not produced by IBlocklyGraphSerializer."}
  ```

  This is **not** canonical IR JSON — it's just a visible payload for the
  demo's "asset is created and writeable" check. Real IR JSON is produced by
  `IBlocklyGraphSerializer` on top of an `EditorIR.Graph` instance, which is
  out of scope for this demo.

## Next steps

- Replace the placeholder with a real `EditorIR.Graph` round-trip once the
  serializer is wired up.
- Add a custom `Editor`-derived inspector that surfaces the JSON and provides
  pretty-print + validate buttons.
- Hook a `GraphView`-based editor window that reads / writes this asset
  (today's UI lives in `Editor/UI/`).
