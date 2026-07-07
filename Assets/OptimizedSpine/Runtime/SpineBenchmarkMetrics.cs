using System.Text;
using UnityEngine;
using UnityEngine.Profiling;

namespace OptimizedSpine.Benchmark
{
    public sealed class SpineBenchmarkMetrics : MonoBehaviour
    {
        [SerializeField] private SpineBenchmarkSpawner spawner;
        [SerializeField, Min(0.01f)] private float smoothing = 0.1f;
        [SerializeField] private int targetFrameRate = -1;

        private readonly StringBuilder textBuilder = new StringBuilder(256);
        private float smoothedDeltaTime;
        private GUIStyle labelStyle;

        private void Awake()
        {
            Application.targetFrameRate = targetFrameRate;
            smoothedDeltaTime = Time.unscaledDeltaTime;
        }

        private void OnGUI()
        {
            EnsureStyle();

            smoothedDeltaTime += (Time.unscaledDeltaTime - smoothedDeltaTime) * smoothing;
            float frameMs = smoothedDeltaTime * 1000f;
            float fps = smoothedDeltaTime > 0f ? 1f / smoothedDeltaTime : 0f;

            textBuilder.Clear();
            textBuilder.AppendLine("Optimized Spine Baseline");
            textBuilder.Append("Instances: ").Append(spawner != null ? spawner.SpawnedCount : 0).AppendLine();
            textBuilder.Append("FPS: ").Append(fps.ToString("0.0")).AppendLine();
            textBuilder.Append("Frame: ").Append(frameMs.ToString("0.00")).AppendLine(" ms");
            textBuilder.Append("Mono Used: ").Append(FormatBytes(Profiler.GetMonoUsedSizeLong())).AppendLine();
            textBuilder.Append("Total Allocated: ").Append(FormatBytes(Profiler.GetTotalAllocatedMemoryLong()));

            GUI.Label(new Rect(12f, 12f, 520f, 240f), textBuilder.ToString(), labelStyle);
        }

        private void EnsureStyle()
        {
            if (labelStyle != null)
                return;

            labelStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = 32,
                normal = { textColor = Color.red },
                padding = new RectOffset(10, 10, 8, 8)
            };
        }

        private static string FormatBytes(long bytes)
        {
            const float kib = 1024f;
            const float mib = kib * 1024f;

            if (bytes >= mib)
                return (bytes / mib).ToString("0.0") + " MB";

            if (bytes >= kib)
                return (bytes / kib).ToString("0.0") + " KB";

            return bytes + " B";
        }
    }
}
