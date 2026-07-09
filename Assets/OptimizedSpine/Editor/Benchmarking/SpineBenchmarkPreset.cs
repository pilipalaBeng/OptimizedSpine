namespace OptimizedSpine.EditorTools.Benchmarking
{
    public readonly struct SpineBenchmarkPreset
    {
        public SpineBenchmarkPreset(int instanceCount, int columns, string experimentName)
        {
            InstanceCount = instanceCount;
            Columns = columns;
            ExperimentName = experimentName;
        }

        public int InstanceCount { get; }
        public int Columns { get; }
        public string ExperimentName { get; }
    }

    public static class SpineBenchmarkPresetCatalog
    {
        public static readonly SpineBenchmarkPreset[] DefaultPresets =
        {
            new SpineBenchmarkPreset(10, 5, "Baseline_10"),
            new SpineBenchmarkPreset(25, 5, "Baseline_25"),
            new SpineBenchmarkPreset(50, 10, "Baseline_50"),
            new SpineBenchmarkPreset(100, 10, "Baseline_100")
        };
    }
}
