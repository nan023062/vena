# Vena Core

`com.vena.core` is the core runtime package in the Vena multi-package Unity repository.

It can be installed and used independently. Higher-level Vena packages, such as `com.vena.framework`, build on top of this package.

## Install

```json
{
  "dependencies": {
    "com.vena.core": "https://github.com/nan023062/vena.git?path=/com.vena.core#main"
  }
}
```

For local development:

```json
{
  "dependencies": {
    "com.vena.core": "file:../../com.vena.core"
  }
}
```

## Contents

- World / Actor / Component / System-style runtime primitives
- Controller, filter, archetype and lifecycle infrastructure
- Collections: FastList, SafeMap, NonAllocLinkList, LinkedStack and helpers
- Object pool and weak pooled helper structures
- TaskGraph / TaskClip primitives
- HierarchicalFsm
- JobBalance stepped job scheduler
- `Vena.Math` math types and utilities
- Profiler / Time / GC watch helpers

## Namespaces

- `Vena`
- `Vena.Math`

## License

MulanPSL-2.0. See the repository [LICENSE](../LICENSE).
