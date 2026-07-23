using System;
using System.Globalization;
using System.IO;
using UnityEngine;
using UnityEngine.Profiling;
using UnityEngine.SceneManagement;

namespace OptimizedSpine.Benchmark
{
    public sealed class SpineBenchmarkSnapshotRecorder : MonoBehaviour
    {
        [Header("采样对象")]
        [SerializeField, InspectorName("生成器"), Tooltip("读取实例数量、动画名和 Skeleton 数据的 benchmark 生成器。")]
        private SpineBenchmarkSpawner spawner;

        [Header("实验信息")]
        [SerializeField, InspectorName("实验名称"), Tooltip("写入 markdown 标题和文件名的实验名称。")]
        private string experimentName = "Baseline";

        [SerializeField, InspectorName("Skeleton 资源路径"), Tooltip("写入 snapshot 的 Skeleton 资源路径，用于保证实验记录可复现。")]
        private string skeletonAssetPath = string.Empty;

        [SerializeField, InspectorName("spine-unity 版本"), Tooltip("写入 snapshot 的 spine-unity 版本。")]
        private string spineUnityVersion = "4.3.95";

        [Header("采样窗口")]
        [SerializeField, InspectorName("预热秒数"), Min(0f), Tooltip("正式采样前等待的秒数，用来避开刚进入 Play Mode 的波动。")]
        private float warmupSeconds = 3f;

        [SerializeField, InspectorName("采样秒数"), Min(0.01f), Tooltip("正式记录 frame/memory 数据的持续时间。")]
        private float sampleSeconds = 10f;

        [Header("输出")]
        [SerializeField, InspectorName("输出目录"), Tooltip("相对于项目根目录的 markdown 输出目录。")]
        private string outputDirectory = "docs/experiments";

        private SpineBenchmarkFrameAccumulator accumulator;

        public bool IsComplete => accumulator != null && accumulator.IsComplete;
        public SpineBenchmarkSamplingStatus SamplingStatus => GetSummary().Status;
        public string SamplingStatusLabel => FormatSamplingStatus(GetSummary());

        private void OnEnable()
        {
            ResetSampling();
        }

        private void Update()
        {
            if (accumulator == null || accumulator.IsComplete)
                return;

            accumulator.RecordFrame(
                Time.unscaledDeltaTime,
                Profiler.GetMonoUsedSizeLong(),
                Profiler.GetTotalAllocatedMemoryLong());
        }

        [ContextMenu("Reset Snapshot Sampling")]
        public void ResetSampling()
        {
            accumulator = new SpineBenchmarkFrameAccumulator(warmupSeconds, sampleSeconds);
        }

        [ContextMenu("Write Benchmark Snapshot")]
        public string WriteSnapshot()
        {
            if (TryWriteSnapshot(out string path, out _))
                return path;

            return string.Empty;
        }

        public bool TryWriteSnapshot(out string path, out string reason)
        {
            SpineBenchmarkSnapshot snapshot = BuildSnapshot();
            if (!snapshot.CanExportAsMeasurement)
            {
                path = string.Empty;
                reason = BuildIncompleteSnapshotReason(snapshot);
                Debug.LogWarning(reason);
                return false;
            }

            path = WriteSnapshotFile(snapshot);
            reason = string.Empty;
            return true;
        }

        [ContextMenu("Write Partial Benchmark Snapshot For Debug")]
        public string WritePartialSnapshotForDebug()
        {
            return WriteSnapshotFile(BuildSnapshot());
        }

        private string WriteSnapshotFile(SpineBenchmarkSnapshot snapshot)
        {
            string markdown = SpineBenchmarkSnapshotMarkdown.ToMarkdown(snapshot);
            string projectRoot = Path.GetFullPath(Path.Combine(Application.dataPath, ".."));
            string directory = Path.GetFullPath(Path.Combine(projectRoot, outputDirectory));

            Directory.CreateDirectory(directory);

            string fileName = DateTime.Now.ToString("yyyy-MM-dd-HHmmss") + "-" + SanitizeFileName(snapshot.ExperimentName) + ".md";
            string path = Path.Combine(directory, fileName);
            File.WriteAllText(path, markdown);

            Debug.Log($"Wrote Spine benchmark snapshot: {path}");
            return path;
        }

