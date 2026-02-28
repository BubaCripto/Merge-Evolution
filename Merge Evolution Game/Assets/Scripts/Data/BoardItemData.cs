using UnityEngine;

namespace MergeEvolution.Data
{
    public enum ItemType
    {
        Creature,   // Dragões e Ovos (Se movem e evoluem)
        Coin,       // Dinheiro (Quando junta 3, dá moedas)
        Chest,      // Baú / Presente (Quando junta 3, ou clica, explode e dá loot)
        Weapon,     // Bazuca, Espada (Itens mágicos que dão dano)
        Obstacle,   // Pedras, Caixote com 'X' (Ocupa espaço, precisa ser explodido)
        Consumable  // Energia (Lâmpada), Interrogação, etc.
    }

    [CreateAssetMenu(fileName = "New Board Item", menuName = "Merge Evolution/Board Item Data")]
    public class BoardItemData : ScriptableObject
    {
        [Header("Informações do Item")]
        public string itemName;
        public Sprite itemSprite;
        public ItemType itemType;
        public int level = 1;

        [Header("Evolução (Merge)")]
        [Tooltip("Para qual item ele evolui ao fazer Merge?")]
        public BoardItemData nextEvolution;
        
        [Header("Economia e Uso")]
        [Tooltip("Quantas moedas ou pontos ele dá ao ser consumido/combinado?")]
        public int value;
    }
}
