using System.Globalization;
using System.Text;

namespace OptimizedSpine.Benchmark
{
    public sealed class SpineBenchmarkSnapshot
    {
        public string ExperimentName { get; set; } = "Spine Benchmark Snapshot";
        public string CapturedAtLocal { get; set; } = string.Empty;
        public string UnityVersion { get; set; } = string.Empty;
        public string SpineUnityVersion { get; set; } = string.Empty;
        public string ScenePath { get; set; } = string.Empty;
        public string SkeletonAssetPath { get; set; } = string.Empty;
        public string AnimationName { get; set; } = string.Empty;
        public int InstanceCount { get; set; }
        public float WarmupSeconds { get; set; }
        public float SampleSeconds { get; set; }
        public int SampleCount { get; set; }
        public float AverageFps { get; set; }
        public float AverageFrameMs { get; set; }
        public float MinFrameMs { get; set; }
        public float MaxFrameMs { get; set; }
        public long MonoUsedBytes { get; set; }
        public long TotalAllocatedBytes { get; set; }
        public bool Completed { get; set; }
    }

    public readonly struct SpineBenchmarkFrameSummary
    {
        public SpineBenchmarkFrameSummary(
            float warmupSeconds,
            float sampleSeconds,
            int sampleCount,
            float averageFps,
            float averageFrameMs,
            float minFrameMs,
            float maxFrameMs,
            long maxMonoUsedBytes,
            long maxTotalAllocatedBytes,
            bool completed)
        {
            WarmupSeconds = warmupSeconds;
            SampleSeconds = sampleSeconds;
            SampleCount = sampleCount;
            AverageFps = averageFps;
            AverageFrameMs = averageFrameMs;
            MinFrameMs = minFrameMs;
            MaxFrameMs = maxFrameMs;
            MaxMonoUsedBytes = maxMonoUsedBytes;
            MaxTotalAllocatedBytes = maxTotalAllocatedBytes;
            Completed = completed;
        }

        public float WarmupSeconds { get; }
        public float SampleSeconds { get; }
        public int SampleCount { get; }
        public float AverageFps { get; }
        public float AverageFrameMs { get; }
        public float MinFrameMs { get; }
        public float MaxFrameMs { get; }
        public long MaxMonoUsedBytes { get; }
        public long MaxTotalAllocatedBytes { get; }
        public bool Completed { get; }
    }

    public sealed class SpineBenchmarkFrameAccumulator
    {
        private readonly float warmupSeconds;
        private readonly float targetSampleSeconds;

        private float elapsedSeconds;
        private float sampledSeconds;
        private float totalFrameSeconds;
        private float minFrameMs = float.MaxValue;
        private float maxFrameMs;
        private int sampleCount;
        private long maxMonoUsedBytes;
        private long maxTotalAllocatedBytes;

        public SpineBenchmarkFrameAccumulator(float warmupSeconds, float sampleSeconds)
        {
            this.warmupSeconds = warmupSeconds > 0f ? warmupSeconds : 0f;
            targetSampleSeconds = sampleSeconds > 0f ? sampleSeconds : 0.01f;
        }

        public bool IsComplete => sampledSeconds >= targetSampleSeconds;

        public void RecordFrame(float deltaSeconds, long monoUsedBytes, long totalAllocatedBytes)
        {
            if (deltaSeconds <= 0f)
                return;

            float frameStartSeconds = elapsedSeconds;
            elapsedSeconds += deltaSeconds;

            if (frameStartSeconds < warmupSeconds || IsComplete)
                return;

            sampleCount++;
            sampledSeconds += deltaSeconds;
            totalFrameSeconds += deltaSeconds;

            float frameMs = deltaSeconds * 1000f;
            if (frameMs < minFrameMs)
                minFrameMs = frameMs;
            if (frameMs > maxFrameMs)
                maxFrameMs = frameMs;

            if (monoUsedBytes > maxMonoUsedBytes)
                maxMonoUsedBytes = monoUsedBytes;
            if (totalAllocatedBytes > maxTotalAllocatedBytes)
                maxTotalAllocatedBytes = totalAllocatedBytes;
        }

