using MergeEvolution.Board;
using MergeEvolution.Items;
using UnityEngine;

namespace MergeEvolution.Input
{
    public class InputManager : MonoBehaviour
    {
        private BoardManager _board;
        private MergeItem _startItem;
        private Vector2 _startPos;

        private void Start()
        {
            _board = FindFirstObjectByType<BoardManager>();
        }

        private void Update()
        {
            if (_board == null || !_board.CanInteract) return;

            if (UnityEngine.Input.GetMouseButtonDown(0))
            {
                _startPos = Camera.main.ScreenToWorldPoint(UnityEngine.Input.mousePosition);
                _startItem = Pick(_startPos);
            }
            else if (UnityEngine.Input.GetMouseButtonUp(0) && _startItem != null)
            {
                var end = (Vector2)Camera.main.ScreenToWorldPoint(UnityEngine.Input.mousePosition);
                var dir = end - _startPos;
                if (dir.magnitude < 0.2f)
                {
                    _startItem = null;
                    return;
                }

                var swipe = Mathf.Abs(dir.x) > Mathf.Abs(dir.y)
                    ? new Vector2Int(dir.x > 0 ? 1 : -1, 0)
                    : new Vector2Int(0, dir.y > 0 ? 1 : -1);

                var targetPos = _startItem.GridPos + swipe;
                if (targetPos.x >= 0 && targetPos.x < _board.Width && targetPos.y >= 0 && targetPos.y < _board.Height)
                {
                    var target = _board.Grid[targetPos.x, targetPos.y].Item;
                    _board.TrySwap(_startItem, target);
                }

                _startItem = null;
            }
        }

        private static MergeItem Pick(Vector2 worldPos)
        {
            var hit = Physics2D.OverlapPoint(worldPos);
            return hit != null ? hit.GetComponent<MergeItem>() : null;
        }
    }
}
