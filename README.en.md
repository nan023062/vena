# Vena

[中文](README.md) | [English](README.en.md)

Vena is a continuously updated multi-package Unity repository for game development frameworks and reusable technical modules. Each module is organized as a Unity Package Manager package and can be consumed independently by Unity projects.

More packages will be added over time as the repository grows into a personal open-source collection of runtime foundations, framework layers, editor tooling, and reusable game-development infrastructure. Different packages may have different responsibilities and release scopes.

Vena is also a summary of my personal game-development experience. The design and implementation are not meant to be the only correct answer; they are shared as accumulated framework ideas, module boundaries, and technical practices from real development work. Feedback, corrections, bug reports, and improvement suggestions are welcome.

## Packages

```
vena/
├── com.vena.core        Core package. Collections, pool, profiler, FlowGraph, FSM, job balance.
├── com.vena.math        Math package. Vectors, matrices, quaternion, ray, rectangle, transform helpers.
├── com.vena.world       World package. World, actor/component/system, controller, lifecycle primitives.
├── com.vena.framework   Framework package. GameWorld, state/module/service/command, character, scene, GUI.
├── com.vena.assets      Assets package. AssetBundle build/load, version control, compression, IOssClient OSS abstraction.
└── unity-project        Development and verification Unity project for packages in this repo.
```

## Dependencies

```
com.vena.core           no package dependency
com.vena.math           no package dependency
com.vena.world          no package dependency
com.vena.framework      depends on com.vena.core
                        depends on com.vena.world
                        depends on com.unity.ugui
com.vena.assets         no package dependency
```

`com.vena.core`, `com.vena.math`, and `com.vena.world` can be used on their own.

`com.vena.world` provides the World / Actor / Component / System-style runtime organization layer. It does not depend on UnityEngine or any other Vena package. Unity projects can forward `Time.deltaTime` or `Time.time` from MonoBehaviour, while non-Unity applications can use the built-in time source or drive updates manually.

`com.vena.framework` is a higher-level Unity game framework package and should be installed together with `com.vena.core` and `com.vena.world`.

## Install From Git

Add the packages you need to your Unity project's `Packages/manifest.json`. Each package can be referenced from the same Git repository using a different `path`.

Core only:

```json
{
  "dependencies": {
    "com.vena.core": "https://github.com/nan023062/vena.git?path=/com.vena.core#main"
  }
}
```

Math only:

```json
{
  "dependencies": {
    "com.vena.math": "https://github.com/nan023062/vena.git?path=/com.vena.math#main"
  }
}
```

World:

```json
{
  "dependencies": {
    "com.vena.world": "https://github.com/nan023062/vena.git?path=/com.vena.world#main"
  }
}
```

Framework:

```json
{
  "dependencies": {
    "com.vena.core": "https://github.com/nan023062/vena.git?path=/com.vena.core#main",
    "com.vena.world": "https://github.com/nan023062/vena.git?path=/com.vena.world#main",
    "com.vena.framework": "https://github.com/nan023062/vena.git?path=/com.vena.framework#main"
  }
}
```

Assets only:

```json
{
  "dependencies": {
    "com.vena.assets": "https://github.com/nan023062/vena.git?path=/com.vena.assets#main"
  }
}
```

> When installing dependency-chain packages from Git paths, it is recommended to declare the required Vena packages explicitly in the project manifest so Unity does not try to resolve personal packages from a default registry.

## Local Development Install

For local development, use `file:` references:

```json
{
  "dependencies": {
    "com.vena.core": "file:../../com.vena.core",
    "com.vena.math": "file:../../com.vena.math",
    "com.vena.world": "file:../../com.vena.world",
    "com.vena.framework": "file:../../com.vena.framework"
  }
}
```

Paths are relative to the Unity project's `Packages/manifest.json`; adjust them to match your local folder layout.

## Package Overview

### com.vena.core

Core runtime foundations for Unity game development:
- Collections and non-alloc containers
- Object pool and weak pooled helper structures
- TaskGraph / TaskClip primitives
- HierarchicalFsm
- JobBalance stepped job scheduler
- Profiler / Time / GC watch helpers

### com.vena.math

Standalone math types and utilities for Unity game development:
- Vector2 / Vector3 / Vector4
- Matrix / Matrix2D
- Quaternion
- Ray / Rectangle / AABB
- Point / Hierarchy / Transformation
- MathHelper utilities

### com.vena.world

Standalone runtime world and object organization layer for Unity and non-Unity environments:
- World / Actor / Component / System-style runtime primitives
- Controller, filter, and archetype management
- Lifecycle interfaces and PairwiseLifeCycle
- Attributes for injection, ordering, requirements, and system markers
- No UnityEngine references; host applications can manually drive Update / LateUpdate / application events

### com.vena.framework

Unity-bound game framework layer built on `com.vena.core` and `com.vena.world`:
- GameWorld
- GameState / GameLevel / GameMode / Transition
- Module / Service / Command
- Character / Pawn / Scene
- GUI / UI base components and widgets
- UnityExtensions runtime and editor utilities

## Repository Policy

- Every `com.vena.*` directory is an independent Unity package.
- Package relationships are declared through `package.json`.
- More packages will be added over time according to their module responsibilities.
- The root `unity-project` is used for development, build verification, and sample integration.
- Public APIs are intentionally conservative: implementation details stay internal unless they are meant to be extended, called, or composed by package users.

## License

MulanPSL-2.0. See [LICENSE](LICENSE).
