namespace OptimizedSpine.EditorTools.Analysis
{
    public sealed class SpineAnalysisFinding
    {
        public SpineAnalysisFinding(SpineAnalysisSeverity severity, string title, string details, string suggestion)
        {
            Severity = severity;
            Title = title;
            Details = details;
            Suggestion = suggestion;
        }

        public SpineAnalysisSeverity Severity { get; }
        public string Title { get; }
        public string Details { get; }
        public string Suggestion { get; }
    }
}
