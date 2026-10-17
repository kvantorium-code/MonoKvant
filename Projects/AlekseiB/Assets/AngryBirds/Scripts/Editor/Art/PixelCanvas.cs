using System;
using System.IO;
using UnityEngine;

namespace AngryBirds.EditorTools.Art
{
    /// <summary>
    /// Tiny software rasterizer used by the placeholder art generator.
    /// Draws anti aliased shapes into a RGBA buffer and writes a PNG.
    /// It exists so the project can produce placeholder sprites without any
    /// external art tool or image generation service.
    /// </summary>
    internal sealed class PixelCanvas
    {
        private const int SampleGrid = 3;

        private readonly Color[] _pixels;

        public PixelCanvas(int width, int height)
        {
            Width = width;
            Height = height;
            _pixels = new Color[width * height];
        }

        public int Width { get; }

        public int Height { get; }

        /// <summary>Fills every pixel where <paramref name="inside"/> is true.</summary>
        public void Fill(Func<Vector2, bool> inside, Func<Vector2, Color> shader)
        {
            if (inside == null || shader == null)
            {
                return;
            }

            float samples = SampleGrid * SampleGrid;

            for (int y = 0; y < Height; y++)
            {
                for (int x = 0; x < Width; x++)
                {
                    int hits = 0;

                    for (int sy = 0; sy < SampleGrid; sy++)
                    {
                        for (int sx = 0; sx < SampleGrid; sx++)
                        {
                            float px = x + (sx + 0.5f) / SampleGrid;
                            float py = y + (sy + 0.5f) / SampleGrid;

                            if (inside(new Vector2(px, py)))
                            {
                                hits++;
                            }
                        }
                    }

                    if (hits == 0)
                    {
                        continue;
                    }

                    Blend(x, y, shader(new Vector2(x + 0.5f, y + 0.5f)), hits / samples);
                }
            }
        }

        public void Fill(Func<Vector2, bool> inside, Color color)
        {
            Fill(inside, _ => color);
        }

        /// <summary>Copies the color of opaque neighbours into transparent pixels to avoid dark fringes.</summary>
        public void DilateAlpha()
        {
            Color[] copy = (Color[])_pixels.Clone();

            for (int y = 0; y < Height; y++)
            {
                for (int x = 0; x < Width; x++)
                {
                    int index = Index(x, y);

                    if (copy[index].a > 0.02f)
                    {
                        continue;
                    }

                    Color neighbour = TransparentNeighbour(copy, x, y);

                    if (neighbour.a > 0f)
                    {
                        _pixels[index] = new Color(neighbour.r, neighbour.g, neighbour.b, 0f);
                    }
                }
            }
        }

        public void SavePng(string absolutePath)
        {
            var texture = new Texture2D(Width, Height, TextureFormat.RGBA32, false);
            texture.SetPixels(_pixels);
            texture.Apply();

            string directory = Path.GetDirectoryName(absolutePath);
            if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }

            File.WriteAllBytes(absolutePath, texture.EncodeToPNG());
            UnityEngine.Object.DestroyImmediate(texture);
        }

        private Color TransparentNeighbour(Color[] source, int x, int y)
        {
            for (int i = 0; i < 4; i++)
            {
                int nx = x + (i == 0 ? 1 : i == 1 ? -1 : 0);
                int ny = y + (i == 2 ? 1 : i == 3 ? -1 : 0);

                if (nx < 0 || ny < 0 || nx >= Width || ny >= Height)
                {
                    continue;
                }

                Color candidate = source[Index(nx, ny)];

                if (candidate.a > 0.4f)
                {
                    return candidate;
                }
            }

            return new Color(0f, 0f, 0f, 0f);
        }

        private void Blend(int x, int y, Color source, float coverage)
        {
            int index = Index(x, y);
            Color destination = _pixels[index];

            float alpha = source.a * coverage;

            if (alpha <= 0f)
            {
                return;
            }

            _pixels[index] = new Color(
                source.r * alpha + destination.r * (1f - alpha),
                source.g * alpha + destination.g * (1f - alpha),
                source.b * alpha + destination.b * (1f - alpha),
                alpha + destination.a * (1f - alpha));
        }

        private int Index(int x, int y)
        {
            return y * Width + x;
        }
    }
}
