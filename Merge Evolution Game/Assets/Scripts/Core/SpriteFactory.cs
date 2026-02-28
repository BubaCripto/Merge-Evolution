using UnityEngine;

namespace MergeEvolution.Core
{
    public static class SpriteFactory
    {
        private static Sprite _whiteSquare;

        public static Sprite WhiteSquare
        {
            get
            {
                if (_whiteSquare != null) return _whiteSquare;

                var tex = new Texture2D(1, 1, TextureFormat.RGBA32, false);
                tex.SetPixel(0, 0, Color.white);
                tex.Apply();
                tex.filterMode = FilterMode.Point;

                _whiteSquare = Sprite.Create(tex, new Rect(0, 0, 1, 1), new Vector2(0.5f, 0.5f), 1f);
                _whiteSquare.name = "WhiteSquareRuntime";
                return _whiteSquare;
            }
        }
    }
}
