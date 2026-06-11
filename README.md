# Vena

Vena 是一个面向 Unity 游戏开发的持续更新多包仓库。仓库中的每个模块都按照 Unity Package Manager package 的形式组织，可以作为独立的 Unity package 被项目依赖和下载。

这个仓库会陆续更新更多 package，用来逐步沉淀个人游戏开发框架、运行时基础设施、工具模块和可复用技术组件。不同 package 可以拥有不同定位：有的提供底层数据结构和运行时原语，有的提供更完整的游戏框架层，有的后续可能专注于编辑器工具、渲染、资源、流程或其他技术方向。

Vena 也是我个人游戏开发经验的一部分总结。这里的设计和实现并不追求成为唯一答案，而是把长期开发中沉淀下来的框架思路、模块划分和技术实践分享出来。欢迎大家阅读、使用、指正，也欢迎在实际使用后反馈 bug、问题和改进建议。

## Packages

```
vena/
├── com.vena.core        Core package. ECS-style world, actor/component/system primitives,
│                        collections, object pool, math, profiler, task graph, FSM, job balance.
├── com.vena.framework   Framework package. Unity-bound game framework layer built on Vena Core:
│                        GameWorld, state/module/service/command, character, scene, GUI, extensions.
└── unity-project        Development and verification Unity project for the packages in this repo.
```

## Package Dependencies

```
com.vena.core           no package dependency
com.vena.framework      depends on com.vena.core
                        depends on com.unity.ugui
```

`com.vena.core` 可以单独使用。

`com.vena.framework` 是更上层的 Unity 游戏框架 package，使用时需要同时引入 `com.vena.core`。

## Install From Git

在 Unity 项目的 `Packages/manifest.json` 中添加需要的 package。每个 package 都可以通过同一个 Git 仓库和不同的 `path` 独立引用。

只使用 Core：

```json
{
  "dependencies": {
    "com.vena.core": "https://github.com/nan023062/vena.git?path=/com.vena.core#main"
  }
}
```

使用 Framework：

```json
{
  "dependencies": {
    "com.vena.core": "https://github.com/nan023062/vena.git?path=/com.vena.core#main",
    "com.vena.framework": "https://github.com/nan023062/vena.git?path=/com.vena.framework#main"
  }
}
```

> Framework 的 `package.json` 声明了对 `com.vena.core` 的依赖，但通过 Git path 安装时，建议在项目 manifest 中显式声明 Core，避免 Unity 无法从默认 registry 解析私有/个人 package。

## Local Development Install

如果 Unity 项目和本仓库在本地同级或相邻目录，也可以使用 `file:` 引用：

```json
{
  "dependencies": {
    "com.vena.core": "file:../../com.vena.core",
    "com.vena.framework": "file:../../com.vena.framework"
  }
}
```

路径以 Unity 项目的 `Packages/manifest.json` 所在目录为基准，请按实际目录结构调整。

## Package Overview

### com.vena.core

定位：Unity 游戏开发的核心运行时基础模块。

包含：
- World / Actor / Component / System 风格的运行时组织原语
- 常用集合与非分配容器
- 对象池与弱引用式池化辅助结构
- TaskGraph / TaskClip 等任务图基础能力
- HierarchicalFsm 层级状态机
- JobBalance 分帧任务调度
- `Vena.Math` 数学类型和工具
- Profiler / Time / GC 观察辅助工具

### com.vena.framework

定位：基于 `com.vena.core` 的 Unity 游戏框架层。

包含：
- GameWorld
- GameState / GameLevel / GameMode / Transition
- Module / Service / Command
- Character / Pawn / Scene
- GUI / UI 基础组件和常用控件
- UnityExtensions 运行时与编辑器扩展

## Repository Policy

- 每个 `com.vena.*` 目录都是一个独立 Unity package。
- package 之间通过 `package.json` 声明依赖关系。
- 后续会按模块定位陆续增加新的 package。
- 根目录的 `unity-project` 用于开发、编译验证和示例集成。
- 公共 API 会尽量保持克制：只有明确面向使用者继承、调用或组合的能力才暴露。

## License

MulanPSL-2.0. See [LICENSE](LICENSE).
