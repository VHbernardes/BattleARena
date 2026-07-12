using UnityEngine;

namespace BattleARena.Battle
{
    /// <summary>
    /// Sistema elemental baseado na matriz de vantagens do grupo.
    /// Linha = atacante, Coluna = defensor
    /// 1f = Dano Normal, 2f = Dobro de Dano (Vantagem)
    /// </summary>
    public static class ElementalSystem
    {
        public enum PokemonType
        {
            Fogo     = 0,
            Planta   = 1,
            Agua     = 2,
            Eletrico = 3,
            Voador   = 4,
            Pedra    = 5
        }

        // Matriz de vantagens (Linha ataca Coluna)
        //                    Fogo  Planta Agua  Elet  Voad  Pedra
        private static readonly float[,] tabelaTipos = new float[,]
        {
            /* Fogo     */ { 1f,   2f,    1f,   1f,   1f,   1f },
            /* Planta   */ { 1f,   1f,    2f,   1f,   1f,   2f },
            /* Agua     */ { 2f,   1f,    1f,   1f,   1f,   1f },
            /* Eletrico */ { 1f,   1f,    2f,   1f,   2f,   1f },
            /* Voador   */ { 1f,   2f,    1f,   1f,   1f,   2f },
            /* Pedra    */ { 2f,   1f,    1f,   2f,   1f,   1f }
        };

        /// <summary>
        /// Retorna o multiplicador de dano baseado nos tipos do atacante e defensor.
        /// </summary>
        public static float GetMultiplier(PokemonType attacker, PokemonType defender)
        {
            return tabelaTipos[(int)attacker, (int)defender];
        }

        /// <summary>
        /// Retorna uma string descrevendo a vantagem elemental para exibir na HUD.
        /// </summary>
        public static string GetAdvantageText(PokemonType attacker, PokemonType defender)
        {
            float mult = GetMultiplier(attacker, defender);
            if (mult >= 2f) return "Vantagem elemental! x2!";
            if (mult <= 0.5f) return "Desvantagem elemental...";
            return "";
        }
    }
}