using NUnit.Framework;
using OptimizedSpine.Benchmark;
using UnityEngine;

namespace OptimizedSpine.Tests
{
    public sealed class SpineBenchmarkLayoutTests
    {
        [Test]
        public void GridPosition_UsesColumnsAndSpacing()
        {
            Vector3 position = SpineBenchmarkLayout.GridPosition(
                index: 7,
                columns: 5,
                spacing: new Vector2(2f, 3f),
                origin: new Vector3(-4f, 6f, 1f));

            Assert.That(position, Is.EqualTo(new Vector3(0f, 3f, 1f)));
        }

        [Test]
        public void GridPosition_ClampsColumnsAndIndex()
        {
            Vector3 position = SpineBenchmarkLayout.GridPosition(
                index: -3,
                columns: 0,
                spacing: new Vector2(2f, 3f),
                origin: Vector3.one);

            Assert.That(position, Is.EqualTo(Vector3.one));
        }
    }
}