        public SpineBenchmarkSnapshot BuildSnapshot()
        {
            if (accumulator == null)
                ResetSampling();

            SpineBenchmarkFrameSummary summary = GetSummary();
            Scene activeScene = SceneManager.GetActiveScene();

            return new SpineBenchmarkSnapshot
            {
                ExperimentName = string.IsNullOrWhiteSpace(experimentName) ? "Spine Benchmark Snapshot" : experimentName,
                CapturedAtLocal = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
                UnityVersion = Application.unityVersion,
                SpineUnityVersion = string.IsNullOrWhiteSpace(spineUnityVersion) ? "Unknown" : spineUnityVersion,
                ScenePath = string.IsNullOrWhiteSpace(activeScene.path) ? activeScene.name : activeScene.path,
                SkeletonAssetPath = ResolveSkeletonAssetPath(),
                AnimationName = spawner != null ? spawner.AnimationName : string.Empty,
                InstanceCount = spawner != null ? spawner.InstanceCount : 0,
                UpdateMode = spawner != null ? spawner.UpdateModeLabel : string.Empty,
                WarmupSeconds = summary.WarmupSeconds,
                TargetSampleSeconds = summary.TargetSampleSeconds,
                SampleSeconds = summary.SampleSeconds,
                SampleCount = summary.SampleCount,
                AverageFps = summary.AverageFps,
                AverageFrameMs = summary.AverageFrameMs,
                MinFrameMs = summary.MinFrameMs,
                MaxFrameMs = summary.MaxFrameMs,
                MonoUsedBytes = summary.MaxMonoUsedBytes,
                TotalAllocatedBytes = summary.MaxTotalAllocatedBytes,
                Completed = summary.Completed
            };
        }

        private SpineBenchmarkFrameSummary GetSummary()
        {
            if (accumulator == null)
                ResetSampling();

            return accumulator.ToSummary();
        }

        private static string FormatSamplingStatus(SpineBenchmarkFrameSummary summary)
        {
            return summary.Status + " Sample "
                + summary.SampleSeconds.ToString("0.##", CultureInfo.InvariantCulture)
                + "/"
                + summary.TargetSampleSeconds.ToString("0.##", CultureInfo.InvariantCulture)
                + " s";
        }

        private static string BuildIncompleteSnapshotReason(SpineBenchmarkSnapshot snapshot)
        {
            string status = snapshot.SampleCount > 0 ? "Partial" : "No Samples";

            if (snapshot.SampleCount <= 0)
                return "Benchmark snapshot is not ready: no samples have been recorded yet. Press Play and wait until the overlay shows Snapshot: Complete before writing.";

            return "Benchmark snapshot is not ready: status is " + status
                + ", actual sample window is " + snapshot.SampleSeconds.ToString("0.##", CultureInfo.InvariantCulture) + " s"
                + " of target " + snapshot.TargetSampleSeconds.ToString("0.##", CultureInfo.InvariantCulture) + " s. Wait until Snapshot: Complete before writing.";
        }

        private string ResolveSkeletonAssetPath()
        {
            if (!string.IsNullOrWhiteSpace(skeletonAssetPath))
                return skeletonAssetPath;

            if (spawner != null && spawner.SkeletonDataAsset != null)
                return spawner.SkeletonDataAsset.name;

            return string.Empty;
        }

        private static string SanitizeFileName(string value)
        {
            string safe = string.IsNullOrWhiteSpace(value) ? "snapshot" : value.Trim();

            foreach (char invalidChar in Path.GetInvalidFileNameChars())
                safe = safe.Replace(invalidChar, '-');

            return safe.Replace(' ', '-').ToLowerInvariant();
        }
    }
}
