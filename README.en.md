# Vena

Vena — the union of Wei (薇) and Nan (南).

Vena is a lightweight Unity game framework monorepo containing 4 independent UPM packages. Migrated from the vena-core / vena-framework source repositories.

## Package Layout

```
vena/
├── com.vena.core             Core: ECS primitives, collections, pool, profiler, FlowGraph, HierarchicalFsm
├── com.vena.math             Math: vectors, matrices, quaternion, AABB, ray, transformation
├── com.vena.framework        Framework: GameWorld, State, Module, Character, Pawn, Scene, GUI
└── com.vena.unity-extensions Unity helpers: NodeReferences, extension methods, editor tools
```

## Dependency Graph

```
com.vena.core           (no dependencies)
com.vena.math           (no dependencies)
com.vena.unity-extensions (no dependencies)
com.vena.framework      → com.vena.core
                        → com.vena.unity-extensions
                        → com.unity.ugui
```

## Local Installation (file: reference)

In your Unity project's `Packages/manifest.json`:

```json
{
  "dependencies": {
    "com.vena.core":             "file:../../com.vena.core",
    "com.vena.math":             "file:../../com.vena.math",
    "com.vena.unity-extensions": "file:../../com.vena.unity-extensions",
    "com.vena.framework":        "file:../../com.vena.framework"
  }
}
```

(Paths are relative from `UnityProject/Packages/` — two levels up to the repo root, then into each package directory.)

## Package Descriptions

### com.vena.core
Namespace: `Vena` (plus `Vena.Test` for unit tests only)

ECS-style World/Actor/Component/System primitives; FastList, SafeMap, and other collections; object pool; GcWatch/ProfilerWatch/TimeWatch; FlowGraph task graph; HierarchicalFsm; JobBalancer.

### com.vena.math
Namespace: `Vena.Math`

Pure math types with no Unity math library dependency: Vector2/3/4, Matrix, Matrix2D, Quaternion, AABB, Ray, Rectangle, Transformation, MathHelper, Hierarchy.

### com.vena.framework
Namespace: `Vena.Framework`

Unity-bound game framework including:
- `GameWorld` (partial aggregate), `GameState/Level/Mode`, `Transition`
- `Module`, `Service`, `Command`
- `Character`, `Pawn`, `SceneRoot`/`SceneController`
- `GUI` (merged from the former ugui package): GuiBase/Panel/Tabs, UIElement/Panel/Root, UIHelper, LoopScrollView, UIColor widgets, UIVfxComponent, and more

### com.vena.unity-extensions
Namespace: `Vena.UnityExtensions`

Runtime: `NodeReferences` (MonoBehaviour node reference container), `GameObjectExtension`, `RectTransformExtension`, `AutoGenTool`
Editor: `ComponentViewer`, `ReadonlyProperty` (PropertyDrawer)

## License

MulanPSL-2.0. See [LICENSE](LICENSE).
