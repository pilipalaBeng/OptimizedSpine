using System;
using System.Globalization;
using System.IO;
using OptimizedSpine.EditorTools.Benchmarking;
using UnityEditor;
using UnityEngine;

namespace OptimizedSpine.EditorTools
{
    public sealed class SpineBenchmarkSnapshotCompareWindow : EditorWindow
    {
        private string baselinePath = string.Empty;
        private string candidatePath = string.Empty;
        private string errorMessage = string.Empty;
        private SpineBenchmarkSnapshotComparison comparison;
        private Vector2 scroll;

        [MenuItem("OptimizedSpine/Compare Benchmark Snapshots")]
        public static void Open()
        {
            GetWindow<SpineBenchmarkSnapshotCompareWindow>("Snapshot Compare");
        }

        private void OnEnable()
        {
            titleContent = new GUIContent("Snapshot Compare");
            TryUseLatestTwoSnapshots();
        }

        private void OnGUI()
        {
            EditorGUILayout.LabelField("Benchmark Snapshot 对比", EditorStyles.boldLabel);
            EditorGUILayout.HelpBox("选择两份 Write Benchmark Snapshot 导出的 markdown。Baseline 是改动前，Candidate 是改动后。", MessageType.Info);

            DrawPathPicker("Baseline", ref baselinePath);
            DrawPathPicker("Candidate", ref candidatePath);

            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("使用最新两份 Snapshot"))
                TryUseLatestTwoSnapshots();

            if (GUILayout.Button("Compare / 对比"))
                LoadAndCompare();
            EditorGUILayout.EndHorizontal();

            if (!string.IsNullOrEmpty(errorMessage))
                EditorGUILayout.HelpBox(errorMessage, MessageType.Error);

            if (comparison == null)
                return;

            scroll = EditorGUILayout.BeginScrollView(scroll);
            DrawSummary(comparison);
            DrawContextWarnings(comparison);
            DrawMetrics(comparison);
            EditorGUILayout.EndScrollView();
        }

        private static void DrawPathPicker(string label, ref string path)
        {
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField(label, GUILayout.Width(78f));
            path = EditorGUILayout.TextField(path);

            if (GUILayout.Button("选择", GUILayout.Width(56f)))
            {
                string selected = EditorUtility.OpenFilePanel(
                    $"Select {label} Snapshot",
                    DefaultExperimentDirectory(),
                    "md");

                if (!string.IsNullOrEmpty(selected))
                    path = ToProjectRelativePath(selected);
            }

            EditorGUILayout.EndHorizontal();
        }

        private void LoadAndCompare()
        {
            errorMessage = string.Empty;
            comparison = null;

            try
            {
                string baselineFullPath = ToFullPath(baselinePath);
                string candidateFullPath = ToFullPath(candidatePath);

                if (!File.Exists(baselineFullPath))
                    throw new FileNotFoundException("Baseline snapshot 不存在。", baselineFullPath);

                if (!File.Exists(candidateFullPath))
                    throw new FileNotFoundException("Candidate snapshot 不存在。", candidateFullPath);

                SpineBenchmarkSnapshotRecord baseline = SpineBenchmarkSnapshotParser.Parse(
                    File.ReadAllText(baselineFullPath),
                    baselinePath);

                SpineBenchmarkSnapshotRecord candidate = SpineBenchmarkSnapshotParser.Parse(
                    File.ReadAllText(candidateFullPath),
                    candidatePath);

                comparison = SpineBenchmarkSnapshotComparer.Compare(baseline, candidate);
            }
            catch (Exception exception)
            {
                errorMessage = exception.Message;
            }
        }

        private void TryUseLatestTwoSnapshots()
        {
            string directory = DefaultExperimentDirectory();
            if (!Directory.Exists(directory))
                return;

            string[] snapshots = Directory.GetFiles(directory, "*.md", SearchOption.TopDirectoryOnly);
            Array.Sort(snapshots, (left, right) => File.GetLastWriteTimeUtc(left).CompareTo(File.GetLastWriteTimeUtc(right)));

            if (snapshots.Length < 2)
                return;

            baselinePath = ToProjectRelativePath(snapshots[snapshots.Length - 2]);
            candidatePath = ToProjectRelativePath(snapshots[snapshots.Length - 1]);
            LoadAndCompare();
        }

