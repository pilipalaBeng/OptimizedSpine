using OptimizedSpine.EditorTools.Analysis;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace OptimizedSpine.EditorTools
{
    public sealed class SpineAnalyzerWindow : EditorWindow
    {
        private Object target;
        private SpineAnalysisReport report;
        private Vector2 scroll;

        [MenuItem("OptimizedSpine/Spine Analyzer")]
        public static void Open()
        {
            GetWindow<SpineAnalyzerWindow>("Spine Analyzer");
        }

        private void OnEnable()
        {
            target = Selection.activeObject;
            AnalyzeSelection();
        }

        private void OnSelectionChange()
        {
            target = Selection.activeObject;
            AnalyzeSelection();
            Repaint();
        }

        private void OnGUI()
        {
            EditorGUILayout.LabelField("Spine Analyzer（Spine 分析器）", EditorStyles.boldLabel);
            EditorGUILayout.Space();

            EditorGUI.BeginChangeCheck();
            target = EditorGUILayout.ObjectField("Target（目标）", target, typeof(Object), true);
            if (EditorGUI.EndChangeCheck())
                AnalyzeSelection();

            if (GUILayout.Button("Analyze Selection"))
                AnalyzeSelection();

            EditorGUILayout.Space();

            if (report == null)
            {
                EditorGUILayout.HelpBox("Select a Spine asset or object to analyze.（请选择一个 Spine 资源或对象进行分析。）", MessageType.Info);
                return;
            }

            scroll = EditorGUILayout.BeginScrollView(scroll);
            DrawSummary(report);
            DrawFindings(report);
            EditorGUILayout.EndScrollView();
        }

        private void AnalyzeSelection()
        {
            report = SpineAssetAnalyzer.Analyze(target);
        }

        private static void DrawSummary(SpineAnalysisReport report)
        {
            EditorGUILayout.LabelField("Target（目标）", EditorStyles.boldLabel);
            EditorGUILayout.LabelField("Name（名称）", report.TargetName);
            EditorGUILayout.LabelField("Type（类型）", report.TargetKind);
            EditorGUILayout.LabelField("Analyzed（已分析）", report.Analyzed ? "Yes（是）" : "No（否）");

            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Static Metrics（静态指标）", EditorStyles.boldLabel);
            EditorGUILayout.LabelField("Atlas Assets（图集资源）", report.AtlasAssetCount.ToString());
            EditorGUILayout.LabelField("Materials（材质）", report.MaterialCount.ToString());
            EditorGUILayout.LabelField("Slots（插槽）", report.SlotCount.ToString());
            EditorGUILayout.LabelField("Skins（皮肤）", report.SkinCount.ToString());
            EditorGUILayout.LabelField("Animations（动画）", report.AnimationCount.ToString());
            EditorGUILayout.LabelField("Attachments（附件）", report.AttachmentCount.ToString());

            if (!report.HasRendererSettings)
                return;

            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Renderer Settings（渲染设置）", EditorStyles.boldLabel);
            EditorGUILayout.LabelField("Use Single Submesh（单一子网格）", report.SingleSubmesh ? "On（开）" : "Off（关）");
            EditorGUILayout.LabelField("Immutable Triangles（固定三角形）", report.ImmutableTriangles ? "On（开）" : "Off（关）");
            EditorGUILayout.LabelField("Update When Invisible（不可见时更新）", report.UpdateWhenInvisible);
        }

        private static void DrawFindings(SpineAnalysisReport report)
        {
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Findings（分析结果）", EditorStyles.boldLabel);

            foreach (SpineAnalysisFinding finding in report.Findings)
            {
                MessageType messageType = ToMessageType(finding.Severity);
                EditorGUILayout.HelpBox($"{finding.Title}\n\n{finding.Details}\n\nSuggestion（建议）: {finding.Suggestion}", messageType);
            }
        }

        private static MessageType ToMessageType(SpineAnalysisSeverity severity)
        {
            switch (severity)
            {
                case SpineAnalysisSeverity.Critical:
                    return MessageType.Error;
                case SpineAnalysisSeverity.Warning:
                    return MessageType.Warning;
                default:
                    return MessageType.Info;
            }
        }
    }
}
