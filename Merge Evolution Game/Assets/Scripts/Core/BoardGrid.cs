using System.Collections.Generic;
using UnityEngine;

namespace MergeEvolution.Core
{
    public class BoardGrid : MonoBehaviour
    {
        [Header("Grid")]
        public int width = 5;
        public int height = 5;
        public float cellSize = 1.2f;

        [Header("Visual")]
        public Color cellColorA = new(0.95f, 0.95f, 1f, 0.22f);
        public Color cellColorB = new(0.8f, 0.88f, 1f, 0.22f);

        private Cell[,] _cells;
        private readonly List<Cell> _allCells = new();

        public IReadOnlyList<Cell> AllCells => _allCells;

        private void Awake()
        {
            Generate();
        }

        public void Generate()
        {
            if (_cells != null && _cells.Length > 0) return;

            _cells = new Cell[width, height];

            var origin = transform.position - new Vector3((width - 1) * cellSize * 0.5f, (height - 1) * cellSize * 0.5f, 0f);

            for (var y = 0; y < height; y++)
            {
                for (var x = 0; x < width; x++)
                {
                    var cellGo = new GameObject($"Cell_{x}_{y}");
                    cellGo.transform.SetParent(transform);
                    cellGo.transform.position = origin + new Vector3(x * cellSize, y * cellSize, 0f);

                    var sr = cellGo.AddComponent<SpriteRenderer>();
                    sr.sprite = SpriteFactory.WhiteSquare;
                    sr.color = ((x + y) % 2 == 0) ? cellColorA : cellColorB;
                    sr.sortingOrder = 1;
                    cellGo.transform.localScale = new Vector3(cellSize * 0.95f, cellSize * 0.95f, 1f);

                    var col = cellGo.AddComponent<BoxCollider2D>();
                    col.isTrigger = true;

                    var cell = cellGo.AddComponent<Cell>();
                    cell.gridPosition = new Vector2Int(x, y);

                    _cells[x, y] = cell;
                    _allCells.Add(cell);
                }
            }
        }

        public Cell GetClosestCell(Vector3 worldPosition, float maxDistance = 0.8f)
        {
            Cell closest = null;
            var bestDist = maxDistance;

            foreach (var cell in _allCells)
            {
                var dist = Vector2.Distance(worldPosition, cell.transform.position);
                if (dist < bestDist)
                {
                    bestDist = dist;
                    closest = cell;
                }
            }

            return closest;
        }

        public List<Cell> GetEmptyCells()
        {
            var list = new List<Cell>();
            foreach (var cell in _allCells)
            {
                if (!cell.IsOccupied) list.Add(cell);
            }
            return list;
        }
    }
}
