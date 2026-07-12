using UnityEngine;

namespace BattleARena.Battle
{
    [CreateAssetMenu(fileName = "PokemonData", menuName = "BattleARena/Pokemon Data")]
    public class PokemonData : ScriptableObject
    {
        [Header("Identificação")]
        public string pokemonName;
        public Sprite icon;

        [Header("Tipo Elemental")]
        public ElementalSystem.PokemonType tipo;

        [Header("Atributos de Batalha")]
        public int baseHP = 100;
        public int quickAttackMinDamage = 10;
        public int quickAttackMaxDamage = 14;
        public int strongAttackMinDamage = 28;
        public int strongAttackMaxDamage = 35;

        [Header("Configuração de IA")]
        public float aiAttackDelay = 2.0f;

        [Header("Crítico")]
        [Range(0f, 1f)]
        public float criticalChance = 0.1f;
        public float criticalMultiplier = 2f;
    }
}