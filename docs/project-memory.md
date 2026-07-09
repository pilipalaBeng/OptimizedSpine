# Project Memory

## Stable Context

- Project name: `OptimizedSpine`
- Unity version: `2022.3.62f2`
- Related discussion: `019f275d-c197-7260-a9a1-c90d907240ab`
- Project nature: 独立 Spine 性能实验沙盒，偏个人研究和验证，不按生产项目压力推进。
- GitHub repository: `https://github.com/pilipalaBeng/OptimizedSpine`
- Git remote: `origin https://github.com/pilipalaBeng/OptimizedSpine.git`

## Starting Assumptions

- 先测量，再分析，再优化。
- 之前讨论过的优化方向仍属于实验假设，不直接当作已验证结论。
- 当前使用官方 spine-unity UPM Git dependency:
  - `com.esotericsoftware.spine.spine-csharp`
  - `com.esotericsoftware.spine.spine-unity`
  - Git branch/tag path: `spine-runtimes` `4.3`
- UnitySkills UPM dependency: `https://github.com/Besty0728/Unity-Skills.git?path=/SkillsForUnity`
- UnitySkills server: `http://127.0.0.1:8090` / `http://localhost:8090`
- Instance id seen in this project: `OptimizedSpine_3A8A4766`

## Imported Assets

- 2026-07-06 用户误导入了一批旧项目 Spine 美术资源到 `Assets/Art/Spine`。
- 误导入资源探测结果:
  - about `10665` files
  - about `163.76 MB`
  - JSON skeleton version mostly Spine `4.0.63`, plus one `4.0.64`
- 已选择干净沙盒路线，并将误导入资源移出项目。
- Backup path: `D:\Porject\Unity\OptimizedSpine_Backups\spine-art-20260706-175152\Spine`
- 当前官方样本路径: `Assets/Samples/spine-unity Runtime/4.3.95/Spine Examples`

## Baseline Setup

- Baseline scene: `Assets/OptimizedSpine/Scenes/Benchmark_01_Baseline.unity`
- Default skeleton: `Assets/Samples/spine-unity Runtime/4.3.95/Spine Examples/Spine Skeletons/spineboy-pro/spineboy-pro_SkeletonData.asset`
- Default animation: `run`
- Default instance count: `25`
- Metrics overlay: Game view top-left, large red text.
- Snapshot recorder: `BenchmarkRunner` includes `SpineBenchmarkSnapshotRecorder`, default warmup `3s`, sample window `10s`, output `docs/experiments/`.
- Main runtime scripts:
  - `Assets/OptimizedSpine/Runtime/SpineBenchmarkSpawner.cs`
  - `Assets/OptimizedSpine/Runtime/SpineBenchmarkMetrics.cs`
  - `Assets/OptimizedSpine/Runtime/SpineBenchmarkLayout.cs`
  - `Assets/OptimizedSpine/Runtime/SpineBenchmarkSnapshot.cs`
  - `Assets/OptimizedSpine/Runtime/SpineBenchmarkSnapshotRecorder.cs`
- Editor helper:
  - `Assets/OptimizedSpine/Editor/SpineBenchmarkSceneBuilder.cs`
  - Menu: `OptimizedSpine/Build Baseline Scene`
  - Menu: `OptimizedSpine/Validate Baseline Spawner`
  - Menu: `OptimizedSpine/Write Benchmark Snapshot`

## Spine 4.3 Runtime Note

- spine-unity `4.3` split `SkeletonRenderer` and `SkeletonAnimation` components.
- In editor-time validation, newly created `SkeletonAnimation` can run its old-component auto-upgrade path and overwrite `SkeletonRenderer.SkeletonDataAsset` with a null deprecated field.
- The benchmark spawner avoids this by creating the spawned GameObject inactive, assigning `SkeletonRenderer` / `SkeletonAnimation`, initializing them, and marking the editor-only `wasDeprecatedTransferred` field before activation.
- This workaround is editor-only; Play Mode should not need the upgrade guard, but keeping edit-time validation clean is useful for this sandbox.

## Verification Status

- `unity_diagnose`: compile clean, Console error/warning `0`.
- `OptimizedSpine/Validate Baseline Spawner`: passed and left Console error/warning `0`.
- Unity Test Runner:
  - Earlier EditMode run passed `132/132`.
  - After UnitySkills mode changed to `auto`, `test_run` is blocked by plugin permission gate because it is classified as `mayEnterPlayMode`.
  - Switch UnitySkills to Bypass before relying on full Test Runner verification.

## Optimization Axes

- Instance count: `10`, `30`, `100`, etc.
- Spine settings: `Immutable Triangles`, `Use Single Submesh`, `Update When Invisible`.
- Rendering: material count, atlas organization, draw calls, batches.
- Animation/update: visibility, offscreen update, update frequency, runtime LOD.
- Import workflow: `PMA` vs `Straight Alpha` consistency.

## First Milestone Candidate

Build an Editor Window that inspects selected Spine prefab / asset and reports static optimization hints:

- material count
- possible slot/material switches
- benchmark suitability
- setting hints that are not auto-applied

## Analyzer Milestone

- Editor window: `OptimizedSpine/Spine Analyzer`
- Analyzer service: `Assets/OptimizedSpine/Editor/Analysis/SpineAssetAnalyzer.cs`
- First version is read-only and reports static hints only.
- Analyzer UI copy supports `English` / `中文` display modes from a language dropdown. Default mode is `English`.
- Runtime LOD remains a later milestone after benchmark measurements.

## Experiment Recording Rules

- Add a file under `docs/experiments/` for each reproducible performance experiment.
- Each record should include Unity version, spine-unity version, scene, asset source, variables, measurement method, and conclusion.
- Use `OptimizedSpine/Write Benchmark Snapshot` after Play Mode sampling to export raw benchmark records.
- Treat generated snapshots as raw measurements. Compare snapshots before claiming an optimization gain.
- Without measurement data, write only assumptions or observations. Do not present them as verified optimization conclusions.

## Update Rules

- Update this file when project direction or durable setup changes.
- Add an ADR in `docs/decisions/` when making a long-term design choice.
- Add an experiment record when a reproducible benchmark or measurement is completed.
- If official spine-unity docs or source confirm behavior, include source/path notes in the relevant doc.
