using System.Linq;
using NUnit.Framework;
using OptimizedSpine.Benchmark;
using OptimizedSpine.EditorTools.Benchmarking;
using UnityEditor;
using UnityEngine;

namespace OptimizedSpine.Tests
{
    public sealed class SpineBenchmarkPresetTests
    {
        [Test]
        public void DefaultPresets_ExposeStableInstanceCounts()
        {
            int[] counts = SpineBenchmarkPresetCatalog.DefaultPresets
                .Select(preset => preset.InstanceCount)
                .ToArray();

            Assert.That(counts, Is.EqualTo(new[] { 10, 25, 50, 100 }));
        }

        [Test]
        public void Apply_SetsSpawnerAndSnapshotContext()
        {
            GameObject owner = new GameObject("PresetUnderTest");
            SpineBenchmarkSpawner spawner = owner.AddComponent<SpineBenchmarkSpawner>();
            SpineBenchmarkSnapshotRecorder recorder = owner.AddComponent<SpineBenchmarkSnapshotRecorder>();

            try
            {
                SpineBenchmarkPreset preset = new SpineBenchmarkPreset(
                    instanceCount: 25,
                    columns: 5,
                    experimentName: "Baseline_25");

                SpineBenchmarkPresetApplier.Apply(spawner, recorder, preset, rebuildInstances: false);

                SerializedObject spawnerObject = new SerializedObject(spawner);
                Assert.That(spawnerObject.FindProperty("instanceCount").intValue, Is.EqualTo(25));
                Assert.That(spawnerObject.FindProperty("columns").intValue, Is.EqualTo(5));
                Assert.That(spawnerObject.FindProperty("updateMode").enumValueIndex, Is.EqualTo((int)SpineBenchmarkUpdateMode.Baseline));

                SerializedObject recorderObject = new SerializedObject(recorder);
                Assert.That(recorderObject.FindProperty("spawner").objectReferenceValue, Is.SameAs(spawner));
                Assert.That(recorderObject.FindProperty("experimentName").stringValue, Is.EqualTo("Baseline_25"));
            }
            finally
            {
                Object.DestroyImmediate(owner);
            }
        }

        [Test]
        public void Apply_CentralizedUpdateModeSetsModeAndExperimentName()
        {
            GameObject owner = new GameObject("CentralizedPresetUnderTest");
            SpineBenchmarkSpawner spawner = owner.AddComponent<SpineBenchmarkSpawner>();
            SpineBenchmarkSnapshotRecorder recorder = owner.AddComponent<SpineBenchmarkSnapshotRecorder>();

            try
            {
                SpineBenchmarkPreset preset = new SpineBenchmarkPreset(
                    instanceCount: 25,
                    columns: 5,
                    experimentName: "Baseline_25");

                SpineBenchmarkPresetApplier.Apply(
                    spawner,
                    recorder,
                    preset,
                    SpineBenchmarkUpdateMode.CentralizedUpdate,
                    rebuildInstances: false);

                SerializedObject spawnerObject = new SerializedObject(spawner);
                Assert.That(spawnerObject.FindProperty("updateMode").enumValueIndex, Is.EqualTo((int)SpineBenchmarkUpdateMode.CentralizedUpdate));

                SerializedObject recorderObject = new SerializedObject(recorder);
                Assert.That(recorderObject.FindProperty("experimentName").stringValue, Is.EqualTo("CentralizedUpdate_25"));
            }
            finally
            {
                Object.DestroyImmediate(owner);
            }
        }
    }
}
