using UnityEngine;

namespace BattleARena.Battle
{
    /// <summary>
    /// ScriptableObject que define os atributos base de cada Pokémon.
    /// Crie um asset para cada Pokémon em Assets/Data/Pokemon/
    /// </summary>
    [CreateAssetMenu(fileName = "PokemonData", menuName = "BattleARena/Pokemon Data")]
    public class PokemonData : ScriptableObject
    {
        [Header("Identificação")]
        public string pokemonName;
        public Sprite icon;

        [Header("Atributos de Batalha")]
        public int baseHP = 100;
        public int quickAttackMinDamage = 10;
        public int quickAttackMaxDamage = 14;
        public int strongAttackMinDamage = 28;
        public int strongAttackMaxDamage = 35;

        [Header("Configuração de IA")]
        [Tooltip("Delay em segundos para a IA atacar")]
        public float aiAttackDelay = 2.0f;

        [Header("Crítico")]
        [Range(0f, 1f)]
        [Tooltip("Chance de crítico (0 a 1). Ex: 0.1 = 10%")]
        public float criticalChance = 0.1f;
        [Tooltip("Multiplicador de dano no crítico")]
        public float criticalMultiplier = 2f;
    }
}
