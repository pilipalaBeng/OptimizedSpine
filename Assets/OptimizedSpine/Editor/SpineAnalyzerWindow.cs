using OptimizedSpine.EditorTools.Analysis;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace OptimizedSpine.EditorTools
{
    public sealed class SpineAnalyzerWindow : EditorWindow
    {
        private const string LanguagePreferenceKey = "OptimizedSpine.SpineAnalyzer.Language";

        private Object target;
        private SpineAnalysisReport report;
        private Vector2 scroll;
        private SpineAnalyzerLanguage language = SpineAnalyzerText.DefaultLanguage;

        [MenuItem("OptimizedSpine/Spine Analyzer")]
        public static void Open()
        {
            GetWindow<SpineAnalyzerWindow>("Spine Analyzer");
        }

        private void OnEnable()
        {
            language = SpineAnalyzerText.NormalizeLanguage(EditorPrefs.GetInt(LanguagePreferenceKey, (int)SpineAnalyzerText.DefaultLanguage));
            UpdateWindowTitle();
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
            EditorGUILayout.LabelField(T(SpineAnalyzerTextKey.WindowTitle), EditorStyles.boldLabel);
            EditorGUILayout.Space();

            DrawLanguagePopup();

            EditorGUI.BeginChangeCheck();
            target = EditorGUILayout.ObjectField(T(SpineAnalyzerTextKey.Target), target, typeof(Object), true);
            if (EditorGUI.EndChangeCheck())
                AnalyzeSelection();

            if (GUILayout.Button(T(SpineAnalyzerTextKey.AnalyzeSelection)))
                AnalyzeSelection();

            EditorGUILayout.Space();

            if (report == null)
            {
                EditorGUILayout.HelpBox(T(SpineAnalyzerTextKey.EmptySelectionHelp), MessageType.Info);
                return;
            }

            scroll = EditorGUILayout.BeginScrollView(scroll);
            DrawSummary(report, language);
            DrawFindings(report, language);
            EditorGUILayout.EndScrollView();
        }

        private void DrawLanguagePopup()
        {
            EditorGUI.BeginChangeCheck();
            int selectedLanguage = EditorGUILayout.Popup(
                T(SpineAnalyzerTextKey.Language),
                (int)language,
                SpineAnalyzerText.LanguageOptionNames);

            if (!EditorGUI.EndChangeCheck())
                return;

            language = SpineAnalyzerText.NormalizeLanguage(selectedLanguage);
            EditorPrefs.SetInt(LanguagePreferenceKey, (int)language);
            UpdateWindowTitle();
        }

        private void AnalyzeSelection()
        {
            report = SpineAssetAnalyzer.Analyze(target);
        }

        private void UpdateWindowTitle()
        {
            titleContent = new GUIContent(T(SpineAnalyzerTextKey.WindowTitle));
        }

        private string T(SpineAnalyzerTextKey key)
        {
            return SpineAnalyzerText.Get(key, language);
        }

        private static string T(SpineAnalyzerTextKey key, SpineAnalyzerLanguage language)
        {
            return SpineAnalyzerText.Get(key, language);
        }

        private static void DrawSummary(SpineAnalysisReport report, SpineAnalyzerLanguage language)
        {
            EditorGUILayout.LabelField(T(SpineAnalyzerTextKey.Target, language), EditorStyles.boldLabel);
            EditorGUILayout.LabelField(T(SpineAnalyzerTextKey.Name, language), report.TargetName);
            EditorGUILayout.LabelField(T(SpineAnalyzerTextKey.Type, language), report.TargetKind);
            EditorGUILayout.LabelField(
                T(SpineAnalyzerTextKey.Analyzed, language),
                report.Analyzed ? T(SpineAnalyzerTextKey.Yes, language) : T(SpineAnalyzerTextKey.No, language));

            EditorGUILayout.Space();
            EditorGUILayout.LabelField(T(SpineAnalyzerTextKey.StaticMetrics, language), EditorStyles.boldLabel);
            EditorGUILayout.LabelField(T(SpineAnalyzerTextKey.AtlasAssets, language), report.AtlasAssetCount.ToString());
            EditorGUILayout.LabelField(T(SpineAnalyzerTextKey.Materials, language), report.MaterialCount.ToString());
            EditorGUILayout.LabelField(T(SpineAnalyzerTextKey.Slots, language), report.SlotCount.ToString());
            EditorGUILayout.LabelField(T(SpineAnalyzerTextKey.Skins, language), report.SkinCount.ToString());
            EditorGUILayout.LabelField(T(SpineAnalyzerTextKey.Animations, language), report.AnimationCount.ToString());
            EditorGUILayout.LabelField(T(SpineAnalyzerTextKey.Attachments, language), report.AttachmentCount.ToString());

            if (!report.HasRendererSettings)
                return;

            EditorGUILayout.Space();
            EditorGUILayout.LabelField(T(SpineAnalyzerTextKey.RendererSettings, language), EditorStyles.boldLabel);
            EditorGUILayout.LabelField(
                T(SpineAnalyzerTextKey.UseSingleSubmesh, language),
                report.SingleSubmesh ? T(SpineAnalyzerTextKey.On, language) : T(SpineAnalyzerTextKey.Off, language));
            EditorGUILayout.LabelField(
                T(SpineAnalyzerTextKey.ImmutableTriangles, language),
                report.ImmutableTriangles ? T(SpineAnalyzerTextKey.On, language) : T(SpineAnalyzerTextKey.Off, language));
            EditorGUILayout.LabelField(T(SpineAnalyzerTextKey.UpdateWhenInvisible, language), report.UpdateWhenInvisible);
        }

        private static void DrawFindings(SpineAnalysisReport report, SpineAnalyzerLanguage language)
        {
            EditorGUILayout.Space();
            EditorGUILayout.LabelField(T(SpineAnalyzerTextKey.Findings, language), EditorStyles.boldLabel);

            foreach (SpineAnalysisFinding finding in report.Findings)
            {
                MessageType messageType = ToMessageType(finding.Severity);
                SpineAnalyzerFindingText findingText = SpineAnalyzerText.FormatFinding(finding, language);
                EditorGUILayout.HelpBox(
                    $"{findingText.Title}\n\n{findingText.Details}\n\n{T(SpineAnalyzerTextKey.Suggestion, language)}: {findingText.Suggestion}",
                    messageType);
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
