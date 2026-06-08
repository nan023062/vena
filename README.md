# Vena

Vena = 薇南，薇与南的结合。

Vena 是一个面向 Unity 的轻量游戏框架 monorepo，包含 4 个独立 UPM 包。迁移自 vena-core / vena-framework 原始仓库。

## 包结构

```
vena/
├── com.vena.core             核心层：ECS 原语、集合、对象池、Profiler、FlowGraph、HierarchicalFsm
├── com.vena.math             纯数学层：向量、矩阵、四元数、AABB、射线、变换
├── com.vena.framework        游戏框架层：GameWorld、State、Module、Character、Pawn、Scene、GUI
└── com.vena.unity-extensions Unity 工具层：NodeReferences、扩展方法、编辑器工具
```

## 依赖关系

```
com.vena.core           (无依赖)
com.vena.math           (无依赖)
com.vena.unity-extensions (无依赖)
com.vena.framework      → com.vena.core
                        → com.vena.unity-extensions
                        → com.unity.ugui
```

## 本地安装（file: 引用）

在 Unity 项目的 `Packages/manifest.json` 中添加：

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

（路径以 `UnityProject/Packages/` 为起点，向上两级到仓库根，再进入各包目录。）

## 包说明

### com.vena.core
命名空间：`Vena`（`Vena.Test` 仅用于单元测试）

ECS 风格的 World/Actor/Component/System 原语；FastList、SafeMap 等集合；对象池；GcWatch/ProfilerWatch/TimeWatch；FlowGraph 任务图；HierarchicalFsm 层级状态机；JobBalancer。

### com.vena.math
命名空间：`Vena.Math`

纯数学类型，不依赖 Unity 数学库：Vector2/3/4、Matrix、Matrix2D、Quaternion、AABB、Ray、Rectangle、Transformation、MathHelper、Hierarchy。

### com.vena.framework
命名空间：`Vena.Framework`

Unity 绑定的游戏框架，含：
- `GameWorld`（partial 聚合）、`GameState/Level/Mode`、`Transition`
- `Module`、`Service`、`Command`
- `Character`、`Pawn`、`SceneRoot`/`SceneController`
- `GUI`（原 ugui 包并入）：GuiBase/Panel/Tabs、UIElement/Panel/Root、UIHelper、LoopScrollView、UIColor、UIVfxComponent 等

### com.vena.unity-extensions
命名空间：`Vena.UnityExtensions`

Runtime：`NodeReferences`（MonoBehaviour 节点引用容器）、`GameObjectExtension`、`RectTransformExtension`、`AutoGenTool`
Editor：`ComponentViewer`、`ReadonlyProperty`（PropertyDrawer）

## 许可证

MulanPSL-2.0，见 [LICENSE](LICENSE)。
