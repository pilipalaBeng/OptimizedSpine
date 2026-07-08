using System;
using System.Globalization;

namespace OptimizedSpine.EditorTools.Analysis
{
    public enum SpineAnalyzerLanguage
    {
        English = 0,
        Chinese = 1
    }

    public enum SpineAnalyzerTextKey
    {
        WindowTitle,
        Language,
        Target,
        AnalyzeSelection,
        EmptySelectionHelp,
        Name,
        Type,
        Analyzed,
        Yes,
        No,
        StaticMetrics,
        AtlasAssets,
        Materials,
        Slots,
        Skins,
        Animations,
        Attachments,
        RendererSettings,
        UseSingleSubmesh,
        ImmutableTriangles,
        UpdateWhenInvisible,
        On,
        Off,
        Findings,
        Suggestion
    }

    public enum SpineAnalysisFindingKey
    {
        NoTargetSelected,
        UnsupportedTarget,
        MissingSkeletonDataAsset,
        SkeletonDataNotLoaded,
        MultipleMaterialsOrAtlasAssets,
        SingleMaterialPath,
        ComplexSkeleton,
        UseSingleSubmesh,
        ImmutableTriangles,
        UpdateWhenInvisible
    }

    public readonly struct SpineAnalyzerFindingText
    {
        public SpineAnalyzerFindingText(string title, string details, string suggestion)
        {
            Title = title;
            Details = details;
            Suggestion = suggestion;
        }

        public string Title { get; }
        public string Details { get; }
        public string Suggestion { get; }
    }

    public static class SpineAnalyzerText
    {
        public const SpineAnalyzerLanguage DefaultLanguage = SpineAnalyzerLanguage.English;

        public static readonly string[] LanguageOptionNames =
        {
            "English",
            "中文"
        };

        private static readonly TextEntry[] UiTextEntries =
        {
            new TextEntry(SpineAnalyzerTextKey.WindowTitle, "Spine Analyzer", "Spine 分析器"),
            new TextEntry(SpineAnalyzerTextKey.Language, "Language", "语言"),
            new TextEntry(SpineAnalyzerTextKey.Target, "Target", "目标"),
            new TextEntry(SpineAnalyzerTextKey.AnalyzeSelection, "Analyze Selection", "分析当前选择"),
            new TextEntry(SpineAnalyzerTextKey.EmptySelectionHelp, "Select a Spine asset or object to analyze.", "请选择一个 Spine 资源或对象进行分析。"),
            new TextEntry(SpineAnalyzerTextKey.Name, "Name", "名称"),
            new TextEntry(SpineAnalyzerTextKey.Type, "Type", "类型"),
            new TextEntry(SpineAnalyzerTextKey.Analyzed, "Analyzed", "已分析"),
            new TextEntry(SpineAnalyzerTextKey.Yes, "Yes", "是"),
            new TextEntry(SpineAnalyzerTextKey.No, "No", "否"),
            new TextEntry(SpineAnalyzerTextKey.StaticMetrics, "Static Metrics", "静态指标"),
            new TextEntry(SpineAnalyzerTextKey.AtlasAssets, "Atlas Assets", "图集资源"),
            new TextEntry(SpineAnalyzerTextKey.Materials, "Materials", "材质"),
            new TextEntry(SpineAnalyzerTextKey.Slots, "Slots", "插槽"),
            new TextEntry(SpineAnalyzerTextKey.Skins, "Skins", "皮肤"),
            new TextEntry(SpineAnalyzerTextKey.Animations, "Animations", "动画"),
            new TextEntry(SpineAnalyzerTextKey.Attachments, "Attachments", "附件"),
            new TextEntry(SpineAnalyzerTextKey.RendererSettings, "Renderer Settings", "渲染设置"),
            new TextEntry(SpineAnalyzerTextKey.UseSingleSubmesh, "Use Single Submesh", "单一子网格"),
            new TextEntry(SpineAnalyzerTextKey.ImmutableTriangles, "Immutable Triangles", "固定三角形"),
            new TextEntry(SpineAnalyzerTextKey.UpdateWhenInvisible, "Update When Invisible", "不可见时更新"),
            new TextEntry(SpineAnalyzerTextKey.On, "On", "开"),
            new TextEntry(SpineAnalyzerTextKey.Off, "Off", "关"),
            new TextEntry(SpineAnalyzerTextKey.Findings, "Findings", "分析结果"),
            new TextEntry(SpineAnalyzerTextKey.Suggestion, "Suggestion", "建议")
        };