        public SpineBenchmarkFrameSummary ToSummary()
        {
            float averageFrameMs = sampleCount > 0 ? totalFrameSeconds / sampleCount * 1000f : 0f;
            float averageFps = totalFrameSeconds > 0f ? sampleCount / totalFrameSeconds : 0f;

            return new SpineBenchmarkFrameSummary(
                warmupSeconds,
                sampledSeconds,
                sampleCount,
                averageFps,
                averageFrameMs,
                sampleCount > 0 ? minFrameMs : 0f,
                sampleCount > 0 ? maxFrameMs : 0f,
                maxMonoUsedBytes,
                maxTotalAllocatedBytes,
                IsComplete);
        }
    }

    public static class SpineBenchmarkSnapshotMarkdown
    {
        public static string ToMarkdown(SpineBenchmarkSnapshot snapshot)
        {
            StringBuilder builder = new StringBuilder(2048);
            string title = string.IsNullOrWhiteSpace(snapshot.ExperimentName)
                ? "Spine Benchmark Snapshot"
                : snapshot.ExperimentName;

            builder.Append("# ").AppendLine(title);
            builder.AppendLine();
            builder.AppendLine("## Context");
            builder.AppendLine();
            AppendTableHeader(builder);
            AppendRow(builder, "Captured At", snapshot.CapturedAtLocal);
            AppendRow(builder, "Unity", snapshot.UnityVersion);
            AppendRow(builder, "spine-unity", snapshot.SpineUnityVersion);
            AppendRow(builder, "Scene", snapshot.ScenePath);
            AppendRow(builder, "Skeleton Asset", snapshot.SkeletonAssetPath);
            AppendRow(builder, "Animation", snapshot.AnimationName);
            AppendRow(builder, "Instance Count", snapshot.InstanceCount.ToString(CultureInfo.InvariantCulture));
            AppendRow(builder, "Warmup", FormatSeconds(snapshot.WarmupSeconds));
            AppendRow(builder, "Sample Window", FormatSeconds(snapshot.SampleSeconds));
            AppendRow(builder, "Status", snapshot.Completed ? "Complete" : "Partial");

            builder.AppendLine();
            builder.AppendLine("## Metrics");
            builder.AppendLine();
            AppendTableHeader(builder);
            AppendRow(builder, "Sample Count", snapshot.SampleCount.ToString(CultureInfo.InvariantCulture));
            AppendRow(builder, "Average FPS", snapshot.AverageFps.ToString("0.0", CultureInfo.InvariantCulture));
            AppendRow(builder, "Average Frame Time", snapshot.AverageFrameMs.ToString("0.00", CultureInfo.InvariantCulture) + " ms");
            AppendRow(builder, "Min Frame Time", snapshot.MinFrameMs.ToString("0.00", CultureInfo.InvariantCulture) + " ms");
            AppendRow(builder, "Max Frame Time", snapshot.MaxFrameMs.ToString("0.00", CultureInfo.InvariantCulture) + " ms");
            AppendRow(builder, "Mono Used", FormatBytes(snapshot.MonoUsedBytes));
            AppendRow(builder, "Total Allocated", FormatBytes(snapshot.TotalAllocatedBytes));

            builder.AppendLine();
            builder.AppendLine("## Conclusion");
            builder.AppendLine();
            builder.AppendLine("Raw snapshot only. Compare against another snapshot before claiming an optimization gain.");

            return builder.ToString();
        }

        private static void AppendTableHeader(StringBuilder builder)
        {
            builder.AppendLine("| Field | Value |");
            builder.AppendLine("| --- | --- |");
        }

        private static void AppendRow(StringBuilder builder, string label, string value)
        {
            builder.Append("| ")
                .Append(label)
                .Append(" | `")
                .Append(string.IsNullOrWhiteSpace(value) ? "Unknown" : value)
                .AppendLine("` |");
        }

        private static string FormatSeconds(float seconds)
        {
            return seconds.ToString("0.##", CultureInfo.InvariantCulture) + " s";
        }

        public static string FormatBytes(long bytes)
        {
            const float kib = 1024f;
            const float mib = kib * 1024f;

            if (bytes >= mib)
                return (bytes / mib).ToString("0.0", CultureInfo.InvariantCulture) + " MB";

            if (bytes >= kib)
                return (bytes / kib).ToString("0.0", CultureInfo.InvariantCulture) + " KB";

            return bytes.ToString(CultureInfo.InvariantCulture) + " B";
        }
    }
}
