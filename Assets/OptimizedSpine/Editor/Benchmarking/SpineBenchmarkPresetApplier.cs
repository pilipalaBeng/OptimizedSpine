using System;
using OptimizedSpine.Benchmark;
using UnityEditor;
using UnityEngine;

namespace OptimizedSpine.EditorTools.Benchmarking
{
    public static class SpineBenchmarkPresetApplier
    {
        public static void Apply(
            SpineBenchmarkSpawner spawner,
            SpineBenchmarkSnapshotRecorder recorder,
            SpineBenchmarkPreset preset,
            bool rebuildInstances)
        {
            Apply(spawner, recorder, preset, SpineBenchmarkUpdateMode.Baseline, rebuildInstances);
        }

        public static void Apply(
            SpineBenchmarkSpawner spawner,
            SpineBenchmarkSnapshotRecorder recorder,
            SpineBenchmarkPreset preset,
            SpineBenchmarkUpdateMode updateMode,
            bool rebuildInstances)
        {
            if (spawner == null)
                throw new ArgumentNullException(nameof(spawner));

            Undo.RecordObject(spawner, "Apply Spine Benchmark Preset");
            SerializedObject spawnerObject = new SerializedObject(spawner);
            spawnerObject.FindProperty("instanceCount").intValue = preset.InstanceCount;
            spawnerObject.FindProperty("columns").intValue = preset.Columns;
            spawnerObject.FindProperty("updateMode").enumValueIndex = (int)updateMode;
            spawnerObject.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(spawner);

            if (recorder != null)
            {
                Undo.RecordObject(recorder, "Apply Spine Benchmark Preset");
                SerializedObject recorderObject = new SerializedObject(recorder);
                recorderObject.FindProperty("spawner").objectReferenceValue = spawner;
                recorderObject.FindProperty("experimentName").stringValue = FormatExperimentName(preset, updateMode);
                recorderObject.ApplyModifiedPropertiesWithoutUndo();
                recorder.ResetSampling();
                EditorUtility.SetDirty(recorder);
            }

            if (rebuildInstances)
                spawner.Rebuild();
        }

        public static string FormatExperimentName(SpineBenchmarkPreset preset, SpineBenchmarkUpdateMode updateMode)
        {
            return updateMode == SpineBenchmarkUpdateMode.Baseline
                ? preset.ExperimentName
                : updateMode + "_" + preset.InstanceCount;
        }
    }
}
