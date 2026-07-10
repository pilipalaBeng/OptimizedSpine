using NUnit.Framework;
using OptimizedSpine.Benchmark;

namespace OptimizedSpine.Tests
{
    public sealed class SpineBenchmarkSnapshotTests
    {
        [Test]
        public void ToMarkdown_IncludesMeasurementContextAndRawMetrics()
        {
            SpineBenchmarkSnapshot snapshot = new SpineBenchmarkSnapshot
            {
                ExperimentName = "Baseline 25",
                CapturedAtLocal = "2026-07-09 12:34:56",
                UnityVersion = "2022.3.62f2",
                SpineUnityVersion = "4.3.95",
                ScenePath = "Assets/OptimizedSpine/Scenes/Benchmark_01_Baseline.unity",
                SkeletonAssetPath = "Assets/Samples/spineboy-pro_SkeletonData.asset",
                AnimationName = "run",
                InstanceCount = 25,
                WarmupSeconds = 3f,
                TargetSampleSeconds = 10f,
                SampleSeconds = 10f,
                SampleCount = 600,
                AverageFps = 60f,
                AverageFrameMs = 16.6667f,
                MinFrameMs = 15f,
                MaxFrameMs = 20f,
                MonoUsedBytes = 12 * 1024 * 1024,
                TotalAllocatedBytes = 128 * 1024 * 1024,
                Completed = true
            };

            string markdown = SpineBenchmarkSnapshotMarkdown.ToMarkdown(snapshot);

            Assert.That(markdown, Does.Contain("# Baseline 25"));
            Assert.That(markdown, Does.Contain("| Unity | `2022.3.62f2` |"));
            Assert.That(markdown, Does.Contain("| spine-unity | `4.3.95` |"));
            Assert.That(markdown, Does.Contain("| Instance Count | `25` |"));
            Assert.That(markdown, Does.Contain("| Target Sample Window | `10 s` |"));
            Assert.That(markdown, Does.Contain("| Actual Sample Window | `10 s` |"));
            Assert.That(markdown, Does.Contain("| Average FPS | `60.0` |"));
            Assert.That(markdown, Does.Contain("| Average Frame Time | `16.67 ms` |"));
            Assert.That(markdown, Does.Contain("| Mono Used | `12.0 MB` |"));
            Assert.That(markdown, Does.Contain("Raw snapshot only"));
        }

        [Test]
        public void FrameAccumulator_IgnoresWarmupAndStopsAtSampleDuration()
        {
            SpineBenchmarkFrameAccumulator accumulator = new SpineBenchmarkFrameAccumulator(
                warmupSeconds: 1f,
                sampleSeconds: 0.5f);

            accumulator.RecordFrame(deltaSeconds: 0.5f, monoUsedBytes: 1, totalAllocatedBytes: 2);
            accumulator.RecordFrame(deltaSeconds: 0.5f, monoUsedBytes: 2, totalAllocatedBytes: 3);
            accumulator.RecordFrame(deltaSeconds: 0.25f, monoUsedBytes: 3, totalAllocatedBytes: 4);
            accumulator.RecordFrame(deltaSeconds: 0.25f, monoUsedBytes: 4, totalAllocatedBytes: 5);
            accumulator.RecordFrame(deltaSeconds: 0.25f, monoUsedBytes: 9, totalAllocatedBytes: 9);

            SpineBenchmarkFrameSummary summary = accumulator.ToSummary();

            Assert.That(summary.Completed, Is.True);
            Assert.That(summary.Status, Is.EqualTo(SpineBenchmarkSamplingStatus.Complete));
            Assert.That(summary.TargetSampleSeconds, Is.EqualTo(0.5f).Within(0.0001f));
            Assert.That(summary.SampleCount, Is.EqualTo(2));
            Assert.That(summary.SampleSeconds, Is.EqualTo(0.5f).Within(0.0001f));
            Assert.That(summary.AverageFps, Is.EqualTo(4f).Within(0.0001f));
            Assert.That(summary.AverageFrameMs, Is.EqualTo(250f).Within(0.0001f));
            Assert.That(summary.MaxMonoUsedBytes, Is.EqualTo(4));
            Assert.That(summary.MaxTotalAllocatedBytes, Is.EqualTo(5));
        }

        [Test]
        public void FrameAccumulator_ReportsWarmupSamplingAndCompleteStatus()
        {
            SpineBenchmarkFrameAccumulator accumulator = new SpineBenchmarkFrameAccumulator(
                warmupSeconds: 1f,
                sampleSeconds: 0.5f);

            Assert.That(accumulator.Status, Is.EqualTo(SpineBenchmarkSamplingStatus.WarmingUp));

            accumulator.RecordFrame(deltaSeconds: 0.5f, monoUsedBytes: 1, totalAllocatedBytes: 1);
            Assert.That(accumulator.Status, Is.EqualTo(SpineBenchmarkSamplingStatus.WarmingUp));

            accumulator.RecordFrame(deltaSeconds: 0.5f, monoUsedBytes: 1, totalAllocatedBytes: 1);
            Assert.That(accumulator.Status, Is.EqualTo(SpineBenchmarkSamplingStatus.Sampling));

            accumulator.RecordFrame(deltaSeconds: 0.25f, monoUsedBytes: 1, totalAllocatedBytes: 1);
            Assert.That(accumulator.Status, Is.EqualTo(SpineBenchmarkSamplingStatus.Sampling));

            accumulator.RecordFrame(deltaSeconds: 0.25f, monoUsedBytes: 1, totalAllocatedBytes: 1);
            Assert.That(accumulator.Status, Is.EqualTo(SpineBenchmarkSamplingStatus.Complete));
        }

        [Test]
        public void Snapshot_CanExportAsMeasurementRequiresCompleteSamples()
        {
            SpineBenchmarkSnapshot noSamples = new SpineBenchmarkSnapshot
            {
                Completed = true,
                SampleCount = 0
            };

            SpineBenchmarkSnapshot partial = new SpineBenchmarkSnapshot
            {
                Completed = false,
                SampleCount = 10
            };

            SpineBenchmarkSnapshot complete = new SpineBenchmarkSnapshot
            {
                Completed = true,
                SampleCount = 10
            };

            Assert.That(noSamples.CanExportAsMeasurement, Is.False);
            Assert.That(partial.CanExportAsMeasurement, Is.False);
            Assert.That(complete.CanExportAsMeasurement, Is.True);
        }
    }
}
