using System.Collections.Generic;
using System.Linq;

namespace OptimizedSpine.EditorTools.Analysis
{
    public sealed class SpineAnalysisReport
    {
        private readonly List<SpineAnalysisFinding> findings = new List<SpineAnalysisFinding>();

        public string TargetName { get; set; } = "None";
        public string TargetKind { get; set; } = "Unsupported";
        public bool Analyzed { get; set; }
        public int AtlasAssetCount { get; set; }
        public int MaterialCount { get; set; }
        public int SlotCount { get; set; }
        public int SkinCount { get; set; }
        public int AnimationCount { get; set; }
        public int AttachmentCount { get; set; }
        public bool HasRendererSettings { get; set; }
        public bool SingleSubmesh { get; set; }
        public bool ImmutableTriangles { get; set; }
        public string UpdateWhenInvisible { get; set; } = "Unavailable";

        public IReadOnlyList<SpineAnalysisFinding> Findings => findings;
        public bool HasCriticalFindings => findings.Any(finding => finding.Severity == SpineAnalysisSeverity.Critical);

        public void AddFinding(SpineAnalysisSeverity severity, string title, string details, string suggestion)
        {
            findings.Add(new SpineAnalysisFinding(severity, title, details, suggestion));
            findings.Sort((left, right) => right.Severity.CompareTo(left.Severity));
        }
    }
}
