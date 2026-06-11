# Vena Framework

`com.vena.framework` is the Unity-bound game framework package in the Vena multi-package Unity repository.

It builds on `com.vena.core` and provides a higher-level framework layer for game projects.

## Install

When installing from Git, declare both Framework and Core in your Unity project's `Packages/manifest.json`:

```json
{
  "dependencies": {
    "com.vena.core": "https://github.com/nan023062/vena.git?path=/com.vena.core#main",
    "com.vena.framework": "https://github.com/nan023062/vena.git?path=/com.vena.framework#main"
  }
}
```

For local development:

```json
{
  "dependencies": {
    "com.vena.core": "file:../../com.vena.core",
    "com.vena.framework": "file:../../com.vena.framework"
  }
}
```

## Dependencies

- `com.vena.core`
- `com.unity.ugui`

## Contents

- GameWorld
- GameState / GameLevel / GameMode / Transition
- Module / Service / Command
- Character / Pawn / SceneRoot / SceneController
- GUI / UI base components and widgets
- UnityExtensions runtime and editor utilities

## Namespaces

- `Vena.Framework`
- `Vena.UnityExtensions`

## License

MulanPSL-2.0. See the repository [LICENSE](../LICENSE).
