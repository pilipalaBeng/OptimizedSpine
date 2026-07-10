using OptimizedSpine.Benchmark;
using Spine.Unity;
using System;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace OptimizedSpine.EditorTools
{
    public static class SpineBenchmarkSceneBuilder
    {
        private const string BaselineScenePath = "Assets/OptimizedSpine/Scenes/Benchmark_01_Baseline.unity";
        private const string DefaultSkeletonPath =
            "Assets/Samples/spine-unity Runtime/4.3.95/Spine Examples/Spine Skeletons/spineboy-pro/spineboy-pro_SkeletonData.asset";

        [MenuItem("OptimizedSpine/Build Baseline Scene")]
        public static void BuildBaselineScene()
        {
            Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

            CreateCamera();
            SpineBenchmarkSpawner spawner = CreateBenchmarkRunner();
            AssignSpawnerDefaults(spawner, 25);

            EditorSceneManager.SaveScene(scene, BaselineScenePath);
            AssetDatabase.Refresh();
            Selection.activeObject = spawner.gameObject;

            Debug.Log($"Built Spine baseline benchmark scene at {BaselineScenePath}.");
        }

        [MenuItem("OptimizedSpine/Validate Baseline Spawner")]
        public static void ValidateBaselineSpawner()
        {
            GameObject owner = null;

            try
            {
                owner = new GameObject("BaselineSpawnerValidation")
                {
                    hideFlags = HideFlags.DontSave
                };

                SpineBenchmarkSpawner spawner = owner.AddComponent<SpineBenchmarkSpawner>();
                AssignSpawnerDefaults(spawner, 3);

                spawner.Rebuild();

                if (spawner.SpawnedCount != 3)
                    throw new InvalidOperationException($"Expected 3 spawned skeletons, got {spawner.SpawnedCount}.");

                if (owner.transform.childCount != 3)
                    throw new InvalidOperationException($"Expected 3 spawned children, got {owner.transform.childCount}.");

                if (owner.transform.GetChild(0).GetComponent<SkeletonAnimation>() == null)
                    throw new InvalidOperationException("Spawned child is missing SkeletonAnimation.");

                Debug.Log("OptimizedSpine baseline spawner validation passed.");
            }
            finally
            {
                if (owner != null)
                {
                    SpineBenchmarkSpawner spawner = owner.GetComponent<SpineBenchmarkSpawner>();
                    if (spawner != null)
                        spawner.Clear();

                    UnityEngine.Object.DestroyImmediate(owner);
                }
            }
        }

        [MenuItem("OptimizedSpine/Write Benchmark Snapshot")]
        public static void WriteBenchmarkSnapshot()
        {
            SpineBenchmarkSnapshotRecorder recorder = UnityEngine.Object.FindObjectOfType<SpineBenchmarkSnapshotRecorder>();
            if (recorder == null)
            {
                Debug.LogWarning("No SpineBenchmarkSnapshotRecorder found in the active scene.");
                return;
            }

            if (!EditorApplication.isPlaying)
            {
                const string message = "Benchmark snapshot is not ready: enter Play Mode and wait until the overlay shows Snapshot: Complete before writing.";
                Debug.LogWarning(message, recorder);
                EditorUtility.DisplayDialog("Benchmark Snapshot Not Ready", message, "OK");
                return;
            }

            if (!recorder.TryWriteSnapshot(out _, out string reason))
            {
                EditorUtility.DisplayDialog("Benchmark Snapshot Not Ready", reason, "OK");
                return;
            }

            AssetDatabase.Refresh();
        }

        private static void CreateCamera()
        {
            GameObject cameraObject = new GameObject("Main Camera");
            cameraObject.tag = "MainCamera";
            cameraObject.transform.position = new Vector3(0f, 0f, -10f);

            Camera camera = cameraObject.AddComponent<Camera>();
            camera.orthographic = true;
            camera.orthographicSize = 7f;
            camera.clearFlags = CameraClearFlags.SolidColor;
            camera.backgroundColor = new Color(0.055f, 0.06f, 0.07f, 1f);

            cameraObject.AddComponent<AudioListener>();
        }

        private static SpineBenchmarkSpawner CreateBenchmarkRunner()
        {
            GameObject runner = new GameObject("BenchmarkRunner");
            SpineBenchmarkSpawner spawner = runner.AddComponent<SpineBenchmarkSpawner>();
            SpineBenchmarkMetrics metrics = runner.AddComponent<SpineBenchmarkMetrics>();
            SpineBenchmarkSnapshotRecorder recorder = runner.AddComponent<SpineBenchmarkSnapshotRecorder>();

            SerializedObject metricsObject = new SerializedObject(metrics);
            metricsObject.FindProperty("spawner").objectReferenceValue = spawner;
            metricsObject.FindProperty("recorder").objectReferenceValue = recorder;
            metricsObject.FindProperty("smoothing").floatValue = 0.1f;
            metricsObject.FindProperty("targetFrameRate").intValue = -1;
            metricsObject.ApplyModifiedPropertiesWithoutUndo();

            SerializedObject recorderObject = new SerializedObject(recorder);
            recorderObject.FindProperty("spawner").objectReferenceValue = spawner;
            recorderObject.FindProperty("experimentName").stringValue = "Baseline";
            recorderObject.FindProperty("skeletonAssetPath").stringValue = DefaultSkeletonPath;
            recorderObject.FindProperty("spineUnityVersion").stringValue = "4.3.95";
            recorderObject.FindProperty("warmupSeconds").floatValue = 3f;
            recorderObject.FindProperty("sampleSeconds").floatValue = 10f;
            recorderObject.FindProperty("outputDirectory").stringValue = "docs/experiments";
            recorderObject.ApplyModifiedPropertiesWithoutUndo();

            return spawner;
        }

        private static void AssignSpawnerDefaults(SpineBenchmarkSpawner spawner, int instanceCount)
        {
            SkeletonDataAsset skeletonDataAsset = AssetDatabase.LoadAssetAtPath<SkeletonDataAsset>(DefaultSkeletonPath);
            if (skeletonDataAsset == null)
                Debug.LogWarning($"Default SkeletonDataAsset not found: {DefaultSkeletonPath}");

            SerializedObject spawnerObject = new SerializedObject(spawner);
            spawnerObject.FindProperty("skeletonDataAsset").objectReferenceValue = skeletonDataAsset;
            spawnerObject.FindProperty("animationName").stringValue = "run";
            spawnerObject.FindProperty("instanceCount").intValue = instanceCount;
            spawnerObject.FindProperty("columns").intValue = 5;
            spawnerObject.FindProperty("spacing").vector2Value = new Vector2(2.2f, 2.2f);
            spawnerObject.FindProperty("origin").vector3Value = new Vector3(-4.4f, 2.2f, 0f);
            spawnerObject.FindProperty("rebuildOnStart").boolValue = true;
            spawnerObject.FindProperty("playAnimation").boolValue = true;
            spawnerObject.FindProperty("randomizeStartTime").boolValue = true;
            spawnerObject.FindProperty("updateMode").enumValueIndex = (int)SpineBenchmarkUpdateMode.Baseline;
            spawnerObject.ApplyModifiedPropertiesWithoutUndo();
        }
    }
}
