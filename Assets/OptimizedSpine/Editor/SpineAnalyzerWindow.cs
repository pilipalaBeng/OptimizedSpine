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
            EditorGUILayout.LabelField("Spine Analyzer", EditorStyles.boldLabel);
            EditorGUILayout.Space();

            EditorGUI.BeginChangeCheck();
            target = EditorGUILayout.ObjectField("Target", target, typeof(Object), true);
            if (EditorGUI.EndChangeCheck())
                AnalyzeSelection();

            if (GUILayout.Button("Analyze Selection"))
                AnalyzeSelection();

            EditorGUILayout.Space();

            if (report == null)
            {
                EditorGUILayout.HelpBox("Select a Spine asset or object to analyze.", MessageType.Info);
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
            EditorGUILayout.LabelField("Target", EditorStyles.boldLabel);
            EditorGUILayout.LabelField("Name", report.TargetName);
            EditorGUILayout.LabelField("Type", report.TargetKind);
            EditorGUILayout.LabelField("Analyzed", report.Analyzed ? "Yes" : "No");

            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Static Metrics", EditorStyles.boldLabel);
            EditorGUILayout.LabelField("Atlas Assets", report.AtlasAssetCount.ToString());
            EditorGUILayout.LabelField("Materials", report.MaterialCount.ToString());
            EditorGUILayout.LabelField("Slots", report.SlotCount.ToString());
            EditorGUILayout.LabelField("Skins", report.SkinCount.ToString());
            EditorGUILayout.LabelField("Animations", report.AnimationCount.ToString());
            EditorGUILayout.LabelField("Attachments", report.AttachmentCount.ToString());

            if (!report.HasRendererSettings)
                return;

            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Renderer Settings", EditorStyles.boldLabel);
            EditorGUILayout.LabelField("Use Single Submesh", report.SingleSubmesh ? "On" : "Off");
            EditorGUILayout.LabelField("Immutable Triangles", report.ImmutableTriangles ? "On" : "Off");
            EditorGUILayout.LabelField("Update When Invisible", report.UpdateWhenInvisible);
        }

        private static void DrawFindings(SpineAnalysisReport report)
        {
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Findings", EditorStyles.boldLabel);

            foreach (SpineAnalysisFinding finding in report.Findings)
            {
                MessageType messageType = ToMessageType(finding.Severity);
                EditorGUILayout.HelpBox($"{finding.Title}\n\n{finding.Details}\n\nSuggestion: {finding.Suggestion}", messageType);
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
