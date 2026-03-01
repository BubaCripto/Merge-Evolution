using System.Collections;
using System.Collections.Generic;
using MergeEvolution.Items;
using MergeEvolution.Logic;
using UnityEngine;

namespace MergeEvolution.Board
{
    public class BoardManager : MonoBehaviour
    {
        [Header("Grid")]
        public int width = 5;
        public int height = 8;
        public float cellSize = 1.05f;
        public Vector2 origin = new Vector2(-2.1f, -3.5f);
        public int colors = 4;

        [Header("Art")]
        public Sprite fallbackSprite;
        public Sprite[] eggSprites;
        public Sprite[] crackedSprites;
        public Sprite[] hatchingSprites;
        public Sprite[] babySprites;

        public Tile[,] Grid { get; private set; }
        public int Width => width;
        public int Height => height;
        public int ColorCount => Mathf.Max(1, colors);

        private MatchDetector _matchDetector;
        private MergeController _mergeController;
        private GravityController _gravityController;
        private bool _busy;

        private void Awake()
        {
            _matchDetector = gameObject.AddComponent<MatchDetector>();
            _mergeController = gameObject.AddComponent<MergeController>();
            _gravityController = gameObject.AddComponent<GravityController>();
        }

        private void Start()
        {
            EnsureSprites();
            BuildGrid();
            FillWithoutInitialMatches();
        }

        public Vector3 GridToWorld(Vector2Int pos) => new(origin.x + pos.x * cellSize, origin.y + pos.y * cellSize, -1f);

        public bool CanInteract => !_busy && !GameManager.Instance.IsGameOver;

        public MergeItem SpawnItemAt(int x, int y, ItemTier tier, int colorId, bool fromTop = false)
        {
            var go = new GameObject($"Item_{x}_{y}");
            var item = go.AddComponent<MergeItem>();
            item.Setup(colorId, tier, GetSprite(colorId, tier), new Vector2Int(x, y));
            Grid[x, y].Item = item;

            var col = go.GetComponent<Collider2D>();
            if (col is CircleCollider2D cc) cc.radius = 0.42f;

            var target = GridToWorld(new Vector2Int(x, y));
            if (fromTop)
            {
                go.transform.position = target + Vector3.up * (cellSize * 2f);
                item.MoveTo(target);
            }
            else
            {
                go.transform.position = target;
            }

            return item;
        }

        public Sprite GetSprite(int colorId, ItemTier tier)
        {
            Sprite[] arr = tier switch
            {
                ItemTier.Egg => eggSprites,
                ItemTier.CrackedEgg => crackedSprites,
                ItemTier.HatchingEgg => hatchingSprites,
                ItemTier.BabyDragon => babySprites,
                _ => eggSprites
            };

            if (arr != null && arr.Length > 0) return arr[Mathf.Abs(colorId) % arr.Length];
            return fallbackSprite;
        }

        public void TrySwap(MergeItem a, MergeItem b)
        {
            if (!CanInteract || a == null || b == null) return;
            if (Vector2Int.Distance(a.GridPos, b.GridPos) > 1.01f) return;
            StartCoroutine(SwapRoutine(a, b));
        }

        private IEnumerator SwapRoutine(MergeItem a, MergeItem b)
        {
            _busy = true;

            var pa = a.GridPos;
            var pb = b.GridPos;
            Grid[pa.x, pa.y].Item = b;
            Grid[pb.x, pb.y].Item = a;
            a.SetGridPos(pb);
            b.SetGridPos(pa);
            a.MoveTo(GridToWorld(pb));
            b.MoveTo(GridToWorld(pa));
            yield return new WaitForSeconds(0.14f);

            var groups = _matchDetector.FindMatches(Grid, width, height);
            if (groups.Count == 0)
            {
                Grid[pa.x, pa.y].Item = a;
                Grid[pb.x, pb.y].Item = b;
                a.SetGridPos(pa);
                b.SetGridPos(pb);
                a.MoveTo(GridToWorld(pa));
                b.MoveTo(GridToWorld(pb));
                GameManager.Instance.SpendMove();
                yield return new WaitForSeconds(0.14f);
                _busy = false;
                yield break;
            }

            GameManager.Instance.SpendMove();
            yield return _mergeController.ResolveMatches(this, groups, pb);

            while (true)
            {
                yield return _gravityController.Apply(this);
                var cascades = _matchDetector.FindMatches(Grid, width, height);
                if (cascades.Count == 0) break;
                yield return _mergeController.ResolveMatches(this, cascades, pb);
            }

            _busy = false;
        }

        private void BuildGrid()
        {
            Grid = new Tile[width, height];
            for (var x = 0; x < width; x++)
                for (var y = 0; y < height; y++)
                    Grid[x, y] = new Tile(x, y);
        }

        private void FillWithoutInitialMatches()
        {
            for (var x = 0; x < width; x++)
            {
                for (var y = 0; y < height; y++)
                {
                    var tries = 0;
                    int c;
                    do
                    {
                        c = Random.Range(0, ColorCount);
                        tries++;
                    }
                    while (CreatesInitialMatch(x, y, c) && tries < 12);

                    SpawnItemAt(x, y, ItemTier.Egg, c);
                }
            }
        }

        private bool CreatesInitialMatch(int x, int y, int colorId)
        {
            if (x >= 2)
            {
                var a = Grid[x - 1, y].Item;
                var b = Grid[x - 2, y].Item;
                if (a != null && b != null && a.ColorId == colorId && b.ColorId == colorId) return true;
            }
            if (y >= 2)
            {
                var a = Grid[x, y - 1].Item;
                var b = Grid[x, y - 2].Item;
                if (a != null && b != null && a.ColorId == colorId && b.ColorId == colorId) return true;
            }
            return false;
        }

        private void EnsureSprites()
        {
            if (fallbackSprite == null)
            {
                var tex = new Texture2D(1, 1);
                tex.SetPixel(0, 0, Color.white);
                tex.Apply();
                fallbackSprite = Sprite.Create(tex, new Rect(0, 0, 1, 1), new Vector2(0.5f, 0.5f), 1);
            }

#if UNITY_EDITOR
            if ((eggSprites == null || eggSprites.Length == 0))
            {
                var atlas = UnityEditor.AssetDatabase.LoadAllAssetsAtPath("Assets/Sprites/UI/Board_Items_Atlas.png");
                var sprites = new List<Sprite>();
                foreach (var a in atlas)
                {
                    if (a is Sprite s) sprites.Add(s);
                }

                if (sprites.Count >= 16)
                {
                    eggSprites = sprites.GetRange(0, 4).ToArray();
                    crackedSprites = sprites.GetRange(4, 4).ToArray();
                    hatchingSprites = sprites.GetRange(8, 4).ToArray();
                    babySprites = sprites.GetRange(12, 4).ToArray();
                }
            }
#endif
        }
    }
}
