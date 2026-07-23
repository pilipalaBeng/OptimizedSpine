# Baseline vs CentralizedUpdate 25

## Context

| Field | Value |
| --- | --- |
| Compared At | `2026-07-23` |
| Unity | `2022.3.62f2` |
| spine-unity | `4.3.95` |
| Scene | `Assets/OptimizedSpine/Scenes/Benchmark_01_Baseline.unity` |
| Skeleton Asset | `Assets/Samples/spine-unity Runtime/4.3.95/Spine Examples/Spine Skeletons/spineboy-pro/spineboy-pro_SkeletonData.asset` |
| Animation | `run` |
| Instance Count | `25` |
| Baseline Snapshot | `docs/experiments/2026-07-23-164259-baseline.md` |
| Centralized Snapshot | `docs/experiments/2026-07-23-165117-centralizedupdate_25.md` |

## Result

| Metric | Baseline | CentralizedUpdate | Delta |
| --- | ---: | ---: | ---: |
| Average FPS | `282.8` | `244.1` | `-38.7` / `-13.7%` |
| Average Frame Time | `3.54 ms` | `4.10 ms` | `+0.56 ms` / `+15.8%` |
| Min Frame Time | `2.81 ms` | `3.21 ms` | `+0.40 ms` / `+14.2%` |
| Max Frame Time | `14.61 ms` | `14.87 ms` | `+0.26 ms` / `+1.8%` |
| Mono Used | `52.8 MB` | `56.7 MB` | `+3.9 MB` / `+7.4%` |
| Total Allocated | `230.9 MB` | `214.7 MB` | `-16.2 MB` / `-7.0%` |

## Conclusion

This single 25-instance run does not show a CPU/frame-time gain from `CentralizedUpdate`.
In this run, `CentralizedUpdate` was slower on average FPS and average frame time, while `Total Allocated` was lower.

Treat this as one measurement pair only. Repeat the same pair several times and add larger instance counts, such as `50` and `100`, before making a durable optimization conclusion.
