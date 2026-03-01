using System.Collections;
using UnityEngine;

namespace MergeEvolution.Items
{
    public enum ItemTier
    {
        Egg = 0,
        CrackedEgg = 1,
        HatchingEgg = 2,
        BabyDragon = 3
    }

    [RequireComponent(typeof(SpriteRenderer), typeof(Collider2D))]
    public class MergeItem : MonoBehaviour
    {
        public int ColorId { get; private set; }
        public ItemTier Tier { get; private set; }
        public Vector2Int GridPos { get; private set; }

        private SpriteRenderer _sr;
        private Coroutine _moveRoutine;

        private void Awake()
        {
            _sr = GetComponent<SpriteRenderer>();
        }

        public void Setup(int colorId, ItemTier tier, Sprite sprite, Vector2Int gridPos)
        {
            ColorId = colorId;
            Tier = tier;
            GridPos = gridPos;
            _sr.sprite = sprite;
            name = $"Item_{tier}_C{colorId}_{gridPos.x}_{gridPos.y}";
        }

        public void SetGridPos(Vector2Int pos) => GridPos = pos;

        public void Evolve(Sprite nextSprite)
        {
            Tier = (ItemTier)Mathf.Min((int)Tier + 1, (int)ItemTier.BabyDragon);
            _sr.sprite = nextSprite;
        }

        public void SetSprite(Sprite sprite) => _sr.sprite = sprite;

        public void MoveTo(Vector3 target, float duration = 0.12f)
        {
            if (_moveRoutine != null) StopCoroutine(_moveRoutine);
            _moveRoutine = StartCoroutine(MoveRoutine(target, duration));
        }

        private IEnumerator MoveRoutine(Vector3 target, float duration)
        {
            var start = transform.position;
            var t = 0f;
            while (t < duration)
            {
                t += Time.deltaTime;
                transform.position = Vector3.Lerp(start, target, t / duration);
                yield return null;
            }
            transform.position = target;
            _moveRoutine = null;
        }
    }
}
