using MergeEvolution.Data;
using UnityEngine;

namespace MergeEvolution.Core
{
    public class Cell : MonoBehaviour
    {
        public Vector2Int gridPosition;
        
        [Header("Estado")]
        public bool isOccupied;
        
        public GameObject currentPiece; 
        public BoardItemData currentItemData;

        public void SetPiece(GameObject newPiece, BoardItemData data)
        {
            currentPiece = newPiece;
            currentItemData = data;
            isOccupied = true;
            
            // Ao entra na célula, vira "filho" dela para herdar o tamanho (perspectiva)
            newPiece.transform.SetParent(this.transform);
        }

        public void ClearCell()
        {
            currentPiece = null;
            currentItemData = null;
            isOccupied = false;
        }
    }
}
