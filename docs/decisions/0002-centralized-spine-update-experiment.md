# ADR 0002: Centralized Spine Update Experiment

## Status

Accepted for benchmark experimentation.

## Context

The project is testing whether many independent `SkeletonAnimation` updates create avoidable runtime overhead. The current evidence in this repository is not enough to claim an optimization win, so the implementation must make the idea measurable before treating it as a runtime strategy.

spine-unity includes sample code such as `SkeletonAnimationFixedTimestep` that disables a `SkeletonAnimation` component and manually calls `SkeletonAnimation.Update(delta)` plus `SkeletonAnimation.Renderer.LateUpdate()`. This project uses that pattern only inside the benchmark sandbox.

## Decision

Add a benchmark `Update Mode` with two values:

- `Baseline`: keep each spawned `SkeletonAnimation` enabled, using spine-unity's normal per-object update path.
- `CentralizedUpdate`: disable each spawned `SkeletonAnimation` component and let `SpineBenchmarkSpawner` update all spawned instances from one `Update` / `LateUpdate` pair.

Snapshots record `Update Mode`, and snapshot comparison warns when modes differ.

## Consequences

- The tool can compare `Baseline_25` and `CentralizedUpdate_25` under the same scene, skeleton, animation, instance count, sample window, Unity version, and spine-unity version.
- This does not prove `CentralizedUpdate` is faster by itself. It only creates a controlled way to measure the claim.
- Future runtime LOD work should use these measurements before expanding the strategy.
