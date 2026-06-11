# Vena

Vena 是一个面向 Unity 的游戏框架 monorepo，包含 2 个独立 UPM 包, 目前就2个后面会慢慢添加补充。

## 包结构

```
vena/
├── com.vena.core             核心层：ECS 原语、集合、对象池、Profiler、FlowGraph、HierarchicalFsm；数学库 Vena.Math
└── com.vena.framework        游戏框架层：GameWorld、State、Module、Character、Pawn、Scene、GUI；Unity 扩展 Vena.UnityExtensions
```

## 依赖关系

```
com.vena.core           (无依赖)
com.vena.framework      → com.vena.core
                        → com.unity.ugui
```

## 本地安装（file: 引用）

在 Unity 项目的 `Packages/manifest.json` 中添加：

```json
{
  "dependencies": {
    "com.vena.core":    "file:../../com.vena.core",
    "com.vena.framework": "file:../../com.vena.framework"
  }
}
```

（路径以 `UnityProject/Packages/` 为起点，向上两级到仓库根，再进入各包目录。）

## 包说明

### com.vena.core
命名空间：`Vena`（`Vena.Test` 仅用于单元测试）；`Vena.Math`

ECS 风格的 World/Actor/Component/System 原语；FastList、SafeMap 等集合；对象池；GcWatch/ProfilerWatch/TimeWatch；FlowGraph 任务图；HierarchicalFsm 层级状态机；JobBalancer。

数学库 Vena.Math：Vector2/Vector3/Vector4、Matrix、Matrix2D、Quaternion、AABB、Ray、Rectangle、Transformation、MathHelper、Hierarchy。

### com.vena.framework
命名空间：`Vena.Framework`；`Vena.UnityExtensions`

Unity 绑定的游戏框架，含：
- `GameWorld`（partial 聚合）、`GameState/Level/Mode`、`Transition`
- `Module`、`Service`、`Command`
- `Character`、`Pawn`、`SceneRoot`/`SceneController`
- `GUI`（原 ugui 包并入）：GuiBase/Panel/Tabs、UIElement/Panel/Root、UIHelper、LoopScrollView、UIColor、UIVfxComponent 等

Unity 扩展 Vena.UnityExtensions Runtime：NodeReferences、GameObjectExtension、RectTransformExtension、AutoGenTool；Editor：ComponentViewer、ReadonlyProperty。

## 许可证

MulanPSL-2.0，见 [LICENSE](LICENSE)。
