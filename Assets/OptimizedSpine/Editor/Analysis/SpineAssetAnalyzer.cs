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
                return Critical("None", "Unsupported", "No target selected", "Select a SkeletonDataAsset or a GameObject with SkeletonAnimation.");

            AnalysisTarget resolved = ResolveTarget(target);
            if (resolved.TargetKind == "Unsupported")
                return Critical(target.name, resolved.TargetKind, "Unsupported target", "Select a SkeletonDataAsset, SkeletonAnimation, SkeletonRenderer, or compatible GameObject.");

            if (resolved.SkeletonDataAsset == null)
                return Critical(target.name, resolved.TargetKind, "Missing SkeletonDataAsset", "Assign a SkeletonDataAsset before analyzing this target.");

            SkeletonData skeletonData = resolved.SkeletonDataAsset.GetSkeletonData(false);
            if (skeletonData == null)
                return Critical(target.name, resolved.TargetKind, "SkeletonData is not loaded", "Reimport the Spine asset or check import errors in the Console.");

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
                    "Multiple materials or atlas assets",
                    "This asset may require extra renderer submeshes or draw calls depending on draw order and blend mode usage.",
                    "Measure it in the benchmark scene before treating it as batch-friendly.");
            }
            else
            {
                report.AddFinding(
                    SpineAnalysisSeverity.Info,
                    "Single material path",
                    "The asset currently resolves to one material in static analysis.",
                    "This is usually easier to batch, but still verify with the benchmark scene.");
            }

            if (report.SlotCount >= HighSlotCount || report.AttachmentCount >= HighAttachmentCount)
            {
                report.AddFinding(
                    SpineAnalysisSeverity.Info,
                    "Complex skeleton",
                    $"Slots: {report.SlotCount}, attachments: {report.AttachmentCount}.",
                    "Use this asset in a 10/30/100 instance benchmark before making runtime optimization decisions.");
            }

            report.AddFinding(
                SpineAnalysisSeverity.Info,
                "Use Single Submesh",
                "Can reduce submeshes for compatible assets, but can be wrong for assets relying on separated materials or special render behavior.",
                "Treat it as an experiment toggle, not a default fix.");

            report.AddFinding(
                SpineAnalysisSeverity.Info,
                "Immutable Triangles",
                "Useful only when triangle topology stays stable for the active skin and animation set.",
                "Enable only after checking that attachments and clipping do not change topology unexpectedly.");

            report.AddFinding(
                SpineAnalysisSeverity.Info,
                "Update When Invisible",
                $"Current value: {report.UpdateWhenInvisible}.",
                "Disable offscreen updates unless gameplay needs invisible characters to keep animating.");
        }

        private static SpineAnalysisReport Critical(string targetName, string targetKind, string title, string suggestion)
        {
            SpineAnalysisReport report = new SpineAnalysisReport
            {
                TargetName = targetName,
                TargetKind = targetKind,
                Analyzed = false
            };

            report.AddFinding(SpineAnalysisSeverity.Critical, title, "The analyzer could not resolve Spine skeleton data from this selection.", suggestion);
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