        private static void DrawSummary(SpineBenchmarkSnapshotComparison comparison)
        {
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("对比对象", EditorStyles.boldLabel);
            EditorGUILayout.LabelField("Baseline", comparison.Baseline.SourcePath);
            EditorGUILayout.LabelField("Candidate", comparison.Candidate.SourcePath);
            EditorGUILayout.LabelField("实例数", $"{comparison.Baseline.InstanceCount} -> {comparison.Candidate.InstanceCount}");
            EditorGUILayout.LabelField("动画", $"{comparison.Baseline.AnimationName} -> {comparison.Candidate.AnimationName}");
            EditorGUILayout.LabelField("采样窗口", $"{FormatSampleWindow(comparison.Baseline)} -> {FormatSampleWindow(comparison.Candidate)}");
        }

        private static void DrawContextWarnings(SpineBenchmarkSnapshotComparison comparison)
        {
            EditorGUILayout.Space();
            if (comparison.ContextWarnings.Count == 0)
            {
                EditorGUILayout.HelpBox("上下文一致，可以作为一次候选对照。仍建议同条件重复跑 2-3 次。", MessageType.Info);
                return;
            }

            foreach (string warning in comparison.ContextWarnings)
                EditorGUILayout.HelpBox(warning, MessageType.Warning);
        }

        private static void DrawMetrics(SpineBenchmarkSnapshotComparison comparison)
        {
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("指标差异", EditorStyles.boldLabel);

            foreach (SpineBenchmarkMetricComparison metric in comparison.Metrics)
            {
                string direction = metric.Delta == 0d
                    ? "持平"
                    : metric.IsImprovement ? "改善" : "变差";

                string line =
                    $"{metric.Name}: {FormatValue(metric.BaselineValue, metric.Unit)} -> {FormatValue(metric.CandidateValue, metric.Unit)}" +
                    $"  Delta {FormatSigned(metric.Delta, metric.Unit)} ({FormatSignedPercent(metric.PercentChange)})  {direction}";

                EditorGUILayout.LabelField(line);
            }
        }

        private static string DefaultExperimentDirectory()
        {
            return Path.GetFullPath(Path.Combine(Application.dataPath, "..", "docs", "experiments"));
        }

        private static string ToFullPath(string path)
        {
            if (Path.IsPathRooted(path))
                return path;

            return Path.GetFullPath(Path.Combine(Application.dataPath, "..", path));
        }

        private static string ToProjectRelativePath(string fullPath)
        {
            string projectRoot = Path.GetFullPath(Path.Combine(Application.dataPath, ".."));
            string normalizedFullPath = Path.GetFullPath(fullPath);

            if (!normalizedFullPath.StartsWith(projectRoot, StringComparison.OrdinalIgnoreCase))
                return normalizedFullPath;

            return normalizedFullPath.Substring(projectRoot.Length + 1).Replace('\\', '/');
        }

        private static string FormatValue(double value, string unit)
        {
            return $"{value:0.##} {unit}";
        }

        private static string FormatSampleWindow(SpineBenchmarkSnapshotRecord record)
        {
            return FormatSeconds(record.ActualSampleSeconds) + "/" + FormatSeconds(record.TargetSampleSeconds);
        }

        private static string FormatSeconds(double seconds)
        {
            return seconds > 0d
                ? seconds.ToString("0.##", CultureInfo.InvariantCulture) + " s"
                : "Unknown";
        }

        private static string FormatSigned(double value, string unit)
        {
            string sign = value > 0d ? "+" : string.Empty;
            return $"{sign}{value:0.##} {unit}";
        }

        private static string FormatSignedPercent(double value)
        {
            string sign = value > 0d ? "+" : string.Empty;
            return $"{sign}{value:0.##}%";
        }
    }
}
