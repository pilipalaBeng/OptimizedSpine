namespace OptimizedSpine.EditorTools.Benchmarking
{
    public static class SpineBenchmarkSnapshotComparer
    {
        public static SpineBenchmarkSnapshotComparison Compare(
            SpineBenchmarkSnapshotRecord baseline,
            SpineBenchmarkSnapshotRecord candidate)
        {
            SpineBenchmarkSnapshotComparison comparison = new SpineBenchmarkSnapshotComparison(baseline, candidate);

            AddContextWarnings(comparison, baseline, candidate);
            comparison.AddMetric(new SpineBenchmarkMetricComparison("Average FPS", "fps", baseline.AverageFps, candidate.AverageFps, lowerIsBetter: false));
            comparison.AddMetric(new SpineBenchmarkMetricComparison("Average Frame Time", "ms", baseline.AverageFrameMs, candidate.AverageFrameMs, lowerIsBetter: true));
            comparison.AddMetric(new SpineBenchmarkMetricComparison("Max Frame Time", "ms", baseline.MaxFrameMs, candidate.MaxFrameMs, lowerIsBetter: true));
            comparison.AddMetric(new SpineBenchmarkMetricComparison("Mono Used", "MB", ToMebibytes(baseline.MonoUsedBytes), ToMebibytes(candidate.MonoUsedBytes), lowerIsBetter: true));
            comparison.AddMetric(new SpineBenchmarkMetricComparison("Total Allocated", "MB", ToMebibytes(baseline.TotalAllocatedBytes), ToMebibytes(candidate.TotalAllocatedBytes), lowerIsBetter: true));

            return comparison;
        }

        private static void AddContextWarnings(
            SpineBenchmarkSnapshotComparison comparison,
            SpineBenchmarkSnapshotRecord baseline,
            SpineBenchmarkSnapshotRecord candidate)
        {
            AddWarningIfDifferent(comparison, "Instance Count", baseline.InstanceCount.ToString(), candidate.InstanceCount.ToString());
            AddWarningIfDifferent(comparison, "Scene", baseline.ScenePath, candidate.ScenePath);
            AddWarningIfDifferent(comparison, "Skeleton Asset", baseline.SkeletonAssetPath, candidate.SkeletonAssetPath);
            AddWarningIfDifferent(comparison, "Animation", baseline.AnimationName, candidate.AnimationName);
            AddWarningIfDifferent(comparison, "Unity", baseline.UnityVersion, candidate.UnityVersion);
            AddWarningIfDifferent(comparison, "spine-unity", baseline.SpineUnityVersion, candidate.SpineUnityVersion);

            if (baseline.Status != "Complete" || candidate.Status != "Complete")
                comparison.AddContextWarning($"Snapshot status is not complete: baseline '{baseline.Status}', candidate '{candidate.Status}'.");
        }

        private static void AddWarningIfDifferent(
            SpineBenchmarkSnapshotComparison comparison,
            string label,
            string baselineValue,
            string candidateValue)
        {
            if (baselineValue != candidateValue)
                comparison.AddContextWarning($"{label} mismatch: baseline '{baselineValue}', candidate '{candidateValue}'.");
        }

        private static double ToMebibytes(long bytes)
        {
            return bytes / 1024d / 1024d;
        }
    }
}
