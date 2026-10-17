using UnityEngine;

namespace AngryBirds.Infrastracture
{
    public static class SpriteFactory
    {
        private static Sprite _square;
        private static Sprite _circle;

        public static Sprite Square
        {
            get
            {
                if (_square == null)
                {
                    _square = BuildSquare();
                }

                return _square;
            }
        }

        public static Sprite Circle
        {
            get
            {
                if (_circle == null)
                {
                    _circle = BuildCircle(64);
                }

                return _circle;
            }
        }

        private static Sprite BuildSquare()
        {
            var texture = new Texture2D(4, 4, TextureFormat.RGBA32, false);
            var pixels = new Color[16];
            for (int i = 0; i < pixels.Length; i++)
            {
                pixels[i] = Color.white;
            }

            texture.SetPixels(pixels);
            texture.Apply();
            texture.filterMode = FilterMode.Bilinear;
            return Sprite.Create(texture, new Rect(0f, 0f, 4f, 4f), new Vector2(0.5f, 0.5f), 4f);
        }

        private static Sprite BuildCircle(int size)
        {
            var texture = new Texture2D(size, size, TextureFormat.RGBA32, false);
            float radius = size * 0.5f;
            var pixels = new Color[size * size];

            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    float dx = x + 0.5f - radius;
                    float dy = y + 0.5f - radius;
                    float distance = Mathf.Sqrt(dx * dx + dy * dy);
                    float alpha = Mathf.Clamp01(radius - distance);
                    pixels[y * size + x] = new Color(1f, 1f, 1f, alpha);
                }
            }

            texture.SetPixels(pixels);
            texture.Apply();
            texture.filterMode = FilterMode.Bilinear;
            return Sprite.Create(texture, new Rect(0f, 0f, size, size), new Vector2(0.5f, 0.5f), size);
        }
    }
}
