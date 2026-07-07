using NUnit.Framework;
using OptimizedSpine.Benchmark;
using Spine.Unity;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace OptimizedSpine.Tests
{
    public sealed class SpineBenchmarkSpawnerTests
    {
        private const string DefaultSkeletonPath =
            "Assets/Samples/spine-unity Runtime/4.3.95/Spine Examples/Spine Skeletons/spineboy-pro/spineboy-pro_SkeletonData.asset";

        [Test]
        public void Rebuild_CreatesRequestedSkeletonAnimations()
        {
            EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

            SkeletonDataAsset skeletonDataAsset = AssetDatabase.LoadAssetAtPath<SkeletonDataAsset>(DefaultSkeletonPath);
            Assert.That(skeletonDataAsset, Is.Not.Null, $"Missing test SkeletonDataAsset at {DefaultSkeletonPath}");

            GameObject owner = new GameObject("SpawnerUnderTest");
            SpineBenchmarkSpawner spawner = owner.AddComponent<SpineBenchmarkSpawner>();

            SerializedObject serializedSpawner = new SerializedObject(spawner);
            serializedSpawner.FindProperty("skeletonDataAsset").objectReferenceValue = skeletonDataAsset;
            serializedSpawner.FindProperty("animationName").stringValue = "run";
            serializedSpawner.FindProperty("instanceCount").intValue = 3;
            serializedSpawner.FindProperty("columns").intValue = 2;
            serializedSpawner.FindProperty("spacing").vector2Value = Vector2.one;
            serializedSpawner.FindProperty("origin").vector3Value = Vector3.zero;
            serializedSpawner.ApplyModifiedPropertiesWithoutUndo();

            spawner.Rebuild();

            Assert.That(spawner.SpawnedCount, Is.EqualTo(3));
            Assert.That(owner.transform.childCount, Is.EqualTo(3));
            Assert.That(owner.transform.GetChild(0).GetComponent<SkeletonAnimation>(), Is.Not.Null);

            spawner.Clear();
            Object.DestroyImmediate(owner);
        }
    }
}
