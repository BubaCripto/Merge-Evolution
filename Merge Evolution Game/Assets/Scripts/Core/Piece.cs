using UnityEngine;

namespace MergeEvolution.Core
{
    [RequireComponent(typeof(SpriteRenderer), typeof(BoxCollider2D))]
    public class Piece : MonoBehaviour
    {
        public int level = 1;
        public Cell currentCell;
        public Cell dragStartCell;

        private SpriteRenderer _sr;
        private Vector3 _offset;

        private static readonly Color[] LevelColors =
        {
            new(0.69f, 0.87f, 1f),
            new(0.58f, 1f, 0.73f),
            new(1f, 0.94f, 0.53f),
            new(1f, 0.75f, 0.49f),
            new(1f, 0.58f, 0.74f),
            new(0.85f, 0.66f, 1f),
            new(1f, 0.42f, 0.42f)
        };

        private void Awake()
        {
            _sr = GetComponent<SpriteRenderer>();
            _sr.sprite = SpriteFactory.WhiteSquare;
            _sr.sortingOrder = 10;

            var collider2D = GetComponent<BoxCollider2D>();
            collider2D.size = Vector2.one;

            ApplyVisual();
        }

        public void SetLevel(int newLevel)
        {
            level = Mathf.Clamp(newLevel, 1, 7);
            ApplyVisual();
        }

        private void ApplyVisual()
        {
            if (_sr == null) return;

            _sr.color = LevelColors[Mathf.Clamp(level - 1, 0, LevelColors.Length - 1)];
            var extraScale = 1f + (level - 1) * 0.05f;
            transform.localScale = new Vector3(extraScale, extraScale, 1f);
        }

        private void OnMouseDown()
        {
            if (MergeGameManager.Instance == null || !MergeGameManager.Instance.CanInteract) return;

            dragStartCell = currentCell;
            if (dragStartCell != null) dragStartCell.ClearPiece();

            var mouse = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            mouse.z = 0f;
            _offset = transform.position - mouse;

            _sr.sortingOrder = 100;
        }

        private void OnMouseDrag()
        {
            if (dragStartCell == null) return;

            var mouse = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            mouse.z = 0f;
            transform.position = mouse + _offset;
        }

        private void OnMouseUp()
        {
            _sr.sortingOrder = 10;

            if (dragStartCell == null) return;
            MergeGameManager.Instance?.HandleDrop(this);
        }

        public void SnapToCell(Cell cell)
        {
            currentCell = cell;
            transform.position = cell.transform.position;
            transform.SetParent(cell.transform);
        }

        public void ReturnToStartCell()
        {
            if (dragStartCell == null) return;
            dragStartCell.SetPiece(this);
            dragStartCell = null;
        }
    }
}
