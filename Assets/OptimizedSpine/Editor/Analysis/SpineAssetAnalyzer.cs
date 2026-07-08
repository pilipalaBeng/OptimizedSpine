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
                return Critical("None", "Unsupported", "No target selected（未选择目标）", "请选择 SkeletonDataAsset 或带 SkeletonAnimation 的 GameObject。");

            AnalysisTarget resolved = ResolveTarget(target);
            if (resolved.TargetKind == "Unsupported")
                return Critical(target.name, resolved.TargetKind, "Unsupported target（不支持的目标）", "请选择 SkeletonDataAsset、SkeletonAnimation、SkeletonRenderer，或包含这些组件的 GameObject。");

            if (resolved.SkeletonDataAsset == null)
                return Critical(target.name, resolved.TargetKind, "Missing SkeletonDataAsset（缺少骨骼数据资源）", "先给目标绑定 SkeletonDataAsset，再进行分析。");

            SkeletonData skeletonData = resolved.SkeletonDataAsset.GetSkeletonData(false);
            if (skeletonData == null)
                return Critical(target.name, resolved.TargetKind, "SkeletonData is not loaded（骨骼数据未加载）", "重新导入 Spine 资源，或检查 Console 里的导入错误。");

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
                    "Multiple materials / atlas assets（多材质或多图集）",
                    "这个资源可能因为 draw order（绘制顺序）或 blend mode（混合模式）产生额外 submesh / draw call。",
                    "先放进 benchmark 场景测量，再判断它是否适合批处理。");
            }
            else
            {
                report.AddFinding(
                    SpineAnalysisSeverity.Info,
                    "Single material path（单材质路径）",
                    "静态分析下这个资源目前只解析到一个 material（材质）。",
                    "这通常更容易 batching（批处理），但仍要用 benchmark 场景验证。");
            }

            if (report.SlotCount >= HighSlotCount || report.AttachmentCount >= HighAttachmentCount)
            {
                report.AddFinding(
                    SpineAnalysisSeverity.Info,
                    "Complex skeleton（复杂骨骼）",
                    $"Slots（插槽）: {report.SlotCount}, attachments（附件）: {report.AttachmentCount}.",
                    "先用 10 / 30 / 100 实例 benchmark 测量，再决定 runtime optimization（运行时优化）策略。");
            }

            report.AddFinding(
                SpineAnalysisSeverity.Info,
                "Use Single Submesh（单一子网格）",
                "对兼容资源可能减少 submesh（子网格），但依赖分离材质或特殊渲染行为的资源可能不适合。",
                "把它当作实验开关，不要当成默认优化。");

            report.AddFinding(
                SpineAnalysisSeverity.Info,
                "Immutable Triangles（固定三角形）",
                "只有 active skin（当前皮肤）和 animation set（动画集合）的 triangle topology（三角形拓扑）稳定时才有意义。",
                "确认 attachments（附件）和 clipping（裁剪）不会意外改变拓扑后再开启。");

            report.AddFinding(
                SpineAnalysisSeverity.Info,
                "Update When Invisible（不可见时更新）",
                $"Current value（当前值）: {report.UpdateWhenInvisible}.",
                "除非 gameplay（玩法逻辑）要求离屏角色继续动画，否则可以考虑降低或关闭 offscreen update（离屏更新）。");
        }

        private static SpineAnalysisReport Critical(string targetName, string targetKind, string title, string suggestion)
        {
            SpineAnalysisReport report = new SpineAnalysisReport
            {
                TargetName = targetName,
                TargetKind = targetKind,
                Analyzed = false
            };

            report.AddFinding(SpineAnalysisSeverity.Critical, title, "Analyzer 无法从当前选择解析 Spine skeleton data（骨骼数据）。", suggestion);
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
