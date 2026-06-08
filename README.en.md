# Vena

Vena — the union of Wei (薇) and Nan (南).

Vena is a lightweight Unity game framework monorepo containing 2 independent UPM packages. Migrated from the vena-core / vena-framework source repositories.

## Package Layout

```
vena/
├── com.vena.core             Core: ECS primitives, collections, pool, profiler, FlowGraph, HierarchicalFsm; math library Vena.Math
└── com.vena.framework        Framework: GameWorld, State, Module, Character, Pawn, Scene, GUI; Unity extensions Vena.UnityExtensions
```

## Dependency Graph

```
com.vena.core           (no dependencies)
com.vena.framework      → com.vena.core
                        → com.unity.ugui
```

## Local Installation (file: reference)

In your Unity project's `Packages/manifest.json`:

```json
{
  "dependencies": {
    "com.vena.core":      "file:../../com.vena.core",
    "com.vena.framework": "file:../../com.vena.framework"
  }
}
```

(Paths are relative from `UnityProject/Packages/` — two levels up to the repo root, then into each package directory.)

## Package Descriptions

### com.vena.core
Namespaces: `Vena` (plus `Vena.Test` for unit tests only); `Vena.Math`

ECS-style World/Actor/Component/System primitives; FastList, SafeMap, and other collections; object pool; GcWatch/ProfilerWatch/TimeWatch; FlowGraph task graph; HierarchicalFsm; JobBalancer.

Math library Vena.Math: Vector2/Vector3/Vector4, Matrix, Matrix2D, Quaternion, AABB, Ray, Rectangle, Transformation, MathHelper, Hierarchy.

### com.vena.framework
Namespaces: `Vena.Framework`; `Vena.UnityExtensions`

Unity-bound game framework including:
- `GameWorld` (partial aggregate), `GameState/Level/Mode`, `Transition`
- `Module`, `Service`, `Command`
- `Character`, `Pawn`, `SceneRoot`/`SceneController`
- `GUI` (merged from the former ugui package): GuiBase/Panel/Tabs, UIElement/Panel/Root, UIHelper, LoopScrollView, UIColor widgets, UIVfxComponent, and more

Unity extensions Vena.UnityExtensions Runtime: NodeReferences, GameObjectExtension, RectTransformExtension, AutoGenTool; Editor: ComponentViewer, ReadonlyProperty.

## License

MulanPSL-2.0. See [LICENSE](LICENSE).
