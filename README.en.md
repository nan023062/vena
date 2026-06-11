# Vena

Vena is a continuously updated multi-package Unity repository for game development frameworks and reusable technical modules. Each module is organized as a Unity Package Manager package and can be consumed independently by Unity projects.

More packages will be added over time as the repository grows into a personal open-source collection of runtime foundations, framework layers, editor tooling, and reusable game-development infrastructure. Different packages may have different responsibilities and release scopes.

Vena is also a summary of my personal game-development experience. The design and implementation are not meant to be the only correct answer; they are shared as accumulated framework ideas, module boundaries, and technical practices from real development work. Feedback, corrections, bug reports, and improvement suggestions are welcome.

## Packages

```
vena/
├── com.vena.core        Core package: ECS-style world, actor/component/system primitives,
│                        collections, object pool, math, profiler, task graph, FSM, job balance.
├── com.vena.framework   Framework package built on Vena Core:
│                        GameWorld, state/module/service/command, character, scene, GUI, extensions.
└── unity-project        Development and verification Unity project for packages in this repo.
```

## Dependencies

```
com.vena.core           no package dependency
com.vena.framework      depends on com.vena.core
                        depends on com.unity.ugui
```

`com.vena.core` can be used on its own.

`com.vena.framework` is a higher-level Unity game framework package and should be installed together with `com.vena.core`.

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

Framework:

```json
{
  "dependencies": {
    "com.vena.core": "https://github.com/nan023062/vena.git?path=/com.vena.core#main",
    "com.vena.framework": "https://github.com/nan023062/vena.git?path=/com.vena.framework#main"
  }
}
```

> `com.vena.framework` declares `com.vena.core` as a dependency, but when installing from Git paths it is recommended to declare Core explicitly in the project manifest so Unity does not try to resolve the personal package from a default registry.

## Local Development Install

For local development, use `file:` references:

```json
{
  "dependencies": {
    "com.vena.core": "file:../../com.vena.core",
    "com.vena.framework": "file:../../com.vena.framework"
  }
}
```

Paths are relative to the Unity project's `Packages/manifest.json`; adjust them to match your local folder layout.

## Package Overview

### com.vena.core

Core runtime foundations for Unity game development:
- World / Actor / Component / System-style runtime primitives
- Collections and non-alloc containers
- Object pool and weak pooled helper structures
- TaskGraph / TaskClip primitives
- HierarchicalFsm
- JobBalance stepped job scheduler
- `Vena.Math` math types and utilities
- Profiler / Time / GC watch helpers

### com.vena.framework

Unity-bound game framework layer built on `com.vena.core`:
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
