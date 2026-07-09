using System;
using System.Collections.Generic;
using System.Globalization;

namespace OptimizedSpine.EditorTools.Benchmarking
{
    public static class SpineBenchmarkSnapshotParser
    {
        public static SpineBenchmarkSnapshotRecord Parse(string markdown, string sourcePath = "")
        {
            if (markdown == null)
                throw new ArgumentNullException(nameof(markdown));

            Dictionary<string, string> fields = new Dictionary<string, string>();
            SpineBenchmarkSnapshotRecord record = new SpineBenchmarkSnapshotRecord
            {
                SourcePath = sourcePath
            };

            string[] lines = markdown.Replace("\r\n", "\n").Split('\n');
            foreach (string rawLine in lines)
            {
                string line = rawLine.Trim();
                if (line.StartsWith("# ", StringComparison.Ordinal) && string.IsNullOrEmpty(record.ExperimentName))
                    record.ExperimentName = line.Substring(2).Trim();

                if (!TryParseTableRow(line, out string label, out string value))
                    continue;

                fields[label] = value;
            }

            record.CapturedAtLocal = Get(fields, "Captured At");
            record.UnityVersion = Get(fields, "Unity");
            record.SpineUnityVersion = Get(fields, "spine-unity");
            record.ScenePath = Get(fields, "Scene");
            record.SkeletonAssetPath = Get(fields, "Skeleton Asset");
            record.AnimationName = Get(fields, "Animation");
            record.Status = Get(fields, "Status");
            record.InstanceCount = ParseInt(Get(fields, "Instance Count"));
            record.SampleCount = ParseInt(Get(fields, "Sample Count"));
            record.AverageFps = ParseDouble(Get(fields, "Average FPS"));
            record.AverageFrameMs = ParseDouble(Get(fields, "Average Frame Time"));
            record.MinFrameMs = ParseDouble(Get(fields, "Min Frame Time"));
            record.MaxFrameMs = ParseDouble(Get(fields, "Max Frame Time"));
            record.MonoUsedBytes = ParseBytes(Get(fields, "Mono Used"));
            record.TotalAllocatedBytes = ParseBytes(Get(fields, "Total Allocated"));

            return record;
        }

        private static bool TryParseTableRow(string line, out string label, out string value)
        {
            label = string.Empty;
            value = string.Empty;

            if (!line.StartsWith("|", StringComparison.Ordinal) || !line.EndsWith("|", StringComparison.Ordinal))
                return false;

            string[] parts = line.Trim('|').Split('|');
            if (parts.Length < 2)
                return false;

            label = parts[0].Trim();
            value = CleanValue(parts[1]);

            if (label == "Field" || label == "---")
                return false;

            return !string.IsNullOrEmpty(label);
        }

        private static string CleanValue(string value)
        {
            return value.Trim().Trim('`').Trim();
        }

        private static string Get(Dictionary<string, string> fields, string key)
        {
            return fields.TryGetValue(key, out string value) ? value : string.Empty;
        }

        private static int ParseInt(string value)
        {
            return int.TryParse(value, NumberStyles.Integer, CultureInfo.InvariantCulture, out int parsed)
                ? parsed
                : 0;
        }

        private static double ParseDouble(string value)
        {
            string number = FirstToken(value);
            return double.TryParse(number, NumberStyles.Float, CultureInfo.InvariantCulture, out double parsed)
                ? parsed
                : 0d;
        }

        private static long ParseBytes(string value)
        {
            double amount = ParseDouble(value);
            string unit = UnitToken(value);

            switch (unit)
            {
                case "MB":
                    return (long)Math.Round(amount * 1024d * 1024d);
                case "KB":
                    return (long)Math.Round(amount * 1024d);
                default:
                    return (long)Math.Round(amount);
            }
        }

        private static string FirstToken(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
                return string.Empty;

            string[] parts = value.Trim().Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
            return parts.Length > 0 ? parts[0] : string.Empty;
        }

        private static string UnitToken(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
                return string.Empty;

            string[] parts = value.Trim().Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
            return parts.Length > 1 ? parts[1] : string.Empty;
        }
    }
}
