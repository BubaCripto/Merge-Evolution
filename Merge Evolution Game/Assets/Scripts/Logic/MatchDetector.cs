using System.Collections.Generic;
using MergeEvolution.Board;
using MergeEvolution.Items;
using UnityEngine;

namespace MergeEvolution.Logic
{
    public class MatchDetector : MonoBehaviour
    {
        public List<List<Tile>> FindMatches(Tile[,] grid, int width, int height)
        {
            var result = new List<List<Tile>>();
            var seen = new HashSet<Tile>();

            for (var y = 0; y < height; y++)
            {
                var x = 0;
                while (x < width)
                {
                    var run = GetRun(grid, width, x, y, Vector2Int.right);
                    if (run.Count >= 3) AddGroup(run, result, seen);
                    x += Mathf.Max(1, run.Count);
                }
            }

            for (var x = 0; x < width; x++)
            {
                var y = 0;
                while (y < height)
                {
                    var run = GetRun(grid, height, x, y, Vector2Int.up);
                    if (run.Count >= 3) AddGroup(run, result, seen);
                    y += Mathf.Max(1, run.Count);
                }
            }

            return result;
        }

        private static List<Tile> GetRun(Tile[,] grid, int max, int x, int y, Vector2Int dir)
        {
            var list = new List<Tile>();
            var start = grid[x, y].Item;
            if (start == null) return list;

            var cx = x;
            var cy = y;
            while (cx >= 0 && cy >= 0 && cx < grid.GetLength(0) && cy < grid.GetLength(1))
            {
                var tile = grid[cx, cy];
                if (tile.Item == null) break;
                if (tile.Item.ColorId != start.ColorId || tile.Item.Tier != start.Tier) break;
                list.Add(tile);
                cx += dir.x;
                cy += dir.y;
            }
            return list;
        }

        private static void AddGroup(List<Tile> run, List<List<Tile>> groups, HashSet<Tile> seen)
        {
            var group = new List<Tile>();
            foreach (var t in run)
            {
                if (seen.Add(t)) group.Add(t);
            }
            if (group.Count > 0) groups.Add(group);
        }
    }
}