        private static readonly FindingTextEntry[] FindingTextEntries =
        {
            new FindingTextEntry(
                SpineAnalysisFindingKey.NoTargetSelected,
                "No target selected",
                "The analyzer could not resolve Spine skeleton data from this selection.",
                "Select a SkeletonDataAsset or a GameObject with SkeletonAnimation.",
                "未选择目标",
                "Analyzer 无法从当前选择解析 Spine 骨骼数据。",
                "请选择 SkeletonDataAsset 或带 SkeletonAnimation 的 GameObject。"),

            new FindingTextEntry(
                SpineAnalysisFindingKey.UnsupportedTarget,
                "Unsupported target",
                "The analyzer could not resolve Spine skeleton data from this selection.",
                "Select a SkeletonDataAsset, SkeletonAnimation, SkeletonRenderer, or compatible GameObject.",
                "不支持的目标",
                "Analyzer 无法从当前选择解析 Spine 骨骼数据。",
                "请选择 SkeletonDataAsset、SkeletonAnimation、SkeletonRenderer，或包含这些组件的 GameObject。"),

            new FindingTextEntry(
                SpineAnalysisFindingKey.MissingSkeletonDataAsset,
                "Missing SkeletonDataAsset",
                "The analyzer could not resolve Spine skeleton data from this selection.",
                "Assign a SkeletonDataAsset before analyzing this target.",
                "缺少骨骼数据资源",
                "Analyzer 无法从当前选择解析 Spine 骨骼数据。",
                "先给目标绑定 SkeletonDataAsset，再进行分析。"),

            new FindingTextEntry(
                SpineAnalysisFindingKey.SkeletonDataNotLoaded,
                "SkeletonData is not loaded",
                "The analyzer could not resolve Spine skeleton data from this selection.",
                "Reimport the Spine asset or check import errors in the Console.",
                "骨骼数据未加载",
                "Analyzer 无法从当前选择解析 Spine 骨骼数据。",
                "重新导入 Spine 资源，或检查 Console 里的导入错误。"),

            new FindingTextEntry(
                SpineAnalysisFindingKey.MultipleMaterialsOrAtlasAssets,
                "Multiple materials / atlas assets",
                "This asset may require extra renderer submeshes or draw calls depending on draw order and blend mode usage.",
                "Measure it in the benchmark scene before treating it as batch-friendly.",
                "多材质或多图集",
                "这个资源可能因为 draw order（绘制顺序）或 blend mode（混合模式）产生额外 submesh / draw call。",
                "先放进 benchmark 场景测量，再判断它是否适合 batching（批处理）。"),

            new FindingTextEntry(
                SpineAnalysisFindingKey.SingleMaterialPath,
                "Single material path",
                "The asset currently resolves to one material in static analysis.",
                "This is usually easier to batch, but still verify with the benchmark scene.",
                "单材质路径",
                "静态分析下这个资源目前只解析到一个 material（材质）。",
                "这通常更容易 batching（批处理），但仍要用 benchmark 场景验证。"),

            new FindingTextEntry(
                SpineAnalysisFindingKey.ComplexSkeleton,
                "Complex skeleton",
                "Slots: {0}, attachments: {1}.",
                "Use this asset in a 10 / 30 / 100 instance benchmark before making runtime optimization decisions.",
                "复杂骨骼",
                "插槽: {0}, 附件: {1}。",
                "先用 10 / 30 / 100 实例 benchmark 测量，再决定 runtime optimization（运行时优化）策略。"),

            new FindingTextEntry(
                SpineAnalysisFindingKey.UseSingleSubmesh,
                "Use Single Submesh",
                "Can reduce submeshes for compatible assets, but can be wrong for assets relying on separated materials or special render behavior.",
                "Treat it as an experiment toggle, not a default fix.",
                "单一子网格",
                "对应 Use Single Submesh。对兼容资源可能减少 submesh（子网格），但依赖分离材质或特殊渲染行为的资源可能不适合。",
                "把它当作实验开关，不要当成默认优化。"),

            new FindingTextEntry(
                SpineAnalysisFindingKey.ImmutableTriangles,
                "Immutable Triangles",
                "Useful only when triangle topology stays stable for the active skin and animation set.",
                "Enable only after checking that attachments and clipping do not change topology unexpectedly.",
                "固定三角形",
                "对应 Immutable Triangles。只有 active skin（当前皮肤）和 animation set（动画集合）的 triangle topology（三角形拓扑）稳定时才有意义。",
                "确认 attachments（附件）和 clipping（裁剪）不会意外改变拓扑后再开启。"),

            new FindingTextEntry(
                SpineAnalysisFindingKey.UpdateWhenInvisible,
                "Update When Invisible",
                "Current value: {0}.",
                "Disable offscreen updates unless gameplay needs invisible characters to keep animating.",
                "不可见时更新",
                "对应 Update When Invisible。当前值: {0}。",
                "除非 gameplay（玩法逻辑）要求离屏角色继续动画，否则可以考虑降低或关闭 offscreen update（离屏更新）。")
        };

