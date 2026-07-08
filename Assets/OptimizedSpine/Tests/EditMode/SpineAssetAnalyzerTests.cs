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

            report.AddFinding(SpineAnalysisSeverity.Info, "Info", "Info details", "Info suggestion");
            report.AddFinding(SpineAnalysisSeverity.Critical, "Critical", "Critical details", "Critical suggestion");
            report.AddFinding(SpineAnalysisSeverity.Warning, "Warning", "Warning details", "Warning suggestion");

            Assert.That(report.Findings[0].Severity, Is.EqualTo(SpineAnalysisSeverity.Critical));
            Assert.That(report.Findings[1].Severity, Is.EqualTo(SpineAnalysisSeverity.Warning));
            Assert.That(report.Findings[2].Severity, Is.EqualTo(SpineAnalysisSeverity.Info));
        }

        [Test]
        public void Analyze_SkeletonDataAsset_UsesBilingualOptimizationTerms()
        {
            SkeletonDataAsset asset = AssetDatabase.LoadAssetAtPath<SkeletonDataAsset>(DefaultSkeletonPath);
            Assert.That(asset, Is.Not.Null, $"Missing test SkeletonDataAsset at {DefaultSkeletonPath}");

            SpineAnalysisReport report = SpineAssetAnalyzer.Analyze(asset);

            Assert.That(report.Findings, Has.Some.Matches<SpineAnalysisFinding>(
                finding => finding.Title.Contains("Use Single Submesh") && finding.Title.Contains("单一子网格")));
            Assert.That(report.Findings, Has.Some.Matches<SpineAnalysisFinding>(
                finding => finding.Title.Contains("Immutable Triangles") && finding.Title.Contains("固定三角形")));
            Assert.That(report.Findings, Has.Some.Matches<SpineAnalysisFinding>(
                finding => finding.Title.Contains("Update When Invisible") && finding.Title.Contains("不可见时更新")));
        }
    }
}
