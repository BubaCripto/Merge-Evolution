using System.Collections;
using System.Collections.Generic;
using MergeEvolution.Board;
using MergeEvolution.Items;
using UnityEngine;

namespace MergeEvolution.Logic
{
    public class MergeController : MonoBehaviour
    {
        public IEnumerator ResolveMatches(BoardManager board, List<List<Tile>> groups, Vector2Int preferredUpgradePos)
        {
            foreach (var group in groups)
            {
                if (group.Count < 3) continue;

                var upgradeTile = PickUpgradeTile(group, preferredUpgradePos);
                var item = upgradeTile.Item;
                if (item == null) continue;

                for (var i = 0; i < group.Count; i++)
                {
                    var t = group[i];
                    if (t == upgradeTile) continue;
                    if (t.Item != null)
                    {
                        Destroy(t.Item.gameObject);
                        t.Item = null;
                    }
                }

                if (item.Tier == ItemTier.HatchingEgg)
                {
                    Destroy(item.gameObject);
                    upgradeTile.Item = null;
                    GameManager.Instance.AddBabyToGallery();
                }
                else
                {
                    item.Evolve(board.GetSprite(item.ColorId, (ItemTier)((int)item.Tier + 1)));
                    GameManager.Instance.AddScore(50 * group.Count);
                }
            }

            yield return new WaitForSeconds(0.1f);
        }

        private static Tile PickUpgradeTile(List<Tile> group, Vector2Int preferred)
        {
            foreach (var t in group)
                if (t.Pos == preferred) return t;
            return group[0];
        }
    }
}
