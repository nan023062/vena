# Vena World

`com.vena.world` is the engine-agnostic world and actor runtime package in the Vena multi-package Unity repository.

It provides the runtime organization layer for actors, components, controllers, systems, filters, archetypes, and lifecycle coordination. The package does not reference UnityEngine and can be used from Unity projects or regular .NET runtime applications.

## Install

```json
{
  "dependencies": {
    "com.vena.world": "https://github.com/nan023062/vena.git?path=/com.vena.world#main"
  }
}
```

For local development:

```json
{
  "dependencies": {
    "com.vena.world": "file:../../com.vena.world"
  }
}
```

## Dependencies

None.

## Runtime Integration

Unity projects can drive the world from a MonoBehaviour:

```csharp
World.Update(UnityEngine.Time.deltaTime);
World.LateUpdate(UnityEngine.Time.deltaTime);
```

For absolute engine time:

```csharp
World.UpdateAt(UnityEngine.Time.time);
```

Non-Unity applications can use the built-in Stopwatch-backed time source:

```csharp
World.Update();
World.LateUpdate();
```

## Contents

- World / Actor / Component / System runtime primitives
- Controller infrastructure
- Component filter and archetype management
- Lifecycle interfaces and pairwise lifecycle support
- Runtime attributes for injection, ordering, requirements, and systems
- Host-driven Update / LateUpdate / application event forwarding

## Namespaces

- `Vena`

## License

MulanPSL-2.0. See the repository [LICENSE](../LICENSE).
