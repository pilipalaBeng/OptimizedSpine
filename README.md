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
- `OptimizedSpine/Benchmark Presets`: 一键设置 `10 / 25 / 50 / 100` 实例，并同步 snapshot 实验名称。
- `OptimizedSpine/Write Benchmark Snapshot`: 在 Play Mode 采样完成后，将 benchmark 结果导出到 `docs/experiments/`。
- `OptimizedSpine/Compare Benchmark Snapshots`: 选择两份 snapshot markdown，对比 FPS、frame time 和 memory 差异。

## Benchmark Snapshot

- `BenchmarkRunner` 上的 `SpineBenchmarkSnapshotRecorder` 会在 Play Mode 中先 warmup，再采集一段固定时长的 frame/memory 数据。
- 默认采样口径: warmup `3s`，sample window `10s`，输出目录 `docs/experiments/`。
- Game 视图 overlay 会显示 `Snapshot: WarmingUp / Sampling / Complete` 和实际/目标采样秒数；等它显示 `Complete` 后再导出。
- `Write Benchmark Snapshot` 默认会拒绝 no-sample 或 partial 结果，避免把未完成采样误当成可对比数据。
- 新 snapshot 会同时记录 `Target Sample Window` 和 `Actual Sample Window`，方便检查采样是否跑满。
- 导出的 markdown 是 raw snapshot，只记录当前场景条件和基础指标；需要与另一份 snapshot 对比后，才能说某个设置带来了优化收益。
- Benchmark runtime 组件的 Inspector 字段使用中文显示名和中文 Tooltip，方便直接调实例数、布局和采样窗口。
- `Benchmark Presets` 用来固定测量条件，避免一次跑 `25` 实例、下一次误跑成 `20` 实例。
- `Compare Benchmark Snapshots` 会提示关键上下文是否一致，例如 `Instance Count`、场景、Skeleton、动画、Unity / spine-unity 版本、采样窗口。

## Analyzer Tool

- `OptimizedSpine/Spine Analyzer`: opens a read-only Editor Window for selected Spine assets or objects.
- The analyzer reports static metrics and candidate hints for material count, atlas count, slots, skins, animations, attachments, `Use Single Submesh`, `Immutable Triangles`, and `Update When Invisible`.
- Analyzer labels and suggestions support `English` / `中文` display modes from a language dropdown. Default mode is `English`.
- It does not auto-apply changes or claim measured runtime gains.

## Project Code

- `Assets/OptimizedSpine/Runtime/SpineBenchmarkSpawner.cs`: 从 `SkeletonDataAsset` 生成 `SkeletonAnimation` 实例。
- `Assets/OptimizedSpine/Runtime/SpineBenchmarkMetrics.cs`: 用大号红色文字显示 FPS、frame time、Mono memory、Total allocated memory、实例数。
- `Assets/OptimizedSpine/Runtime/SpineBenchmarkSnapshot.cs`: benchmark snapshot 数据结构、frame accumulator 和 markdown formatter。
- `Assets/OptimizedSpine/Runtime/SpineBenchmarkSnapshotRecorder.cs`: 运行时采样并导出 `docs/experiments/*.md`。
- `Assets/OptimizedSpine/Runtime/SpineBenchmarkLayout.cs`: 网格布局 helper。
- `Assets/OptimizedSpine/Editor/Benchmarking/`: snapshot markdown 解析和指标对比逻辑。
- `Assets/OptimizedSpine/Editor/SpineBenchmarkPresetWindow.cs`: benchmark 实例数预设窗口。
- `Assets/OptimizedSpine/Editor/SpineBenchmarkSnapshotCompareWindow.cs`: snapshot 对比窗口。
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
