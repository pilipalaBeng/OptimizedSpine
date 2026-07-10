# Experiment Records

Use this folder to record Spine performance experiments.

Recommended filename:

```text
YYYY-MM-DD-short-topic.md
```

Generated snapshots from `OptimizedSpine/Write Benchmark Snapshot` also land in this folder. Use `OptimizedSpine/Benchmark Presets` before sampling to keep instance counts consistent, then wait until the Game view overlay shows `Snapshot: Complete` before exporting. These files are raw measurements: keep them, compare them against another snapshot with `OptimizedSpine/Compare Benchmark Snapshots`, and only then write an explicit conclusion. No-sample or partial snapshots are debugging artifacts, not optimization evidence.

## Template

```markdown
# Experiment: Short description

- Date:
- Unity version:
- spine-unity version:
- Branch / commit:
- Scene:
- Test asset:
- Update mode:

## Goal

What question is this experiment answering?

## Variables

- Fixed:
- Changed:

## Procedure

1. 

## Metrics

- FPS / frame time:
- GC Alloc:
- batches / draw calls:
- vertices / triangles:
- material count:

## Results

## Notes

## Conclusion

Is this result actionable, inconclusive, or rejected?
```
