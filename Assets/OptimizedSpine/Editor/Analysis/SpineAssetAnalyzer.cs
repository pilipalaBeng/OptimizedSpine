using System.Collections.Generic;
using Spine;
using Spine.Unity;
using UnityEngine;
using Object = UnityEngine.Object;

namespace OptimizedSpine.EditorTools.Analysis
{
    public static class SpineAssetAnalyzer
    {
        private const int HighSlotCount = 80;
        private const int HighAttachmentCount = 160;

        public static SpineAnalysisReport Analyze(Object target)
        {
            if (target == null)
                return Critical("None", "Unsupported", SpineAnalysisFindingKey.NoTargetSelected);

            AnalysisTarget resolved = ResolveTarget(target);
            if (resolved.TargetKind == "Unsupported")
                return Critical(target.name, resolved.TargetKind, SpineAnalysisFindingKey.UnsupportedTarget);

            if (resolved.SkeletonDataAsset == null)
                return Critical(target.name, resolved.TargetKind, SpineAnalysisFindingKey.MissingSkeletonDataAsset);

            SkeletonData skeletonData = resolved.SkeletonDataAsset.GetSkeletonData(false);
            if (skeletonData == null)
                return Critical(target.name, resolved.TargetKind, SpineAnalysisFindingKey.SkeletonDataNotLoaded);

            SpineAnalysisReport report = new SpineAnalysisReport
            {
                TargetName = target.name,
                TargetKind = resolved.TargetKind,
                Analyzed = true,
                AtlasAssetCount = resolved.SkeletonDataAsset.atlasAssets != null ? resolved.SkeletonDataAsset.atlasAssets.Length : 0,
                MaterialCount = CountMaterials(resolved.SkeletonDataAsset),
                SlotCount = skeletonData.Slots.Count,
                SkinCount = skeletonData.Skins.Count,
                AnimationCount = skeletonData.Animations.Count,
                AttachmentCount = CountAttachments(skeletonData)
            };

            ApplyRendererSettings(report, resolved.Renderer);
            AddFindings(report);
            return report;
        }

        private static AnalysisTarget ResolveTarget(Object target)
        {
            if (target is SkeletonDataAsset skeletonDataAsset)
                return new AnalysisTarget("SkeletonDataAsset", skeletonDataAsset, null);

            if (target is SkeletonAnimation skeletonAnimation)
                return new AnalysisTarget("SkeletonAnimation", skeletonAnimation.SkeletonDataAsset, skeletonAnimation.Renderer as SkeletonRenderer);

            if (target is SkeletonRenderer skeletonRenderer)
                return new AnalysisTarget("SkeletonRenderer", skeletonRenderer.SkeletonDataAsset, skeletonRenderer);

            if (target is GameObject gameObject)
            {
                SkeletonAnimation childAnimation = gameObject.GetComponentInChildren<SkeletonAnimation>(true);
                if (childAnimation != null)
                    return new AnalysisTarget("GameObject", childAnimation.SkeletonDataAsset, childAnimation.Renderer as SkeletonRenderer);

                SkeletonRenderer childRenderer = gameObject.GetComponentInChildren<SkeletonRenderer>(true);
                if (childRenderer != null)
                    return new AnalysisTarget("GameObject", childRenderer.SkeletonDataAsset, childRenderer);

                return new AnalysisTarget("GameObject", null, null);
            }

            return new AnalysisTarget("Unsupported", null, null);
        }

        private static int CountMaterials(SkeletonDataAsset skeletonDataAsset)
        {
            HashSet<Material> materials = new HashSet<Material>();
            if (skeletonDataAsset.atlasAssets == null)
                return 0;

            foreach (AtlasAssetBase atlasAsset in skeletonDataAsset.atlasAssets)
            {
                if (atlasAsset == null)
                    continue;

                IEnumerable<Material> atlasMaterials = atlasAsset.Materials;
                if (atlasMaterials == null)
                    continue;

                foreach (Material material in atlasMaterials)
                {
                    if (material != null)
                        materials.Add(material);
                }
            }

            return materials.Count;
        }

        private static int CountAttachments(SkeletonData skeletonData)
        {
            int count = 0;
            List<Skin.SkinEntry> entries = new List<Skin.SkinEntry>();

            foreach (Skin skin in skeletonData.Skins)
            {
                for (int slotIndex = 0; slotIndex < skeletonData.Slots.Count; slotIndex++)
                {
                    entries.Clear();
                    skin.GetAttachments(slotIndex, entries);
                    count += entries.Count;
                }
            }

            return count;
        }

        private static void ApplyRendererSettings(SpineAnalysisReport report, SkeletonRenderer renderer)
        {
            if (renderer == null)
                return;

            report.HasRendererSettings = true;
            report.SingleSubmesh = renderer.singleSubmesh;
            report.ImmutableTriangles = renderer.MeshSettings.immutableTriangles;
            report.UpdateWhenInvisible = renderer.UpdateWhenInvisible.ToString();
        }

        private static void AddFindings(SpineAnalysisReport report)
        {
            if (report.MaterialCount > 1 || report.AtlasAssetCount > 1)
            {
                report.AddFinding(
                    SpineAnalysisSeverity.Warning,
                    SpineAnalysisFindingKey.MultipleMaterialsOrAtlasAssets);
            }
            else
            {
                report.AddFinding(
                    SpineAnalysisSeverity.Info,
                    SpineAnalysisFindingKey.SingleMaterialPath);
            }

            if (report.SlotCount >= HighSlotCount || report.AttachmentCount >= HighAttachmentCount)
            {
                report.AddFinding(
                    SpineAnalysisSeverity.Info,
                    SpineAnalysisFindingKey.ComplexSkeleton,
                    report.SlotCount,
                    report.AttachmentCount);
            }

            report.AddFinding(
                SpineAnalysisSeverity.Info,
                SpineAnalysisFindingKey.UseSingleSubmesh);

            report.AddFinding(
                SpineAnalysisSeverity.Info,
                SpineAnalysisFindingKey.ImmutableTriangles);

            report.AddFinding(
                SpineAnalysisSeverity.Info,
                SpineAnalysisFindingKey.UpdateWhenInvisible,
                report.UpdateWhenInvisible);
        }

        private static SpineAnalysisReport Critical(string targetName, string targetKind, SpineAnalysisFindingKey key)
        {
            SpineAnalysisReport report = new SpineAnalysisReport
            {
                TargetName = targetName,
                TargetKind = targetKind,
                Analyzed = false
            };

            report.AddFinding(SpineAnalysisSeverity.Critical, key);
            return report;
        }

        private readonly struct AnalysisTarget
        {
            public AnalysisTarget(string targetKind, SkeletonDataAsset skeletonDataAsset, SkeletonRenderer renderer)
            {
                TargetKind = targetKind;
                SkeletonDataAsset = skeletonDataAsset;
                Renderer = renderer;
            }

            public string TargetKind { get; }
            public SkeletonDataAsset SkeletonDataAsset { get; }
            public SkeletonRenderer Renderer { get; }
        }
    }
}
