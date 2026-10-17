using System;
using UnityEngine;

namespace AngryBirds.EditorTools.Art
{
    /// <summary>Shape predicates in canvas pixel space, origin at the bottom left.</summary>
    internal static class Shapes
    {
        public static Func<Vector2, bool> Circle(Vector2 center, float radius)
        {
            float squared = radius * radius;
            return point => (point - center).sqrMagnitude <= squared;
        }

        public static Func<Vector2, bool> Ellipse(Vector2 center, Vector2 radius)
        {
            return point =>
            {
                float x = (point.x - center.x) / radius.x;
                float y = (point.y - center.y) / radius.y;
                return x * x + y * y <= 1f;
            };
        }

        public static Func<Vector2, bool> Rectangle(float minX, float minY, float maxX, float maxY)
        {
            return point => point.x >= minX && point.x <= maxX && point.y >= minY && point.y <= maxY;
        }

        public static Func<Vector2, bool> RoundRectangle(float minX, float minY, float maxX, float maxY, float radius)
        {
            Func<Vector2, bool> box = Rectangle(minX + radius, minY + radius, maxX - radius, maxY - radius);

            return point =>
            {
                if (box(point))
                {
                    return true;
                }

                float cx = Mathf.Clamp(point.x, minX + radius, maxX - radius);
                float cy = Mathf.Clamp(point.y, minY + radius, maxY - radius);
                return (point - new Vector2(cx, cy)).sqrMagnitude <= radius * radius;
            };
        }

        public static Func<Vector2, bool> Polygon(params Vector2[] vertices)
        {
            return point => Contains(vertices, point);
        }

        public static Func<Vector2, bool> Union(params Func<Vector2, bool>[] shapes)
        {
            return point =>
            {
                for (int i = 0; i < shapes.Length; i++)
                {
                    if (shapes[i](point))
                    {
                        return true;
                    }
                }

                return false;
            };
        }

        public static Func<Vector2, bool> Except(Func<Vector2, bool> shape, params Func<Vector2, bool>[] holes)
        {
            return point =>
            {
                if (!shape(point))
                {
                    return false;
                }

                for (int i = 0; i < holes.Length; i++)
                {
                    if (holes[i](point))
                    {
                        return false;
                    }
                }

                return true;
            };
        }

        public static Func<Vector2, bool> ThickSegment(Vector2 from, Vector2 to, float width)
        {
            Vector2 direction = to - from;
            float lengthSquared = direction.sqrMagnitude;

            if (lengthSquared < 0.0001f)
            {
                return Circle(from, width * 0.5f);
            }

            float halfWidth = width * 0.5f;

            return point =>
            {
                float t = Mathf.Clamp01(Vector2.Dot(point - from, direction) / lengthSquared);
                Vector2 closest = from + direction * t;
                return (point - closest).sqrMagnitude <= halfWidth * halfWidth;
            };
        }

        private static bool Contains(Vector2[] vertices, Vector2 point)
        {
            if (vertices == null || vertices.Length < 3)
            {
                return false;
            }

            bool inside = false;

            for (int i = 0, j = vertices.Length - 1; i < vertices.Length; j = i++)
            {
                if (vertices[i].y > point.y != vertices[j].y > point.y)
                {
                    float crossing = (vertices[j].x - vertices[i].x) * (point.y - vertices[i].y) /
                                     (vertices[j].y - vertices[i].y) + vertices[i].x;

                    if (point.x < crossing)
                    {
                        inside = !inside;
                    }
                }
            }

            return inside;
        }
    }

    /// <summary>Color functions for shapes.</summary>
    internal static class Shades
    {
        public static Func<Vector2, Color> Solid(Color color)
        {
            return _ => color;
        }

        public static Func<Vector2, Color> Vertical(Color bottom, Color top, float fromY, float toY)
        {
            return point => Color.Lerp(bottom, top, Mathf.InverseLerp(fromY, toY, point.y));
        }

        public static Func<Vector2, Color> Radial(Color inner, Color outer, Vector2 center, float radius)
        {
            return point => Color.Lerp(inner, outer, Mathf.Clamp01((point - center).magnitude / radius));
        }

        public static Func<Vector2, Color> Noise(Color baseColor, float amount, float scale)
        {
            return Noise(Shades.Solid(baseColor), amount, scale);
        }

        public static Func<Vector2, Color> Noise(Func<Vector2, Color> baseColor, float amount, float scale)
        {
            return point =>
            {
                Color color = baseColor(point);
                color.r = Mathf.Clamp01(color.r + (Noise2D(point * scale) - 0.5f) * amount);
                color.g = Mathf.Clamp01(color.g + (Noise2D(point * scale + new Vector2(31.7f, 12.1f)) - 0.5f) * amount);
                color.b = Mathf.Clamp01(color.b + (Noise2D(point * scale + new Vector2(77.3f, 45.9f)) - 0.5f) * amount);
                return color;
            };
        }

        public static Func<Vector2, Color> WithAlpha(Func<Vector2, Color> inner, float alpha)
        {
            return point =>
            {
                Color color = inner(point);
                color.a = alpha;
                return color;
            };
        }

        /// <summary>Deterministic value noise in the 0..1 range, so art is stable between runs.</summary>
        public static float Noise2D(Vector2 point)
        {
            int x = Mathf.FloorToInt(point.x);
            int y = Mathf.FloorToInt(point.y);
            float fx = point.x - x;
            float fy = point.y - y;
            fx = fx * fx * (3f - 2f * fx);
            fy = fy * fy * (3f - 2f * fy);

            float a = Hash(x, y);
            float b = Hash(x + 1, y);
            float c = Hash(x, y + 1);
            float d = Hash(x + 1, y + 1);

            return Mathf.Lerp(Mathf.Lerp(a, b, fx), Mathf.Lerp(c, d, fx), fy);
        }

        private static float Hash(int x, int y)
        {
            unchecked
            {
                int n = x * 374761393 + y * 668265263;
                n = (n ^ (n >> 13)) * 1274126177;
                return ((n ^ (n >> 16)) & 0xFFFFFF) / (float)0xFFFFFF;
            }
        }
    }
}
