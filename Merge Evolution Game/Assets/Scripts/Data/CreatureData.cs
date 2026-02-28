using UnityEngine;

namespace MergeEvolution.Data
{
    /// <summary>
    /// ScriptableObject que define os atributos base de cada criatura no jogo.
    /// Isso segue a abordagem Data-Driven recomendada para jogos Mobile/Merge.
    /// </summary>
    [CreateAssetMenu(fileName = "NewCreatureData", menuName = "Merge Evolution/Creature Data")]
    public class CreatureData : ScriptableObject
    {
        [Header("Identificação")]
        public string creatureName;
        public int level; // Nível da criatura no mural de evolução (N1, N2, N3...)

        [Header("Visual")]
        public Sprite creatureSprite; // A imagem tirada da pasta Dragon Sprint

        [Header("Atributos de Pontuação")]
        public int baseScore; // Pontos base calculados (valor da criatura x nível²)

        [Header("Evolução")]
        public CreatureData nextEvolution; // Para o que esta criatura evolui após o merge

        /// <summary>
        /// Calcula os pontos recebidos ao fazer o merge, considerando o sistema de combos.
        /// FÓRMULA SCORE: (valor base) x (nível²) x (combo)
        /// </summary>
        public int CalculateMergeScore(int comboMultiplier)
        {
            return baseScore * (level * level) * comboMultiplier;
        }
    }
}
