using UnityEngine;

namespace MergeEvolution.Core
{
    public class Cell : MonoBehaviour
    {
        public Vector2Int gridPosition;
        public Piece currentPiece;

        public bool IsOccupied => currentPiece != null;

        public void SetPiece(Piece piece)
        {
            currentPiece = piece;
            if (piece == null) return;

            piece.currentCell = this;
            piece.transform.SetParent(transform);
            piece.transform.position = transform.position;
        }

        public void ClearPiece()
        {
            currentPiece = null;
        }
    }
}
