namespace OptimizedSpine.EditorTools.Analysis
{
    public sealed class SpineAnalysisFinding
    {
        public SpineAnalysisFinding(SpineAnalysisSeverity severity, SpineAnalysisFindingKey key, params object[] formatArguments)
        {
            Severity = severity;
            Key = key;
            FormatArguments = formatArguments ?? new object[0];
        }

        public SpineAnalysisSeverity Severity { get; }
        public SpineAnalysisFindingKey Key { get; }
        public object[] FormatArguments { get; }

        public string Title => SpineAnalyzerText.FormatFinding(this, SpineAnalyzerText.DefaultLanguage).Title;
        public string Details => SpineAnalyzerText.FormatFinding(this, SpineAnalyzerText.DefaultLanguage).Details;
        public string Suggestion => SpineAnalyzerText.FormatFinding(this, SpineAnalyzerText.DefaultLanguage).Suggestion;
    }
}
