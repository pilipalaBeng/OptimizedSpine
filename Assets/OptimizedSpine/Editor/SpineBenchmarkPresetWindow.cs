using OptimizedSpine.Benchmark;
using OptimizedSpine.EditorTools.Benchmarking;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace OptimizedSpine.EditorTools
{
    public sealed class SpineBenchmarkPresetWindow : EditorWindow
    {
        private SpineBenchmarkSpawner spawner;
        private SpineBenchmarkSnapshotRecorder recorder;
        private bool rebuildInstances = true;
        private SpineBenchmarkUpdateMode updateMode = SpineBenchmarkUpdateMode.Baseline;
        private string lastMessage = string.Empty;

        [MenuItem("OptimizedSpine/Benchmark Presets")]
        public static void Open()
        {
            GetWindow<SpineBenchmarkPresetWindow>("Benchmark Presets");
        }

        private void OnEnable()
        {
            titleContent = new GUIContent("Benchmark Presets");
            RefreshTargets();
        }

        private void OnGUI()
        {
            EditorGUILayout.LabelField("Benchmark Presets / 基准预设", EditorStyles.boldLabel);
            EditorGUILayout.HelpBox("选择固定实例数量，工具会同步 Spawner 的生成数量/每行数量，以及 Snapshot Recorder 的实验名称。", MessageType.Info);

            EditorGUI.BeginChangeCheck();
            spawner = (SpineBenchmarkSpawner)EditorGUILayout.ObjectField("生成器", spawner, typeof(SpineBenchmarkSpawner), true);
            recorder = (SpineBenchmarkSnapshotRecorder)EditorGUILayout.ObjectField("记录器", recorder, typeof(SpineBenchmarkSnapshotRecorder), true);
            if (EditorGUI.EndChangeCheck())
                lastMessage = string.Empty;

            rebuildInstances = EditorGUILayout.ToggleLeft("应用后重建当前实例", rebuildInstances);
            updateMode = (SpineBenchmarkUpdateMode)EditorGUILayout.EnumPopup("Spine 更新模式", updateMode);

            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("查找场景对象"))
                RefreshTargets();
            if (GUILayout.Button("打开 Snapshot 对比"))
                SpineBenchmarkSnapshotCompareWindow.Open();
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.Space();
            EditorGUILayout.LabelField("预设", EditorStyles.boldLabel);

            foreach (SpineBenchmarkPreset preset in SpineBenchmarkPresetCatalog.DefaultPresets)
            {
                string experimentName = SpineBenchmarkPresetApplier.FormatExperimentName(preset, updateMode);
                if (GUILayout.Button($"{experimentName}  ({preset.InstanceCount} instances / {preset.Columns} columns)"))
                    ApplyPreset(preset);
            }

            if (!string.IsNullOrEmpty(lastMessage))
                EditorGUILayout.HelpBox(lastMessage, MessageType.Info);
        }

        private void RefreshTargets()
        {
            spawner = Object.FindObjectOfType<SpineBenchmarkSpawner>();
            recorder = Object.FindObjectOfType<SpineBenchmarkSnapshotRecorder>();
            lastMessage = spawner == null
                ? "当前场景没有找到 SpineBenchmarkSpawner。请先打开或生成 baseline 场景。"
                : string.Empty;
        }

        private void ApplyPreset(SpineBenchmarkPreset preset)
        {
            if (spawner == null)
                RefreshTargets();

            if (spawner == null)
            {
                lastMessage = "没有找到生成器，无法应用 preset。";
                return;
            }

            SpineBenchmarkPresetApplier.Apply(spawner, recorder, preset, updateMode, rebuildInstances);

            if (!Application.isPlaying)
                EditorSceneManager.MarkSceneDirty(spawner.gameObject.scene);

            lastMessage = $"已应用 {SpineBenchmarkPresetApplier.FormatExperimentName(preset, updateMode)}。下一次 Write Benchmark Snapshot 会使用这个实验名称。";
        }
    }
}
