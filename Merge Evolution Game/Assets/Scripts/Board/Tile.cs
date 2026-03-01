using MergeEvolution.Items;
using UnityEngine;

namespace MergeEvolution.Board
{
    public class Tile
    {
        public Vector2Int Pos { get; }
        public MergeItem Item;

        public Tile(int x, int y)
        {
            Pos = new Vector2Int(x, y);
        }

        public bool IsEmpty => Item == null;
    }
}
