using NUnit.Framework;
using OptimizedSpine.EditorTools.Benchmarking;

namespace OptimizedSpine.Tests
{
    public sealed class SpineBenchmarkSnapshotComparisonTests
    {
        [Test]
        public void Parse_ReadsGeneratedSnapshotMarkdown()
        {
            SpineBenchmarkSnapshotRecord record = SpineBenchmarkSnapshotParser.Parse(SampleSnapshot(
                instanceCount: 25,
                averageFps: 286.8,
                averageFrameMs: 3.49,
                maxFrameMs: 12.94,
                monoUsed: "33.5 MB",
                totalAllocated: "173.9 MB"));

            Assert.That(record.ExperimentName, Is.EqualTo("Baseline"));
            Assert.That(record.UnityVersion, Is.EqualTo("2022.3.62f2"));
            Assert.That(record.SpineUnityVersion, Is.EqualTo("4.3.95"));
            Assert.That(record.ScenePath, Is.EqualTo("Assets/OptimizedSpine/Scenes/Benchmark_01_Baseline.unity"));
            Assert.That(record.AnimationName, Is.EqualTo("run"));
            Assert.That(record.InstanceCount, Is.EqualTo(25));
            Assert.That(record.WarmupSeconds, Is.EqualTo(3d).Within(0.001));
            Assert.That(record.TargetSampleSeconds, Is.EqualTo(10d).Within(0.001));
            Assert.That(record.ActualSampleSeconds, Is.EqualTo(10d).Within(0.001));
            Assert.That(record.AverageFps, Is.EqualTo(286.8).Within(0.001));
            Assert.That(record.AverageFrameMs, Is.EqualTo(3.49).Within(0.001));
            Assert.That(record.MaxFrameMs, Is.EqualTo(12.94).Within(0.001));
            Assert.That(record.MonoUsedBytes, Is.EqualTo(35127296));
            Assert.That(record.TotalAllocatedBytes, Is.EqualTo(182347366));
        }

        [Test]
        public void Compare_ComputesDirectionAndContextWarnings()
        {
            SpineBenchmarkSnapshotRecord baseline = SpineBenchmarkSnapshotParser.Parse(SampleSnapshot(
                instanceCount: 25,
                averageFps: 200,
                averageFrameMs: 5,
                maxFrameMs: 8,
                monoUsed: "40 MB",
                totalAllocated: "180 MB"));

            SpineBenchmarkSnapshotRecord candidate = SpineBenchmarkSnapshotParser.Parse(SampleSnapshot(
                instanceCount: 20,
                averageFps: 250,
                averageFrameMs: 4,
                maxFrameMs: 7,
                monoUsed: "32 MB",
                totalAllocated: "160 MB"));

            SpineBenchmarkSnapshotComparison comparison = SpineBenchmarkSnapshotComparer.Compare(baseline, candidate);

            Assert.That(comparison.ContextWarnings, Has.Some.Contains("Instance Count"));

            SpineBenchmarkMetricComparison fps = comparison.GetMetric("Average FPS");
            Assert.That(fps.Delta, Is.EqualTo(50).Within(0.001));
            Assert.That(fps.PercentChange, Is.EqualTo(25).Within(0.001));
            Assert.That(fps.IsImprovement, Is.True);

            SpineBenchmarkMetricComparison frameTime = comparison.GetMetric("Average Frame Time");
            Assert.That(frameTime.Delta, Is.EqualTo(-1).Within(0.001));
            Assert.That(frameTime.PercentChange, Is.EqualTo(-20).Within(0.001));
            Assert.That(frameTime.IsImprovement, Is.True);
        }

