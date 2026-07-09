using System;
using System.Collections.Generic;

namespace OptimizedSpine.EditorTools.Benchmarking
{
    public sealed class SpineBenchmarkSnapshotComparison
    {
        private readonly List<string> contextWarnings = new List<string>();
        private readonly List<SpineBenchmarkMetricComparison> metrics = new List<SpineBenchmarkMetricComparison>();

        public SpineBenchmarkSnapshotComparison(
            SpineBenchmarkSnapshotRecord baseline,
            SpineBenchmarkSnapshotRecord candidate)
        {
            Baseline = baseline;
            Candidate = candidate;
        }

        public SpineBenchmarkSnapshotRecord Baseline { get; }
        public SpineBenchmarkSnapshotRecord Candidate { get; }
        public IReadOnlyList<string> ContextWarnings => contextWarnings;
        public IReadOnlyList<SpineBenchmarkMetricComparison> Metrics => metrics;

        public SpineBenchmarkMetricComparison GetMetric(string metricName)
        {
            foreach (SpineBenchmarkMetricComparison metric in metrics)
            {
                if (metric.Name == metricName)
                    return metric;
            }

            throw new InvalidOperationException($"Metric '{metricName}' was not found.");
        }

        internal void AddContextWarning(string warning)
        {
            contextWarnings.Add(warning);
        }

        internal void AddMetric(SpineBenchmarkMetricComparison metric)
        {
            metrics.Add(metric);
        }
    }

    public readonly struct SpineBenchmarkMetricComparison
    {
        public SpineBenchmarkMetricComparison(
            string name,
            string unit,
            double baselineValue,
            double candidateValue,
            bool lowerIsBetter)
        {
            Name = name;
            Unit = unit;
            BaselineValue = baselineValue;
            CandidateValue = candidateValue;
            Delta = candidateValue - baselineValue;
            PercentChange = baselineValue != 0d ? Delta / baselineValue * 100d : 0d;
            LowerIsBetter = lowerIsBetter;
        }

        public string Name { get; }
        public string Unit { get; }
        public double BaselineValue { get; }
        public double CandidateValue { get; }
        public double Delta { get; }
        public double PercentChange { get; }
        public bool LowerIsBetter { get; }
        public bool IsImprovement => LowerIsBetter ? Delta < 0d : Delta > 0d;
    }
}
