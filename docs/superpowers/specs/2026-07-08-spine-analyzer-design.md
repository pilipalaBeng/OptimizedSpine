# Spine Analyzer Design

- Date: 2026-07-08
- Status: Approved for implementation
- Related discussion: `019f275d-c197-7260-a9a1-c90d907240ab`

## Context

`OptimizedSpine` is a measurement-first Unity / spine-unity sandbox. The current project already has a reproducible baseline scene and runtime metrics overlay. The next milestone is an editor-side analyzer that can inspect Spine assets before runtime optimization work begins.

The analyzer must stay exploratory. It should report observations and candidate optimization hints, not present assumptions as proven performance wins.

## Goals

- Add an Editor Window for inspecting selected Spine assets and objects.
- Support `SkeletonDataAsset`, scene objects, and prefabs that contain `SkeletonAnimation` or `SkeletonRenderer`.
- Produce a readable report with severity, summary, details, and suggestions.
- Report static signals such as material count, atlas count, animation count, skin count, slot count, and attachment count.
- Flag common optimization topics: multi-material rendering, `Use Single Submesh`, `Immutable Triangles`, and `Update When Invisible`.
- Keep the first version read-only. It must not mutate imported assets, prefab contents, or project settings.
- Cover pure analysis behavior with EditMode tests.

## Non-Goals

- Do not implement runtime LOD yet.
- Do not auto-apply optimization settings.
- Do not claim measured FPS, batch, or memory improvements from static analysis.
- Do not replace Unity Profiler or the existing benchmark scene.

## User Experience

Add a menu item:

- `OptimizedSpine/Spine Analyzer`

The window shows the current selection and an `Analyze Selection` action. If the selection is unsupported, it shows a clear error report instead of throwing. For supported selections, the window displays:

- target name and source type
- summary counts
- findings grouped by severity
- short practical explanations for each suggestion

## Architecture

### `SpineAnalysisReport`

A serializable editor/runtime-neutral data model:

- target display name
- target kind
- analyzed flag
- summary metrics
- ordered findings

Findings use a small severity enum:

- `Info`
- `Warning`
- `Critical`

### `SpineAssetAnalyzer`

A pure analyzer service that accepts a Unity object and returns `SpineAnalysisReport`.

It resolves supported inputs in this order:

1. `SkeletonDataAsset`
2. `SkeletonAnimation`
3. `SkeletonRenderer`
4. `GameObject` containing `SkeletonAnimation` or `SkeletonRenderer`

The analyzer reads from spine-unity objects and skeleton data, but does not modify them. When data is unavailable, it reports a warning instead of failing silently.

### `SpineAnalyzerWindow`

An editor-only UI in `Assets/OptimizedSpine/Editor`.

The window owns selection handling and rendering only. It should not contain the analysis rules directly, so the analyzer can be tested without opening UI.

## Analysis Rules

First-version rules:

- Missing or unsupported target: critical finding.
- Missing `SkeletonDataAsset`: critical finding.
- Missing loaded `SkeletonData`: critical finding.
- Multiple atlas assets or materials: warning about possible extra batches or material switches.
- One material: info that the asset is likely easier to batch.
- High slot count or attachment count: info warning that the asset should be measured in benchmark scenes.
- `Use Single Submesh`: explain that it can reduce submeshes for compatible assets, but may be wrong for assets relying on separated materials or special render behavior.
- `Immutable Triangles`: explain that it is useful only when triangle topology stays stable.
- `Update When Invisible`: explain that offscreen animation should be disabled unless gameplay requires invisible characters to keep animating.

Thresholds should be conservative and visible in code constants so they can be tuned later.

## Testing

Add EditMode tests for:

- unsupported object returns a critical report
- null input returns a critical report
- `SkeletonDataAsset` from the official sample returns an analyzed report with non-zero metrics
- temporary `GameObject` with `SkeletonAnimation` can be analyzed through component resolution
- findings are ordered by severity

The tests should exercise the analyzer service directly, not the Editor Window.

## Documentation

Update project docs after implementation:

- `README.md`: add the analyzer menu and short usage.
- `docs/project-memory.md`: record the new milestone and verification status.

No experiment record is required unless measured runtime data is collected.
