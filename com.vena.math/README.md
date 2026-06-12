# Vena Math

`com.vena.math` is the math package in the Vena multi-package Unity repository.

It can be installed and used independently by Unity projects that need reusable math types and helpers without depending on the higher-level Vena runtime packages.

## Install

```json
{
  "dependencies": {
    "com.vena.math": "https://github.com/nan023062/vena.git?path=/com.vena.math#main"
  }
}
```

For local development:

```json
{
  "dependencies": {
    "com.vena.math": "file:../../com.vena.math"
  }
}
```

## Contents

- Vector2 / Vector3 / Vector4
- Matrix and Matrix2D
- Quaternion
- Ray / Rectangle / AABB
- Point / Hierarchy / Transformation
- Math helper utilities

## Namespaces

- `Vena.Math`

## License

MulanPSL-2.0. See the repository [LICENSE](../LICENSE).
