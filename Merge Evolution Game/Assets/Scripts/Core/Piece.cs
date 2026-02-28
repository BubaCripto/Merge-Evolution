using UnityEngine;
using MergeEvolution.Data;

namespace MergeEvolution.Core
{
    public class Piece : MonoBehaviour
    {
        [Header("Dados do Item")]
        public BoardItemData data;
        
        [Header("Estado")]
        public Cell currentCell;

        private Vector3 offset;
        private SpriteRenderer spriteRenderer;
        
        [Header("Animação (Movimento Suave)")]
        private Vector3 targetPosition;
        private bool isSnapping = false;
        public float snapSpeed = 15f;

        private void Awake()
        {
            spriteRenderer = GetComponent<SpriteRenderer>();
            targetPosition = transform.position;
        }

        private void Update()
        {
            if (isSnapping)
            {
                // Movimentação interpolada mansa ("Smooth Slide")
                transform.position = Vector3.Lerp(transform.position, targetPosition, Time.deltaTime * snapSpeed);
                if (Vector3.Distance(transform.position, targetPosition) < 0.05f)
                {
                    transform.position = targetPosition;
                    isSnapping = false;
                }
            }
        }

        public void Setup(BoardItemData newData)
        {
            data = newData;
            if (spriteRenderer != null && data != null)
            {
                spriteRenderer.sprite = data.itemSprite;
            }
        }

        private void OnMouseDown()
        {
            isSnapping = false; // Interrompe a animação se o jogador botar o dedo na tela
            
            // Arranca da célula temporariamente para a escala do toque não bugar
            if (currentCell != null) { transform.SetParent(currentCell.transform.parent); }
            transform.localScale = Vector3.one; // Restaura o tamanho ao segurar

            offset = transform.position - Camera.main.ScreenToWorldPoint(Input.mousePosition);
            spriteRenderer.sortingOrder = 100; // Fica por cima de tudo
        }

        private void OnMouseDrag()
        {
            Vector3 newPosition = Camera.main.ScreenToWorldPoint(Input.mousePosition) + offset;
            newPosition.z = 0; 
            transform.position = newPosition;
        }

        private void OnMouseUp()
        {
            spriteRenderer.sortingOrder = 10; 
            Cell closestCell = FindClosestCell();

            if (closestCell != null)
            {
                // Tem alguém na célula?
                if (closestCell.isOccupied && closestCell != currentCell)
                {
                    Piece targetPiece = closestCell.currentPiece.GetComponent<Piece>();
                    
                    // SÃO O MESMO ITEM? (Ex: Dois Ovos Amarelos) -> Tenta fazer o Merge!
                    if (targetPiece != null && targetPiece.data == this.data)
                    {
                        MoveToCell(closestCell, true); // O 'true' avisa pra checar o Merge de 3!
                    }
                    else
                    {
                        // ITENS DIFERENTES? Troca de lugar um com o outro (Swap)!
                        Cell myOldCell = currentCell;
                        targetPiece.MoveToCell(myOldCell, false);
                        MoveToCell(closestCell, false);
                    }
                }
                else
                {
                    // Célula vazia? Só vai.
                    MoveToCell(closestCell, false);
                }
            }
            else
            {
                // Jogou fora do tabuleiro. Retorna!
                ReturnToCurrentCell();
            }
        }

        private Cell FindClosestCell()
        {
            Cell closest = null;
            float minDistance = 2.0f; 
            Cell[] allCells = FindObjectsOfType<Cell>();

            foreach (Cell cell in allCells)
            {
                // Como a perspectiva bagunça o grid original, checamos distância visual 2D na tela
                float dist = Vector2.Distance(transform.position, cell.transform.position);
                if (dist < minDistance)
                {
                    minDistance = dist;
                    closest = cell;
                }
            }

            return closest;
        }

        public void MoveToCell(Cell newCell, bool checkIfMerge)
        {
            if (currentCell != null)
            {
                currentCell.ClearCell();
            }

            currentCell = newCell;
            currentCell.SetPiece(this.gameObject, this.data);
            
            targetPosition = currentCell.transform.position;
            transform.localScale = Vector3.one; // Como é filho, herda a distorção da célula.
            isSnapping = true;

            // Chama o Gerentão para verificar o combo poderoso de 3 ou mais
            if (checkIfMerge && BoardGrid.Instance != null)
            {
                BoardGrid.Instance.CheckForMerge(currentCell, this);
            }
        }

        private void ReturnToCurrentCell()
        {
            if (currentCell != null)
            {
                targetPosition = currentCell.transform.position;
                currentCell.SetPiece(this.gameObject, this.data); // Religa o parentesco
                transform.localScale = Vector3.one;
                isSnapping = true;
            }
        }
    }
}
