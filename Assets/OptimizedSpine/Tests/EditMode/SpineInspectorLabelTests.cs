using System;
using System.Reflection;
using NUnit.Framework;
using OptimizedSpine.Benchmark;
using UnityEngine;

namespace OptimizedSpine.Tests
{
    public sealed class SpineInspectorLabelTests
    {
        [TestCase(typeof(SpineBenchmarkSpawner), "skeletonDataAsset", "Skeleton 数据资源")]
        [TestCase(typeof(SpineBenchmarkSpawner), "animationName", "动画名称")]
        [TestCase(typeof(SpineBenchmarkSpawner), "instanceCount", "生成数量")]
        [TestCase(typeof(SpineBenchmarkSpawner), "columns", "每行数量")]
        [TestCase(typeof(SpineBenchmarkSpawner), "spacing", "间距")]
        [TestCase(typeof(SpineBenchmarkSpawner), "origin", "起始位置")]
        [TestCase(typeof(SpineBenchmarkSpawner), "rebuildOnStart", "开始时重建")]
        [TestCase(typeof(SpineBenchmarkSpawner), "playAnimation", "播放动画")]
        [TestCase(typeof(SpineBenchmarkSpawner), "randomizeStartTime", "随机起始时间")]
        [TestCase(typeof(SpineBenchmarkMetrics), "spawner", "生成器")]
        [TestCase(typeof(SpineBenchmarkMetrics), "smoothing", "平滑系数")]
        [TestCase(typeof(SpineBenchmarkMetrics), "targetFrameRate", "目标帧率")]
        [TestCase(typeof(SpineBenchmarkSnapshotRecorder), "spawner", "生成器")]
        [TestCase(typeof(SpineBenchmarkSnapshotRecorder), "experimentName", "实验名称")]
        [TestCase(typeof(SpineBenchmarkSnapshotRecorder), "skeletonAssetPath", "Skeleton 资源路径")]
        [TestCase(typeof(SpineBenchmarkSnapshotRecorder), "spineUnityVersion", "spine-unity 版本")]
        [TestCase(typeof(SpineBenchmarkSnapshotRecorder), "warmupSeconds", "预热秒数")]
        [TestCase(typeof(SpineBenchmarkSnapshotRecorder), "sampleSeconds", "采样秒数")]
        [TestCase(typeof(SpineBenchmarkSnapshotRecorder), "outputDirectory", "输出目录")]
        public void SerializedFields_HaveChineseInspectorNames(
            Type componentType,
            string fieldName,
            string expectedDisplayName)
        {
            FieldInfo field = componentType.GetField(fieldName, BindingFlags.Instance | BindingFlags.NonPublic);
            Assert.That(field, Is.Not.Null, $"{componentType.Name}.{fieldName} was not found.");

            InspectorNameAttribute inspectorName = field.GetCustomAttribute<InspectorNameAttribute>();
            Assert.That(inspectorName, Is.Not.Null, $"{componentType.Name}.{fieldName} is missing InspectorNameAttribute.");
            Assert.That(inspectorName.displayName, Is.EqualTo(expectedDisplayName));
        }
    }
}
