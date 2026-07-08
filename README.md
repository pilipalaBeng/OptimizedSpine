# OptimizedSpine

`OptimizedSpine` 是一个 Unity / spine-unity 性能实验沙盒，来自对话 `019f275d-c197-7260-a9a1-c90d907240ab` 中的 Spine 优化想法。

当前目标不是马上做生产级优化框架，而是先建立可复现、可测量、可记录的 benchmark 环境。

## Current State

- Unity: `2022.3.62f2`
- spine-unity UPM:
  - `com.esotericsoftware.spine.spine-csharp`: `4.3.38`
  - `com.esotericsoftware.spine.spine-unity`: `4.3.95`
- UnitySkills: 已安装并可通过本地 REST server 连接。
- 官方样本: 已从 package cache 导入 `Spine Examples` 到 `Assets/Samples/spine-unity Runtime/4.3.95/Spine Examples`。
- Baseline scene: `Assets/OptimizedSpine/Scenes/Benchmark_01_Baseline.unity`
- 默认测试 skeleton: `Assets/Samples/spine-unity Runtime/4.3.95/Spine Examples/Spine Skeletons/spineboy-pro/spineboy-pro_SkeletonData.asset`

## How To Use

1. 打开 `Assets/OptimizedSpine/Scenes/Benchmark_01_Baseline.unity`。
2. Press Play。
3. 场景会生成 `25` 个 `spineboy-pro` skeleton，并在 Game 视图左上角用大号红色文字显示基础指标。

可用菜单:

- `OptimizedSpine/Build Baseline Scene`: 重新生成 baseline 场景。
- `OptimizedSpine/Validate Baseline Spawner`: 在编辑器中临时生成 3 个 Spine 实例并清理，用于快速验证 spawner 创建链路。

## Analyzer Tool

- `OptimizedSpine/Spine Analyzer`: opens a read-only Editor Window for selected Spine assets or objects.
- The analyzer reports static metrics and candidate hints for material count, atlas count, slots, skins, animations, attachments, `Use Single Submesh`, `Immutable Triangles`, and `Update When Invisible`.
- It does not auto-apply changes or claim measured runtime gains.

## Project Code

- `Assets/OptimizedSpine/Runtime/SpineBenchmarkSpawner.cs`: 从 `SkeletonDataAsset` 生成 `SkeletonAnimation` 实例。
- `Assets/OptimizedSpine/Runtime/SpineBenchmarkMetrics.cs`: 用大号红色文字显示 FPS、frame time、Mono memory、Total allocated memory、实例数。
- `Assets/OptimizedSpine/Runtime/SpineBenchmarkLayout.cs`: 网格布局 helper。
- `Assets/OptimizedSpine/Editor/SpineBenchmarkSceneBuilder.cs`: baseline 场景生成和验证菜单。
- `Assets/OptimizedSpine/Tests/EditMode/`: EditMode 测试草稿。

## Local Backups

- `D:\Porject\Unity\OptimizedSpine_Backups\spine-art-20260706-175152\Spine`: 旧项目误导入的 Spine 美术资源备份。

## Verification

- `unity_diagnose`: compile clean，Console error/warning 为 `0`。
- `OptimizedSpine/Validate Baseline Spawner`: 已执行通过，执行后 Console error/warning 为 `0`。
- Unity Test Runner: 之前 EditMode 跑过 `132/132 passed`；当前 UnitySkills 模式变成 `auto` 后，`test_run` 被插件权限门禁拦截。需要切到 Bypass 后再跑完整 EditMode。

## Direction

1. Benchmark sandbox: 固定场景、固定资源、固定实例数量，先采集基础指标。
2. Analyzer tool: 检查 Spine prefab / asset 的材质、slot、mesh、update 设置，输出建议。
3. Runtime LOD: 在有测量基础后，再研究可切换的运行时策略。

## Metrics To Track

- `FPS` / frame time
- `GC Alloc`
- batches / draw calls
- mesh vertices / triangles
- material count
- visible / invisible update behavior

## Documentation Map

- `AGENTS.md`: 给未来 Agent 的协作规则和项目约定。
- `docs/project-memory.md`: 长期上下文、关键假设、当前状态和更新规则。
- `docs/decisions/`: 记录重要方向选择。
- `docs/experiments/`: 记录每次性能实验的条件和结果。
