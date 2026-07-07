# Experiment: Baseline scene setup

- Date: 2026-07-06
- Unity version: `2022.3.62f2`
- spine-unity version: `4.3.95`
- spine-csharp version: `4.3.38`
- Scene: `Assets/OptimizedSpine/Scenes/Benchmark_01_Baseline.unity`
- Test asset: `spineboy-pro_SkeletonData.asset`

## Goal

Create the first reproducible baseline scene for later Spine optimization measurements.

## Variables

- Fixed:
  - official `Spine Examples` sample package
  - `spineboy-pro` skeleton
  - `run` animation
  - `25` spawned instances
  - orthographic camera
- Changed:
  - none; this is setup, not a performance comparison

## Procedure

1. Copied official `Samples~/Spine Examples` from the `spine-unity` package cache into `Assets/Samples/spine-unity Runtime/4.3.95/Spine Examples`.
2. Added runtime benchmark scripts under `Assets/OptimizedSpine/Runtime`.
3. Added `OptimizedSpine/Build Baseline Scene` editor menu.
4. Built `Benchmark_01_Baseline.unity`.
5. Added `OptimizedSpine/Validate Baseline Spawner` editor menu and ran it.

## Metrics

- FPS / frame time: not measured yet
- GC Alloc: not measured yet
- batches / draw calls: not measured yet
- vertices / triangles: not measured yet
- material count: not measured yet

## Results

- Baseline scene is saved and opens with `Main Camera` plus `BenchmarkRunner`.
- `BenchmarkRunner` is configured to spawn `25` `spineboy-pro` instances.
- Validation menu successfully created and cleaned up `3` temporary `SkeletonAnimation` instances.
- Final `unity_diagnose` reported compile clean and Console error/warning `0`.

## Notes

- UnitySkills was initially able to run EditMode tests and reported `132/132 passed`.
- Later UnitySkills mode changed to `auto`; `test_run` became blocked by plugin permissions. Full Test Runner verification should be rerun after switching UnitySkills to Bypass.
- spine-unity `4.3` editor-time split-component upgrade required a guard in `SpineBenchmarkSpawner` to keep editor validation clean.

## Conclusion

Actionable setup complete. No performance conclusion yet.
