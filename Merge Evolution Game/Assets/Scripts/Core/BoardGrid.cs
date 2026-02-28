using UnityEngine;
using MergeEvolution.Data;
using System.Collections.Generic;

namespace MergeEvolution.Core
{
    public class BoardGrid : MonoBehaviour
    {
        [Header("Configurações Básicas")]
        public int width = 5;
        public int height = 7;

        [Header("Perspectiva Trapezoidal (Crie 4 Empties na Cena)")]
        [Tooltip("Arraste 4 GameObjects que representem visualmente os 4 cantos do seu desenho de tabuleiro!")]
        public Transform bottomLeftCorner;
        public Transform bottomRightCorner;
        public Transform topLeftCorner;
        public Transform topRightCorner;

        [Header("Profundidade / Tamanho Virtual")]
        public float scaleAtBottom = 1.0f;
        public float scaleAtTop = 0.5f;

        [Header("Referências")]
        public GameObject cellPrefab;

        private Cell[,] gridCells;

        public static BoardGrid Instance;

        private void Awake()
        {
            Instance = this;
        }

        private void Start()
        {
            GeneratePerspectiveGrid();
        }

        private void GeneratePerspectiveGrid()
        {
            // Se o usuário esqueceu de criar os cantos, travamos para evitar erro
            if (bottomLeftCorner == null || topRightCorner == null)
            {
                Debug.LogWarning("Opa! Você esqueceu de definir os 4 cantos da Perspectiva no BoardGrid!");
                return;
            }

            gridCells = new Cell[width, height];

            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    // Descobrimos a "porcentagem" de onde estamos no tabuleiro (0.0 a 1.0)
                    float percentX = width > 1 ? (float)x / (width - 1) : 0.5f;
                    float percentY = height > 1 ? (float)y / (height - 1) : 0.5f;

                    // Extrapola as pontas diagonais para achar a LINHA exata
                    Vector3 leftEdge = Vector3.Lerp(bottomLeftCorner.position, topLeftCorner.position, percentY);
                    Vector3 rightEdge = Vector3.Lerp(bottomRightCorner.position, topRightCorner.position, percentY);

                    // Desliza do lado esquerdo para o lado direito para achar O PONTO exato (Matemática Mágica!)
                    Vector3 finalPositionInPerspective = Vector3.Lerp(leftEdge, rightEdge, percentX);

                    GameObject cellObj = Instantiate(cellPrefab, finalPositionInPerspective, Quaternion.identity, this.transform);
                    cellObj.name = $"Cell_X{x}_Y{y}";

                    // Ilusão de que lá atrás os quadradinhos são menores
                    float currentDepthScale = Mathf.Lerp(scaleAtBottom, scaleAtTop, percentY);
                    cellObj.transform.localScale = new Vector3(currentDepthScale, currentDepthScale, 1f);

                    Cell newCell = cellObj.GetComponent<Cell>();
                    newCell.gridPosition = new Vector2Int(x, y);

                    gridCells[x, y] = newCell;
                }
            }
        }

        public Cell GetCell(int x, int y)
        {
            if (x >= 0 && x < width && y >= 0 && y < height)
            {
                return gridCells[x, y];
            }
            return null;
        }

        // ==========================================
        //         MECÂNICA DE COMBINAR 3 (MATCH)
        // ==========================================
        public void CheckForMerge(Cell targetCell, Piece droppedPiece)
        {
            List<Cell> matches = new List<Cell>();
            HashSet<Cell> visited = new HashSet<Cell>();

            // Espalha um "vírus" matemático achando todos os vizinhos iguais!
            FloodFillFindMatches(targetCell, droppedPiece.data, matches, visited);

            // ACHOU 3 OU MAIS PEÇAS IGUAIS E GRUDADAS? BINGO!
            if (matches.Count >= 3)
            {
                if (droppedPiece.data != null && droppedPiece.data.nextEvolution != null)
                {
                    // O jogador fez a fusão. 
                    // Apagamos O RESTO (menos a peça na sua mão que puxou tudo)
                    foreach(Cell c in matches)
                    {
                        if (c != targetCell && c.currentPiece != null)
                        {
                            Destroy(c.currentPiece);
                            c.ClearCell();
                        }
                    }

                    // A peça que você está segurando BRILHA e EVOLUI magicamente!
                    droppedPiece.Setup(droppedPiece.data.nextEvolution);

                    // (Futuramente colocaremos aqui: TocarSomDeExplosao(); InstanciarPoeiraMagica();)
                    Debug.Log($"<color=green>🌟 SUPER COMBINAÇÃO DE {matches.Count} PEÇAS!</color>");
                }
            }
        }

        private void FloodFillFindMatches(Cell cell, BoardItemData dataToMatch, List<Cell> matches, HashSet<Cell> visited)
        {
            if (cell == null || visited.Contains(cell) || !cell.isOccupied) return;
            
            Piece piece = cell.currentPiece.GetComponent<Piece>();
            if (piece == null || piece.data != dataToMatch) return;

            // Marca que foi testado e adiciona na roda de amigos
            visited.Add(cell);
            matches.Add(cell);

            // Pula para cima, baixo, esquerda e direita contagiando
            FloodFillFindMatches(GetCell(cell.gridPosition.x + 1, cell.gridPosition.y), dataToMatch, matches, visited);
            FloodFillFindMatches(GetCell(cell.gridPosition.x - 1, cell.gridPosition.y), dataToMatch, matches, visited);
            FloodFillFindMatches(GetCell(cell.gridPosition.x, cell.gridPosition.y + 1), dataToMatch, matches, visited);
            FloodFillFindMatches(GetCell(cell.gridPosition.x, cell.gridPosition.y - 1), dataToMatch, matches, visited);
        }
    }
}
