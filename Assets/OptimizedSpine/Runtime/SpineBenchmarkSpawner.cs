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
        [Header("Spine 资源")]
        [SerializeField, InspectorName("Skeleton 数据资源"), Tooltip("用于生成 benchmark 实例的 SkeletonDataAsset。")]
        private SkeletonDataAsset skeletonDataAsset;

        [SerializeField, InspectorName("动画名称"), SpineAnimation(dataField: "skeletonDataAsset"), Tooltip("每个实例默认播放的 Spine 动画。")]
        private string animationName = "run";

        [Header("生成布局")]
        [SerializeField, InspectorName("生成数量"), Min(0), Tooltip("要生成的 Spine 实例数量。")]
        private int instanceCount = 25;

        [SerializeField, InspectorName("每行数量"), Min(1), Tooltip("网格布局中每一行放多少个实例。")]
        private int columns = 5;

        [SerializeField, InspectorName("间距"), Tooltip("相邻实例之间的水平和垂直间距。")]
        private Vector2 spacing = new Vector2(2.2f, 2.2f);

        [SerializeField, InspectorName("起始位置"), Tooltip("第一个实例在本地坐标中的起始位置。")]
        private Vector3 origin = new Vector3(-4.4f, 2.2f, 0f);

        [Header("播放行为")]
        [SerializeField, InspectorName("开始时重建"), Tooltip("进入 Play Mode 时是否自动清理并重新生成实例。")]
        private bool rebuildOnStart = true;

        [SerializeField, InspectorName("播放动画"), Tooltip("生成实例后是否立即播放指定动画。")]
        private bool playAnimation = true;

        [SerializeField, InspectorName("随机起始时间"), Tooltip("让每个实例从不同动画时间点开始，避免所有动画完全同步。")]
        private bool randomizeStartTime = true;

        [SerializeField, InspectorName("Spine 更新模式"), Tooltip("Baseline 使用每个 SkeletonAnimation 自己的 Update；CentralizedUpdate 禁用单体 Update 并由本生成器统一调度。")]
        private SpineBenchmarkUpdateMode updateMode = SpineBenchmarkUpdateMode.Baseline;

        private readonly List<GameObject> spawned = new List<GameObject>();
        private readonly List<SkeletonAnimation> spawnedSkeletons = new List<SkeletonAnimation>();
#if UNITY_EDITOR
        private static readonly FieldInfo WasDeprecatedTransferredField =
            typeof(SkeletonAnimation).GetField("wasDeprecatedTransferred", BindingFlags.Instance | BindingFlags.NonPublic);
#endif

        public int SpawnedCount => spawned.Count;
        public int InstanceCount => instanceCount;
        public string AnimationName => animationName;
        public SkeletonDataAsset SkeletonDataAsset => skeletonDataAsset;
        public SpineBenchmarkUpdateMode UpdateMode => updateMode;
        public string UpdateModeLabel => updateMode.ToString();
        public int CentralizedUpdateTargetCount => updateMode == SpineBenchmarkUpdateMode.CentralizedUpdate ? spawnedSkeletons.Count : 0;

        private void Start()
        {
            if (rebuildOnStart)
                Rebuild();
        }

        private void Update()
        {
            if (updateMode != SpineBenchmarkUpdateMode.CentralizedUpdate)
                return;

            float deltaSeconds = Time.deltaTime;
            for (int index = 0; index < spawnedSkeletons.Count; index++)
            {
                SkeletonAnimation skeletonAnimation = spawnedSkeletons[index];
                if (skeletonAnimation == null)
                    continue;

                if (skeletonAnimation.enabled)
                    skeletonAnimation.enabled = false;

                skeletonAnimation.Update(deltaSeconds);
            }
        }

        private void LateUpdate()
        {
            if (updateMode != SpineBenchmarkUpdateMode.CentralizedUpdate)
                return;

            for (int index = 0; index < spawnedSkeletons.Count; index++)
            {
                SkeletonAnimation skeletonAnimation = spawnedSkeletons[index];
                if (skeletonAnimation == null || skeletonAnimation.Renderer == null)
                    continue;

                skeletonAnimation.Renderer.LateUpdate();
            }
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

                ApplyUpdateMode(skeletonAnimation);
                spawned.Add(spawnedTransform.gameObject);
                spawnedSkeletons.Add(skeletonAnimation);
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
            spawnedSkeletons.Clear();
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

        private void ApplyUpdateMode(SkeletonAnimation skeletonAnimation)
        {
            if (skeletonAnimation == null)
                return;

            skeletonAnimation.enabled = updateMode == SpineBenchmarkUpdateMode.Baseline;
        }
    }
}