        public static SpineAnalyzerLanguage NormalizeLanguage(int value)
        {
            return Enum.IsDefined(typeof(SpineAnalyzerLanguage), value)
                ? (SpineAnalyzerLanguage)value
                : DefaultLanguage;
        }

        public static string Get(SpineAnalyzerTextKey key, SpineAnalyzerLanguage language)
        {
            foreach (TextEntry entry in UiTextEntries)
            {
                if (entry.Key == key)
                    return entry.Get(language);
            }

            return key.ToString();
        }

        public static SpineAnalyzerFindingText FormatFinding(SpineAnalysisFinding finding, SpineAnalyzerLanguage language)
        {
            foreach (FindingTextEntry entry in FindingTextEntries)
            {
                if (entry.Key == finding.Key)
                    return entry.Format(language, finding.FormatArguments);
            }

            string fallback = finding.Key.ToString();
            return new SpineAnalyzerFindingText(fallback, fallback, fallback);
        }

        private readonly struct TextEntry
        {
            public TextEntry(SpineAnalyzerTextKey key, string english, string chinese)
            {
                Key = key;
                English = english;
                Chinese = chinese;
            }

            public SpineAnalyzerTextKey Key { get; }
            private string English { get; }
            private string Chinese { get; }

            public string Get(SpineAnalyzerLanguage language)
            {
                return language == SpineAnalyzerLanguage.Chinese ? Chinese : English;
            }
        }

        private readonly struct FindingTextEntry
        {
            public FindingTextEntry(
                SpineAnalysisFindingKey key,
                string englishTitle,
                string englishDetails,
                string englishSuggestion,
                string chineseTitle,
                string chineseDetails,
                string chineseSuggestion)
            {
                Key = key;
                EnglishTitle = englishTitle;
                EnglishDetails = englishDetails;
                EnglishSuggestion = englishSuggestion;
                ChineseTitle = chineseTitle;
                ChineseDetails = chineseDetails;
                ChineseSuggestion = chineseSuggestion;
            }

            public SpineAnalysisFindingKey Key { get; }
            private string EnglishTitle { get; }
            private string EnglishDetails { get; }
            private string EnglishSuggestion { get; }
            private string ChineseTitle { get; }
            private string ChineseDetails { get; }
            private string ChineseSuggestion { get; }

            public SpineAnalyzerFindingText Format(SpineAnalyzerLanguage language, object[] arguments)
            {
                if (language == SpineAnalyzerLanguage.Chinese)
                    return new SpineAnalyzerFindingText(
                        Format(ChineseTitle, arguments),
                        Format(ChineseDetails, arguments),
                        Format(ChineseSuggestion, arguments));

                return new SpineAnalyzerFindingText(
                    Format(EnglishTitle, arguments),
                    Format(EnglishDetails, arguments),
                    Format(EnglishSuggestion, arguments));
            }

            private static string Format(string template, object[] arguments)
            {
                return arguments == null || arguments.Length == 0
                    ? template
                    : string.Format(CultureInfo.InvariantCulture, template, arguments);
            }
        }
    }
}