        [Test]
        public void Parse_ReadsTargetAndActualSampleWindows()
        {
            SpineBenchmarkSnapshotRecord record = SpineBenchmarkSnapshotParser.Parse(SampleSnapshotWithWindows(
                targetSampleWindow: "10 s",
                actualSampleWindow: "4.25 s",
                status: "Partial"));

            Assert.That(record.TargetSampleSeconds, Is.EqualTo(10d).Within(0.001));
            Assert.That(record.ActualSampleSeconds, Is.EqualTo(4.25d).Within(0.001));
            Assert.That(record.Status, Is.EqualTo("Partial"));
        }

        [Test]
        public void Compare_WarnsWhenSampleWindowsMismatchOrAreIncomplete()
        {
            SpineBenchmarkSnapshotRecord baseline = SpineBenchmarkSnapshotParser.Parse(SampleSnapshotWithWindows(
                targetSampleWindow: "10 s",
                actualSampleWindow: "10 s",
                status: "Complete"));

            SpineBenchmarkSnapshotRecord candidate = SpineBenchmarkSnapshotParser.Parse(SampleSnapshotWithWindows(
                targetSampleWindow: "5 s",
                actualSampleWindow: "4.25 s",
                status: "Complete"));

            SpineBenchmarkSnapshotComparison comparison = SpineBenchmarkSnapshotComparer.Compare(baseline, candidate);

            Assert.That(comparison.ContextWarnings, Has.Some.Contains("Target Sample Window"));
            Assert.That(comparison.ContextWarnings, Has.Some.Contains("Candidate actual sample window is shorter than target"));
        }

        private static string SampleSnapshot(
            int instanceCount,
            double averageFps,
            double averageFrameMs,
            double maxFrameMs,
            string monoUsed,
            string totalAllocated)
        {
            return $@"# Baseline

## Context

| Field | Value |
| --- | --- |
| Captured At | `2026-07-09 19:43:13` |
| Unity | `2022.3.62f2` |
| spine-unity | `4.3.95` |
| Scene | `Assets/OptimizedSpine/Scenes/Benchmark_01_Baseline.unity` |
| Skeleton Asset | `Assets/Samples/spineboy-pro_SkeletonData.asset` |
| Animation | `run` |
| Instance Count | `{instanceCount}` |
| Warmup | `3 s` |
| Sample Window | `10 s` |
| Status | `Complete` |

## Metrics

| Field | Value |
| --- | --- |
| Sample Count | `2868` |
| Average FPS | `{averageFps}` |
| Average Frame Time | `{averageFrameMs} ms` |
| Min Frame Time | `2.86 ms` |
| Max Frame Time | `{maxFrameMs} ms` |
| Mono Used | `{monoUsed}` |
| Total Allocated | `{totalAllocated}` |

## Conclusion

Raw snapshot only. Compare against another snapshot before claiming an optimization gain.";
        }

        private static string SampleSnapshotWithWindows(
            string targetSampleWindow,
            string actualSampleWindow,
            string status)
        {
            return $@"# Baseline_25

## Context

| Field | Value |
| --- | --- |
| Captured At | `2026-07-10 11:32:51` |
| Unity | `2022.3.62f2` |
| spine-unity | `4.3.95` |
| Scene | `Assets/OptimizedSpine/Scenes/Benchmark_01_Baseline.unity` |
| Skeleton Asset | `Assets/Samples/spineboy-pro_SkeletonData.asset` |
| Animation | `run` |
| Instance Count | `25` |
| Warmup | `3 s` |
| Target Sample Window | `{targetSampleWindow}` |
| Actual Sample Window | `{actualSampleWindow}` |
| Status | `{status}` |

## Metrics

| Field | Value |
| --- | --- |
| Sample Count | `2868` |
| Average FPS | `286.8` |
| Average Frame Time | `3.49 ms` |
| Min Frame Time | `2.86 ms` |
| Max Frame Time | `12.94 ms` |
| Mono Used | `33.5 MB` |
| Total Allocated | `173.9 MB` |

## Conclusion

Raw snapshot only. Compare against another snapshot before claiming an optimization gain.";
        }
    }
}
