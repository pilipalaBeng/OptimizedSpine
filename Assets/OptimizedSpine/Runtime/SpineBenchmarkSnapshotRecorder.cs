using System;
using System.IO;
using UnityEngine;
using UnityEngine.Profiling;
using UnityEngine.SceneManagement;

namespace OptimizedSpine.Benchmark
{
    public sealed class SpineBenchmarkSnapshotRecorder : MonoBehaviour
    {
        [SerializeField] private SpineBenchmarkSpawner spawner;
        [SerializeField] private string experimentName = "Baseline";
        [SerializeField] private string skeletonAssetPath = string.Empty;
        [SerializeField] private string spineUnityVersion = "4.3.95";
        [SerializeField, Min(0f)] private float warmupSeconds = 3f;
        [SerializeField, Min(0.01f)] private float sampleSeconds = 10f;
        [SerializeField] private string outputDirectory = "docs/experiments";

        private SpineBenchmarkFrameAccumulator accumulator;

        public bool IsComplete => accumulator != null && accumulator.IsComplete;

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
            SpineBenchmarkSnapshot snapshot = BuildSnapshot();
            string markdown = SpineBenchmarkSnapshotMarkdown.ToMarkdown(snapshot);
            string projectRoot = Path.GetFullPath(Path.Combine(Application.dataPath, ".."));
            string directory = Path.GetFullPath(Path.Combine(projectRoot, outputDirectory));

            Directory.CreateDirectory(directory);

            string fileName = DateTime.Now.ToString("yyyy-MM-dd-HHmmss") + "-" + SanitizeFileName(snapshot.ExperimentName) + ".md";
            string path = Path.Combine(directory, fileName);
            File.WriteAllText(path, markdown);

            Debug.Log($"Wrote Spine benchmark snapshot: {path}", this);
            return path;
        }

        public SpineBenchmarkSnapshot BuildSnapshot()
        {
            if (accumulator == null)
                ResetSampling();

            SpineBenchmarkFrameSummary summary = accumulator.ToSummary();
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
                WarmupSeconds = summary.WarmupSeconds,
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
