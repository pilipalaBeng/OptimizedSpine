using System.Collections.Generic;
using Spine;
using Spine.Unity;
using UnityEngine;
using SpineRuntimeAnimation = Spine.Animation;
#if UNITY_EDITOR
using System.Reflection;
#endif

namespace OptimizedSpine.Benchmark
{
    public sealed class SpineBenchmarkSpawner : MonoBehaviour
    {
        [SerializeField] private SkeletonDataAsset skeletonDataAsset;
        [SerializeField, SpineAnimation(dataField: "skeletonDataAsset")] private string animationName = "run";
        [SerializeField, Min(0)] private int instanceCount = 25;
        [SerializeField, Min(1)] private int columns = 5;
        [SerializeField] private Vector2 spacing = new Vector2(2.2f, 2.2f);
        [SerializeField] private Vector3 origin = new Vector3(-4.4f, 2.2f, 0f);
        [SerializeField] private bool rebuildOnStart = true;
        [SerializeField] private bool playAnimation = true;
        [SerializeField] private bool randomizeStartTime = true;

        private readonly List<GameObject> spawned = new List<GameObject>();
#if UNITY_EDITOR
        private static readonly FieldInfo WasDeprecatedTransferredField =
            typeof(SkeletonAnimation).GetField("wasDeprecatedTransferred", BindingFlags.Instance | BindingFlags.NonPublic);
#endif

        public int SpawnedCount => spawned.Count;
        public int InstanceCount => instanceCount;
        public string AnimationName => animationName;
        public SkeletonDataAsset SkeletonDataAsset => skeletonDataAsset;

        private void Start()
        {
            if (rebuildOnStart)
                Rebuild();
        }

        private void OnDestroy()
        {
            Clear();
        }

        [ContextMenu("Rebuild Benchmark Instances")]
        public void Rebuild()
        {
            Clear();

            if (skeletonDataAsset == null || instanceCount <= 0)
                return;

            SkeletonData skeletonData = skeletonDataAsset.GetSkeletonData(false);
            SpineRuntimeAnimation animation = !string.IsNullOrEmpty(animationName) ? skeletonData.FindAnimation(animationName) : null;

            if (playAnimation && animation == null && !string.IsNullOrEmpty(animationName))
                Debug.LogWarning($"Spine animation '{animationName}' was not found on {skeletonDataAsset.name}.", this);

            for (int index = 0; index < instanceCount; index++)
            {
                SkeletonAnimation skeletonAnimation = CreateSkeletonAnimation();
                Transform spawnedTransform = skeletonAnimation.transform;
                spawnedTransform.SetParent(transform, false);
                spawnedTransform.localPosition = SpineBenchmarkLayout.GridPosition(index, columns, spacing, origin);
                spawnedTransform.localRotation = Quaternion.identity;
                spawnedTransform.localScale = Vector3.one;
                spawnedTransform.gameObject.name = $"Spine_{index:000}";

                if (playAnimation && animation != null)
                    Play(skeletonAnimation, animation, index);

                spawned.Add(spawnedTransform.gameObject);
            }
        }

        [ContextMenu("Clear Benchmark Instances")]
        public void Clear()
        {
            for (int index = spawned.Count - 1; index >= 0; index--)
            {
                GameObject spawnedObject = spawned[index];
                if (spawnedObject == null)
                    continue;

                if (Application.isPlaying)
                    Destroy(spawnedObject);
                else
                    DestroyImmediate(spawnedObject);
            }

            spawned.Clear();
        }

        private void Play(SkeletonAnimation skeletonAnimation, SpineRuntimeAnimation animation, int index)
        {
            skeletonAnimation.Initialize(false);
            TrackEntry trackEntry = skeletonAnimation.AnimationState.SetAnimation(0, animation, true);

            if (randomizeStartTime && animation.Duration > 0f)
            {
                float normalizedOffset = Mathf.Repeat(index * 0.371f, 1f);
                trackEntry.TrackTime = normalizedOffset * animation.Duration;
            }
        }

        private SkeletonAnimation CreateSkeletonAnimation()
        {
            GameObject spawnedObject = new GameObject("Spine");
            spawnedObject.SetActive(false);

            SkeletonRenderer skeletonRenderer = spawnedObject.AddComponent<SkeletonRenderer>();
            SkeletonAnimation skeletonAnimation = spawnedObject.AddComponent<SkeletonAnimation>();

            skeletonRenderer.SkeletonDataAsset = skeletonDataAsset;
            skeletonRenderer.Animation = skeletonAnimation;
            skeletonRenderer.Initialize(false, quiet: true);
            skeletonAnimation.Initialize(false, quiet: true);
#if UNITY_EDITOR
            if (!Application.isPlaying)
                WasDeprecatedTransferredField?.SetValue(skeletonAnimation, true);
#endif

            spawnedObject.SetActive(true);
            return skeletonAnimation;
        }
    }
}
