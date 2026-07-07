using UnityEngine;

namespace OptimizedSpine.Benchmark
{
    public static class SpineBenchmarkLayout
    {
        public static Vector3 GridPosition(int index, int columns, Vector2 spacing, Vector3 origin)
        {
            columns = Mathf.Max(1, columns);
            index = Mathf.Max(0, index);

            int row = index / columns;
            int column = index % columns;

            return origin + new Vector3(column * spacing.x, -row * spacing.y, 0f);
        }
    }
}
