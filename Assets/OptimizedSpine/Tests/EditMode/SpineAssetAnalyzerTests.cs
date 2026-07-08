using NUnit.Framework;
using OptimizedSpine.EditorTools.Analysis;
using Spine.Unity;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace OptimizedSpine.Tests
{
    public sealed class SpineAssetAnalyzerTests
    {
        private const string DefaultSkeletonPath =
            "Assets/Samples/spine-unity Runtime/4.3.95/Spine Examples/Spine Skeletons/spineboy-pro/spineboy-pro_SkeletonData.asset";

        [Test]
        public void Analyze_NullTarget_ReturnsCriticalReport()
        {
            SpineAnalysisReport report = SpineAssetAnalyzer.Analyze(null);

            Assert.That(report.Analyzed, Is.False);
            Assert.That(report.HasCriticalFindings, Is.True);
            Assert.That(report.Findings[0].Severity, Is.EqualTo(SpineAnalysisSeverity.Critical));
        }

        [Test]
        public void Analyze_UnsupportedObject_ReturnsCriticalReport()
        {
            Texture2D texture = new Texture2D(1, 1);

            try
            {
                SpineAnalysisReport report = SpineAssetAnalyzer.Analyze(texture);

                Assert.That(report.Analyzed, Is.False);
                Assert.That(report.TargetKind, Is.EqualTo("Unsupported"));
                Assert.That(report.HasCriticalFindings, Is.True);
            }
            finally
            {
                Object.DestroyImmediate(texture);
            }
        }

        [Test]
        public void Analyze_SkeletonDataAsset_ReturnsMetrics()
        {
            SkeletonDataAsset asset = AssetDatabase.LoadAssetAtPath<SkeletonDataAsset>(DefaultSkeletonPath);
            Assert.That(asset, Is.Not.Null, $"Missing test SkeletonDataAsset at {DefaultSkeletonPath}");

            SpineAnalysisReport report = SpineAssetAnalyzer.Analyze(asset);

            Assert.That(report.Analyzed, Is.True);
            Assert.That(report.TargetKind, Is.EqualTo("SkeletonDataAsset"));
            Assert.That(report.SlotCount, Is.GreaterThan(0));
            Assert.That(report.AnimationCount, Is.GreaterThan(0));
            Assert.That(report.SkinCount, Is.GreaterThan(0));
            Assert.That(report.AttachmentCount, Is.GreaterThan(0));
        }

        [Test]
        public void Analyze_GameObjectWithSkeletonAnimation_UsesComponentSkeletonData()
        {
            EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

            SkeletonDataAsset asset = AssetDatabase.LoadAssetAtPath<SkeletonDataAsset>(DefaultSkeletonPath);
            Assert.That(asset, Is.Not.Null, $"Missing test SkeletonDataAsset at {DefaultSkeletonPath}");

            GameObject owner = new GameObject("AnalyzerSkeleton");
            owner.SetActive(false);

            try
            {
                SkeletonRenderer renderer = owner.AddComponent<SkeletonRenderer>();
                SkeletonAnimation animation = owner.AddComponent<SkeletonAnimation>();
                renderer.SkeletonDataAsset = asset;
                renderer.Animation = animation;

                SpineAnalysisReport report = SpineAssetAnalyzer.Analyze(owner);

                Assert.That(report.Analyzed, Is.True);
                Assert.That(report.TargetKind, Is.EqualTo("GameObject"));
                Assert.That(report.TargetName, Is.EqualTo("AnalyzerSkeleton"));
                Assert.That(report.HasRendererSettings, Is.True);
            }
            finally
            {
                Object.DestroyImmediate(owner);
            }
        }

        [Test]
        public void AddFinding_OrdersCriticalFindingsFirst()
        {
            SpineAnalysisReport report = new SpineAnalysisReport();

            report.AddFinding(SpineAnalysisSeverity.Info, SpineAnalysisFindingKey.SingleMaterialPath);
            report.AddFinding(SpineAnalysisSeverity.Critical, SpineAnalysisFindingKey.NoTargetSelected);
            report.AddFinding(SpineAnalysisSeverity.Warning, SpineAnalysisFindingKey.MultipleMaterialsOrAtlasAssets);

            Assert.That(report.Findings[0].Severity, Is.EqualTo(SpineAnalysisSeverity.Critical));
            Assert.That(report.Findings[1].Severity, Is.EqualTo(SpineAnalysisSeverity.Warning));
            Assert.That(report.Findings[2].Severity, Is.EqualTo(SpineAnalysisSeverity.Info));
        }

        [Test]
        public void Analyze_SkeletonDataAsset_AddsStableOptimizationFindingKeys()
        {
            SkeletonDataAsset asset = AssetDatabase.LoadAssetAtPath<SkeletonDataAsset>(DefaultSkeletonPath);
            Assert.That(asset, Is.Not.Null, $"Missing test SkeletonDataAsset at {DefaultSkeletonPath}");

            SpineAnalysisReport report = SpineAssetAnalyzer.Analyze(asset);

            Assert.That(report.Findings, Has.Some.Matches<SpineAnalysisFinding>(
                finding => finding.Key == SpineAnalysisFindingKey.UseSingleSubmesh));
            Assert.That(report.Findings, Has.Some.Matches<SpineAnalysisFinding>(
                finding => finding.Key == SpineAnalysisFindingKey.ImmutableTriangles));
            Assert.That(report.Findings, Has.Some.Matches<SpineAnalysisFinding>(
                finding => finding.Key == SpineAnalysisFindingKey.UpdateWhenInvisible));
        }

        [Test]
        public void Text_DefaultLanguage_IsEnglishWithEnglishAndChineseOptions()
        {
            Assert.That(SpineAnalyzerText.DefaultLanguage, Is.EqualTo(SpineAnalyzerLanguage.English));
            Assert.That(SpineAnalyzerText.LanguageOptionNames, Is.EqualTo(new[] { "English", "中文" }));
        }

        [Test]
        public void Text_FormatFinding_UsesSelectedLanguage()
        {
            SpineAnalysisFinding finding = new SpineAnalysisFinding(
                SpineAnalysisSeverity.Info,
                SpineAnalysisFindingKey.UseSingleSubmesh);

            SpineAnalyzerFindingText english = SpineAnalyzerText.FormatFinding(finding, SpineAnalyzerLanguage.English);
            SpineAnalyzerFindingText chinese = SpineAnalyzerText.FormatFinding(finding, SpineAnalyzerLanguage.Chinese);

            Assert.That(english.Title, Is.EqualTo("Use Single Submesh"));
            Assert.That(chinese.Title, Is.EqualTo("单一子网格"));
            Assert.That(chinese.Details, Does.Contain("Use Single Submesh"));
        }
    }
}
