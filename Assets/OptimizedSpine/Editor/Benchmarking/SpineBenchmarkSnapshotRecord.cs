namespace OptimizedSpine.EditorTools.Benchmarking
{
    public sealed class SpineBenchmarkSnapshotRecord
    {
        public string SourcePath { get; set; } = string.Empty;
        public string ExperimentName { get; set; } = string.Empty;
        public string CapturedAtLocal { get; set; } = string.Empty;
        public string UnityVersion { get; set; } = string.Empty;
        public string SpineUnityVersion { get; set; } = string.Empty;
        public string ScenePath { get; set; } = string.Empty;
        public string SkeletonAssetPath { get; set; } = string.Empty;
        public string AnimationName { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public int InstanceCount { get; set; }
        public int SampleCount { get; set; }
        public double AverageFps { get; set; }
        public double AverageFrameMs { get; set; }
        public double MinFrameMs { get; set; }
        public double MaxFrameMs { get; set; }
        public long MonoUsedBytes { get; set; }
        public long TotalAllocatedBytes { get; set; }
    }
}
