using UnityEngine;

namespace SDClub.Core
{
    /// <summary>
    /// Math helpers for 2D physics queries. No MonoBehaviour dependency.
    /// </summary>
    public static class Physics2DHelper
    {
        // Layer mask helpers
        public static bool IsInLayerMask(int layer, LayerMask mask)
        {
            return (mask.value & (1 << layer)) != 0;
        }

        public static bool IsInLayerMask(GameObject go, LayerMask mask)
        {
            return IsInLayerMask(go.layer, mask);
        }

        public static int GetLayerMask(params int[] layers)
        {
            int mask = 0;
            foreach (int layer in layers)
            {
                mask |= 1 << layer;
            }
            return mask;
        }

        // Collision primitives
        public struct Rect
        {
            public Vector2 Min;
            public Vector2 Max;

            public Vector2 Center => (Min + Max) * 0.5f;
            public Vector2 Size => Max - Min;

            public Rect(Vector2 min, Vector2 max)
            {
                this.Min = min;
                this.Max = max;
            }

            public bool Overlaps(Rect other)
            {
                return Min.x <= other.Max.x && Max.x >= other.Min.x
                    && Min.y <= other.Max.y && Max.y >= other.Min.y;
            }

            public bool Contains(Vector2 point)
            {
                return point.x >= Min.x && point.x <= Max.x
                    && point.y >= Min.y && point.y <= Max.y;
            }
        }

        public struct Circle
        {
            public Vector2 Center;
            public float Radius;

            public Circle(Vector2 center, float radius)
            {
                this.Center = center;
                this.Radius = radius;
            }

            public bool Contains(Vector2 point)
            {
                return Vector2.Distance(Center, point) <= Radius;
            }

            public bool Overlaps(Circle other)
            {
                return Vector2.Distance(Center, other.Center) <= Radius + other.Radius;
            }

            public bool Overlaps(in Rect rect)
            {
                Vector2 closest = new Vector2(
                    Mathf.Clamp(Center.x, rect.Min.x, rect.Max.x),
                    Mathf.Clamp(Center.y, rect.Min.y, rect.Max.y)
                );
                return Vector2.Distance(Center, closest) <= Radius;
            }
        }

        // Collision checks
        public static bool CircleCircle(Vector2 centerA, float radiusA, Vector2 centerB, float radiusB)
        {
            float distSq = (centerA - centerB).sqrMagnitude;
            float radiusSum = radiusA + radiusB;
            return distSq <= radiusSum * radiusSum;
        }

        public static bool PointInRect(Vector2 point, in Rect rect)
        {
            return rect.Contains(point);
        }

        public static bool PointInCircle(Vector2 point, Vector2 center, float radius)
        {
            return (point - center).sqrMagnitude <= radius * radius;
        }

        public static bool RectOverlap(in Rect a, in Rect b)
        {
            return a.Overlaps(b);
        }

        // Overlap helpers using Physics2D (Unity's 2D physics)
        public static int OverlapCircleAll(Vector2 point, float radius, int layerMask, Collider2D[] results)
        {
            return Physics2D.OverlapCircleNonAlloc(point, radius, results, layerMask);
        }

        public static int OverlapBoxAll(in Rect rect, int layerMask, Collider2D[] results)
        {
            return Physics2D.OverlapBoxNonAlloc(rect.Center, rect.Size, 0f, results, layerMask);
        }
    }
}
