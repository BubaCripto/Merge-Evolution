using System.Collections;
using MergeEvolution.Board;
using MergeEvolution.Items;
using UnityEngine;

namespace MergeEvolution.Logic
{
    public class GravityController : MonoBehaviour
    {
        public IEnumerator Apply(BoardManager board)
        {
            for (var x = 0; x < board.Width; x++)
            {
                var writeY = 0;
                for (var y = 0; y < board.Height; y++)
                {
                    var tile = board.Grid[x, y];
                    if (tile.Item == null) continue;

                    if (writeY != y)
                    {
                        var target = board.Grid[x, writeY];
                        target.Item = tile.Item;
                        target.Item.SetGridPos(target.Pos);
                        target.Item.MoveTo(board.GridToWorld(target.Pos));
                        tile.Item = null;
                    }
                    writeY++;
                }

                for (var y = writeY; y < board.Height; y++)
                {
                    board.SpawnItemAt(x, y, ItemTier.Egg, Random.Range(0, board.ColorCount), true);
                }
            }

            yield return new WaitForSeconds(0.18f);
        }
    }
}
