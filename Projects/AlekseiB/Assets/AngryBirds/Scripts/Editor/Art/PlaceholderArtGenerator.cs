using System;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace AngryBirds.EditorTools.Art
{
    /// <summary>
    /// Generates the placeholder sprite set of the game.
    /// The art is intentionally simple and fully deterministic: it exists so the game is
    /// playable and testable without an artist, and can be replaced file by file later.
    /// </summary>
    internal static class PlaceholderArtGenerator
    {
        public const string SpriteFolder = "Assets/AngryBirds/Content/Sprites";
        private const int PixelsPerUnit = 200;

        [MenuItem("Tools/AngryBirds/1. Generate Placeholder Art", false, 10)]
        public static void GenerateAll()
        {
            EnsureFolder(SpriteFolder);

            Save("bird", DrawBird());
            Save("pig", DrawPig());
            Save("block_wood_square", DrawWood(200, 200, true));
            Save("block_wood_wide", DrawWood(400, 100, true));
            Save("block_wood_tall", DrawWood(100, 400, false));
            Save("block_glass_square", DrawGlass(200, 200));
            Save("block_glass_wide", DrawGlass(400, 100));
            Save("block_glass_tall", DrawGlass(100, 400));
            Save("block_stone_square", DrawStone(200, 200));
            Save("block_stone_wide", DrawStone(400, 100));
            Save("block_stone_tall", DrawStone(100, 400));
            Save("ground", DrawGround(800, 120));
            Save("dot", DrawDot(64));
            Save("star", DrawStar(128, new Color(1f, 0.84f, 0.25f), new Color(0.72f, 0.52f, 0.06f)));
            Save("star_empty", DrawStar(128, new Color(0.35f, 0.36f, 0.4f, 0.9f), new Color(0.2f, 0.21f, 0.24f, 0.9f)));
            Save("chip", DrawChip(64));
            Save("slingshot_back", DrawSlingshotBack(200, 400));
            Save("slingshot_front", DrawSlingshotFront(200, 400));
            Save("cloud", DrawCloud(600, 300));
            Save("sky", DrawSky(8, 256));

            AssetDatabase.Refresh();
            Debug.Log($"[PlaceholderArtGenerator] Placeholder sprites written to {SpriteFolder}.");
        }

        private static PixelCanvas DrawBird()
        {
            var canvas = new PixelCanvas(224, 224);
            Color outline = new Color(0.33f, 0.07f, 0.07f);
            Color bodyTop = new Color(0.9f, 0.24f, 0.19f);
            Color bodyBottom = new Color(0.62f, 0.1f, 0.09f);

            // Tail feathers sit behind the body.
            canvas.Fill(Shapes.Polygon(
                new Vector2(96, 150), new Vector2(20, 176), new Vector2(34, 130), new Vector2(96, 126)),
                Shades.Solid(new Color(0.5f, 0.09f, 0.08f)));
            canvas.Fill(Shapes.Polygon(
                new Vector2(96, 116), new Vector2(14, 104), new Vector2(40, 66), new Vector2(96, 82)),
                Shades.Solid(new Color(0.44f, 0.08f, 0.07f)));

            canvas.Fill(Shapes.Circle(new Vector2(112, 106), 94), Shades.Solid(outline));
            canvas.Fill(
                Shapes.Circle(new Vector2(112, 106), 88),
                Shades.Vertical(bodyBottom, bodyTop, 18, 194));
            canvas.Fill(
                Shapes.Ellipse(new Vector2(130, 84), new Vector2(58, 44)),
                Shades.Solid(new Color(0.96f, 0.75f, 0.38f, 0.95f)));

            // Beak.
            canvas.Fill(Shapes.Polygon(
                new Vector2(184, 118), new Vector2(222, 104), new Vector2(184, 90)),
                Shades.Solid(new Color(1f, 0.72f, 0.18f)));
            canvas.Fill(Shapes.Polygon(
                new Vector2(184, 104), new Vector2(214, 100), new Vector2(184, 92)),
                Shades.Solid(new Color(0.85f, 0.5f, 0.08f)));

            // Eye.
            canvas.Fill(Shapes.Circle(new Vector2(126, 148), 27), Shades.Solid(Color.white));
            canvas.Fill(Shapes.Circle(new Vector2(133, 143), 13), Shades.Solid(new Color(0.15f, 0.1f, 0.1f)));
            canvas.Fill(Shapes.Circle(new Vector2(139, 152), 5), Shades.Solid(Color.white));
            canvas.Fill(
                Shapes.ThickSegment(new Vector2(98, 174), new Vector2(158, 164), 9),
                Shades.Solid(new Color(0.35f, 0.06f, 0.06f)));

            canvas.DilateAlpha();
            return canvas;
        }

        private static PixelCanvas DrawPig()
        {
            var canvas = new PixelCanvas(224, 224);
            Color outline = new Color(0.16f, 0.32f, 0.11f);
            Color headTop = new Color(0.66f, 0.88f, 0.4f);
            Color headBottom = new Color(0.36f, 0.66f, 0.22f);

            // Ears behind the head.
            canvas.Fill(Shapes.Polygon(
                new Vector2(52, 186), new Vector2(96, 168), new Vector2(46, 148)), Shades.Solid(outline));
            canvas.Fill(Shapes.Polygon(
                new Vector2(172, 186), new Vector2(128, 168), new Vector2(178, 148)), Shades.Solid(outline));

            canvas.Fill(Shapes.Circle(new Vector2(112, 96), 92), Shades.Solid(outline));
            canvas.Fill(
                Shapes.Circle(new Vector2(112, 96), 86),
                Shades.Vertical(headBottom, headTop, 10, 182));

            // Snout.
            canvas.Fill(
                Shapes.Ellipse(new Vector2(112, 76), new Vector2(48, 36)),
                Shades.Vertical(new Color(0.5f, 0.76f, 0.3f), new Color(0.78f, 0.93f, 0.5f), 40, 112));
            canvas.Fill(Shapes.Ellipse(new Vector2(96, 76), new Vector2(11, 15)), Shades.Solid(new Color(0.2f, 0.42f, 0.14f)));
            canvas.Fill(Shapes.Ellipse(new Vector2(128, 76), new Vector2(11, 15)), Shades.Solid(new Color(0.2f, 0.42f, 0.14f)));

            // Eyes.
            canvas.Fill(Shapes.Circle(new Vector2(84, 132), 25), Shades.Solid(Color.white));
            canvas.Fill(Shapes.Circle(new Vector2(140, 132), 25), Shades.Solid(Color.white));
            canvas.Fill(Shapes.Circle(new Vector2(88, 128), 12), Shades.Solid(new Color(0.12f, 0.1f, 0.1f)));
            canvas.Fill(Shapes.Circle(new Vector2(144, 128), 12), Shades.Solid(new Color(0.12f, 0.1f, 0.1f)));
            canvas.Fill(Shapes.Circle(new Vector2(93, 136), 5), Shades.Solid(Color.white));
            canvas.Fill(Shapes.Circle(new Vector2(149, 136), 5), Shades.Solid(Color.white));

            canvas.DilateAlpha();
            return canvas;
        }

        private static PixelCanvas DrawWood(int width, int height, bool verticalPlanks)
        {
            var canvas = new PixelCanvas(width, height);
            Color frame = new Color(0.36f, 0.22f, 0.1f);

            canvas.Fill(Shapes.RoundRectangle(3, 3, width - 3, height - 3, 12), Shades.Solid(frame));

            int strips = Mathf.Clamp(Mathf.RoundToInt((verticalPlanks ? width : height) / 130f), 2, 5);
            float margin = 11f;
            float span = (verticalPlanks ? width : height) - margin * 2f;
            float stripSize = (span - (strips - 1) * 6f) / strips;

            for (int i = 0; i < strips; i++)
            {
                float start = margin + i * (stripSize + 6f);
                float end = start + stripSize;
                Func<Vector2, bool> shape = verticalPlanks
                    ? Shapes.RoundRectangle(start, margin, end, height - margin, 5)
                    : Shapes.RoundRectangle(margin, start, width - margin, end, 5);

                float low = verticalPlanks ? margin : start;
                float high = verticalPlanks ? height - margin : end;

                canvas.Fill(shape, Shades.Noise(Shades.Vertical(
                    new Color(0.68f, 0.46f, 0.24f),
                    new Color(0.84f, 0.63f, 0.36f),
                    low,
                    high), 0.1f, 0.12f));
            }

            canvas.DilateAlpha();
            return canvas;
        }

        private static PixelCanvas DrawGlass(int width, int height)
        {
            var canvas = new PixelCanvas(width, height);
            Color border = new Color(0.82f, 0.94f, 1f, 0.95f);
            Color pane = new Color(0.68f, 0.87f, 0.98f, 0.42f);

            canvas.Fill(Shapes.RoundRectangle(3, 3, width - 3, height - 3, 12), Shades.Solid(border));
            canvas.Fill(Shapes.RoundRectangle(12, 12, width - 12, height - 12, 8), Shades.Solid(pane));
            canvas.Fill(
                Shapes.ThickSegment(
                    new Vector2(width * 0.22f, height * 0.24f),
                    new Vector2(width * 0.5f, height * 0.8f),
                    Mathf.Min(width, height) * 0.12f),
                Shades.Solid(new Color(1f, 1f, 1f, 0.35f)));

            canvas.DilateAlpha();
            return canvas;
        }

        private static PixelCanvas DrawStone(int width, int height)
        {
            var canvas = new PixelCanvas(width, height);
            Color frame = new Color(0.3f, 0.31f, 0.34f);
            Color baseColor = new Color(0.56f, 0.57f, 0.6f);

            canvas.Fill(Shapes.RoundRectangle(3, 3, width - 3, height - 3, 12), Shades.Solid(frame));
            canvas.Fill(
                Shapes.RoundRectangle(11, 11, width - 11, height - 11, 9),
                Shades.Noise(baseColor, 0.16f, 0.14f));

            // A few darker chips.
            Color chip = new Color(0.42f, 0.43f, 0.46f);
            canvas.Fill(Shapes.Circle(new Vector2(width * 0.28f, height * 0.66f), Mathf.Min(width, height) * 0.07f), Shades.Solid(chip));
            canvas.Fill(Shapes.Circle(new Vector2(width * 0.68f, height * 0.36f), Mathf.Min(width, height) * 0.05f), Shades.Solid(chip));
            canvas.Fill(Shapes.Circle(new Vector2(width * 0.5f, height * 0.5f), Mathf.Min(width, height) * 0.04f), Shades.Solid(chip));

            canvas.DilateAlpha();
            return canvas;
        }

        private static PixelCanvas DrawGround(int width, int height)
        {
            var canvas = new PixelCanvas(width, height);
            float grassHeight = height * 0.28f;

            canvas.Fill(
                Shapes.Rectangle(0, 0, width, height),
                Shades.Noise(new Color(0.55f, 0.38f, 0.22f), 0.14f, 0.1f));
            canvas.Fill(
                Shapes.Rectangle(0, height - grassHeight, width, height),
                Shades.Noise(new Color(0.35f, 0.68f, 0.25f), 0.12f, 0.18f));
            canvas.Fill(
                Shapes.Rectangle(0, height - grassHeight - 4, width, height - grassHeight),
                Shades.Solid(new Color(0.26f, 0.52f, 0.18f)));

            Color pebble = new Color(0.42f, 0.29f, 0.18f);
            for (int i = 0; i < 8; i++)
            {
                float x = (i * 97 + 23) % width;
                float y = 12 + (i * 53) % Mathf.Max(1, (int)(height - grassHeight - 20));
                canvas.Fill(Shapes.Circle(new Vector2(x, y), 5), Shades.Solid(pebble));
            }

            canvas.DilateAlpha();
            return canvas;
        }

        private static PixelCanvas DrawDot(int size)
        {
            var canvas = new PixelCanvas(size, size);
            Vector2 center = new Vector2(size * 0.5f, size * 0.5f);

            canvas.Fill(Shapes.Circle(center, size * 0.46f), Shades.Solid(new Color(1f, 1f, 1f, 0.18f)));
            canvas.Fill(Shapes.Circle(center, size * 0.36f), Shades.Solid(new Color(1f, 1f, 1f, 0.5f)));
            canvas.Fill(Shapes.Circle(center, size * 0.26f), Shades.Solid(new Color(1f, 1f, 1f, 0.95f)));

            return canvas;
        }

        private static PixelCanvas DrawStar(int size, Color fill, Color outline)
        {
            var canvas = new PixelCanvas(size, size);
            Vector2 center = new Vector2(size * 0.5f, size * 0.5f);
            float outer = size * 0.46f;
            float inner = size * 0.2f;

            var points = new Vector2[10];
            for (int i = 0; i < 10; i++)
            {
                float radius = i % 2 == 0 ? outer : inner;
                float angle = Mathf.PI * 0.5f + i * Mathf.PI / 5f;
                points[i] = center + new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)) * radius;
            }

            canvas.Fill(Shapes.Polygon(points), Shades.Solid(outline));

            var inner2 = new Vector2[10];
            for (int i = 0; i < 10; i++)
            {
                Vector2 direction = (points[i] - center).normalized;
                inner2[i] = center + direction * (points[i] - center).magnitude * 0.82f;
            }

            canvas.Fill(Shapes.Polygon(inner2), Shades.Solid(fill));
            canvas.DilateAlpha();
            return canvas;
        }

        private static PixelCanvas DrawChip(int size)
        {
            var canvas = new PixelCanvas(size, size);
            canvas.Fill(Shapes.RoundRectangle(5, 5, size - 5, size - 5, 7), Shades.Solid(Color.white));
            canvas.DilateAlpha();
            return canvas;
        }

        private static PixelCanvas DrawSlingshotBack(int width, int height)
        {
            var canvas = new PixelCanvas(width, height);
            Color dark = new Color(0.24f, 0.14f, 0.07f);
            Color wood = new Color(0.55f, 0.35f, 0.18f);

            canvas.Fill(Shapes.RoundRectangle(width * 0.3f - 6, 0, width * 0.7f + 6, height * 0.56f, 10), Shades.Solid(dark));
            canvas.Fill(Shapes.RoundRectangle(width * 0.3f, 6, width * 0.7f, height * 0.55f, 8), Shades.Solid(wood));
            canvas.Fill(Shapes.RoundRectangle(width * 0.22f, 0, width * 0.78f, height * 0.11f, 8), Shades.Solid(dark));

            canvas.Fill(Shapes.Polygon(
                new Vector2(width * 0.36f, height * 0.5f),
                new Vector2(width * 0.1f, height * 0.96f),
                new Vector2(width * 0.28f, height),
                new Vector2(width * 0.46f, height * 0.56f)), Shades.Solid(dark));

            canvas.DilateAlpha();
            return canvas;
        }

        private static PixelCanvas DrawSlingshotFront(int width, int height)
        {
            var canvas = new PixelCanvas(width, height);
            Color dark = new Color(0.24f, 0.14f, 0.07f);

            canvas.Fill(Shapes.Polygon(
                new Vector2(width * 0.64f, height * 0.5f),
                new Vector2(width * 0.72f, height),
                new Vector2(width * 0.9f, height * 0.96f),
                new Vector2(width * 0.64f, height * 0.56f)), Shades.Solid(dark));

            canvas.DilateAlpha();
            return canvas;
        }

        private static PixelCanvas DrawCloud(int width, int height)
        {
            var canvas = new PixelCanvas(width, height);
            float r = height * 0.34f;
            float y = height * 0.42f;

            canvas.Fill(Shapes.Circle(new Vector2(width * 0.28f, y), r), Shades.Solid(new Color(1f, 1f, 1f, 0.92f)));
            canvas.Fill(Shapes.Circle(new Vector2(width * 0.46f, y + r * 0.5f), r * 1.15f), Shades.Solid(new Color(1f, 1f, 1f, 0.92f)));
            canvas.Fill(Shapes.Circle(new Vector2(width * 0.68f, y), r * 0.95f), Shades.Solid(new Color(1f, 1f, 1f, 0.92f)));
            canvas.Fill(Shapes.RoundRectangle(width * 0.24f, y - r * 0.7f, width * 0.72f, y + r * 0.2f, r * 0.6f), Shades.Solid(new Color(1f, 1f, 1f, 0.92f)));

            return canvas;
        }

        private static PixelCanvas DrawSky(int width, int height)
        {
            var canvas = new PixelCanvas(width, height);
            canvas.Fill(
                Shapes.Rectangle(0, 0, width, height),
                Shades.Vertical(new Color(0.66f, 0.85f, 0.96f), new Color(0.32f, 0.62f, 0.88f), 0, height));
            return canvas;
        }

        private static void Save(string name, PixelCanvas canvas)
        {
            string assetPath = $"{SpriteFolder}/{name}.png";
            string absolutePath = Path.Combine(Directory.GetCurrentDirectory(), assetPath);

            canvas.SavePng(absolutePath);
            AssetDatabase.ImportAsset(assetPath, ImportAssetOptions.ForceUpdate);
            ConfigureImporter(assetPath);
        }

        private static void ConfigureImporter(string assetPath)
        {
            var importer = AssetImporter.GetAtPath(assetPath) as TextureImporter;
            if (importer == null)
            {
                return;
            }

            importer.textureType = TextureImporterType.Sprite;
            importer.spriteImportMode = SpriteImportMode.Single;
            importer.spritePixelsPerUnit = PixelsPerUnit;
            importer.filterMode = FilterMode.Bilinear;

            // Sprites are minified a lot in the play area, so mipmaps keep them crisp
            // instead of shimmering.
            importer.mipmapEnabled = true;
            importer.alphaIsTransparency = true;
            importer.wrapMode = TextureWrapMode.Clamp;
            importer.textureCompression = TextureImporterCompression.Uncompressed;
            importer.maxTextureSize = 1024;
            importer.SaveAndReimport();
        }

        private static void EnsureFolder(string assetPath)
        {
            if (AssetDatabase.IsValidFolder(assetPath))
            {
                return;
            }

            string[] parts = assetPath.Split('/');
            string current = parts[0];

            for (int i = 1; i < parts.Length; i++)
            {
                string next = current + "/" + parts[i];
                if (!AssetDatabase.IsValidFolder(next))
                {
                    AssetDatabase.CreateFolder(current, parts[i]);
                }

                current = next;
            }
        }
    }
}
