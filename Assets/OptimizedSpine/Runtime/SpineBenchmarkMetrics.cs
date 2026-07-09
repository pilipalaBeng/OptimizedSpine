using System.Text;
using UnityEngine;
using UnityEngine.Profiling;

namespace OptimizedSpine.Benchmark
{
    public sealed class SpineBenchmarkMetrics : MonoBehaviour
    {
        [Header("显示来源")]
        [SerializeField, InspectorName("生成器"), Tooltip("读取实例数量的 Spine benchmark 生成器。")]
        private SpineBenchmarkSpawner spawner;

        [Header("显示设置")]
        [SerializeField, InspectorName("平滑系数"), Min(0.01f), Tooltip("FPS 和 frame time 的平滑速度，数值越大越贴近瞬时变化。")]
        private float smoothing = 0.1f;

        [SerializeField, InspectorName("目标帧率"), Tooltip("运行时设置 Application.targetFrameRate；-1 表示使用平台默认值。")]
        private int targetFrameRate = -1;

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
