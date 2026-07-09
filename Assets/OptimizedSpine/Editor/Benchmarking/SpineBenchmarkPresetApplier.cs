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
            if (spawner == null)
                throw new ArgumentNullException(nameof(spawner));

            Undo.RecordObject(spawner, "Apply Spine Benchmark Preset");
            SerializedObject spawnerObject = new SerializedObject(spawner);
            spawnerObject.FindProperty("instanceCount").intValue = preset.InstanceCount;
            spawnerObject.FindProperty("columns").intValue = preset.Columns;
            spawnerObject.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(spawner);

            if (recorder != null)
            {
                Undo.RecordObject(recorder, "Apply Spine Benchmark Preset");
                SerializedObject recorderObject = new SerializedObject(recorder);
                recorderObject.FindProperty("spawner").objectReferenceValue = spawner;
                recorderObject.FindProperty("experimentName").stringValue = preset.ExperimentName;
                recorderObject.ApplyModifiedPropertiesWithoutUndo();
                recorder.ResetSampling();
                EditorUtility.SetDirty(recorder);
            }

            if (rebuildInstances)
                spawner.Rebuild();
        }
    }
}
