using UnityEngine;
using System.Collections;
using BattleARena.UI;
using BattleARena.AI;

namespace BattleARena.Battle
{
    /// <summary>
    /// Gerencia toda a lógica de batalha: HP, ataques, turnos e condição de vitória.
    /// Deve existir como um único objeto na cena (Singleton).
    /// </summary>
    public class BattleManager : MonoBehaviour
    {
        public static BattleManager Instance { get; private set; }

        [Header("Dados dos Pokémon")]
        public PokemonData playerPokemonData;
        public PokemonData enemyPokemonData;

        [Header("Referências de UI")]
        public BattleHUD battleHUD;

        [Header("Referências de IA")]
        public AIController aiController;

        // HP atual
        private int playerCurrentHP;
        private int enemyCurrentHP;

        // Controle de turno
        private bool isPlayerTurn = true;
        private bool isBattleOver = false;

        // -------------------------------------------------------
        // Inicialização
        // -------------------------------------------------------

        void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
        }

        void Start()
        {
            InitializeBattle();
        }

        private void InitializeBattle()
        {
            playerCurrentHP = playerPokemonData.baseHP;
            enemyCurrentHP  = enemyPokemonData.baseHP;
            isBattleOver    = false;
            isPlayerTurn    = true;

            battleHUD.Initialize(
                playerPokemonData.pokemonName, playerPokemonData.baseHP,
                enemyPokemonData.pokemonName,  enemyPokemonData.baseHP
            );

            battleHUD.UpdateHP(playerCurrentHP, enemyCurrentHP);
            battleHUD.SetButtonsInteractable(true);
            battleHUD.ShowMessage($"Batalha iniciada! {playerPokemonData.pokemonName} VS {enemyPokemonData.pokemonName}");

            Debug.Log("[BattleManager] Batalha inicializada.");
        }

        // -------------------------------------------------------
        // Ações do Jogador
        // -------------------------------------------------------

        /// <summary>Chamado pelo botão "Ataque Rápido" na UI.</summary>
        public void PlayerQuickAttack()
        {
            if (!isPlayerTurn || isBattleOver) return;
            int damage = CalculateDamage(
                playerPokemonData.quickAttackMinDamage,
                playerPokemonData.quickAttackMaxDamage,
                playerPokemonData.criticalChance,
                playerPokemonData.criticalMultiplier,
                out bool isCrit
            );
            ApplyDamageToEnemy(damage, isCrit, "Ataque Rápido");
        }

        /// <summary>Chamado pelo botão "Ataque Forte" na UI.</summary>
        public void PlayerStrongAttack()
        {
            if (!isPlayerTurn || isBattleOver) return;
            int damage = CalculateDamage(
                playerPokemonData.strongAttackMinDamage,
                playerPokemonData.strongAttackMaxDamage,
                playerPokemonData.criticalChance,
                playerPokemonData.criticalMultiplier,
                out bool isCrit
            );
            ApplyDamageToEnemy(damage, isCrit, "Ataque Forte");
        }

        // -------------------------------------------------------
        // Aplicar Dano
        // -------------------------------------------------------

        private void ApplyDamageToEnemy(int damage, bool isCrit, string attackName)
        {
            enemyCurrentHP = Mathf.Max(0, enemyCurrentHP - damage);
            string critText = isCrit ? " <color=yellow>[CRÍTICO!]</color>" : "";
            battleHUD.ShowMessage($"{playerPokemonData.pokemonName} usou {attackName}! -{damage} HP{critText}");
            battleHUD.UpdateHP(playerCurrentHP, enemyCurrentHP);

            Debug.Log($"[BattleManager] Jogador atacou: {damage} de dano. HP inimigo: {enemyCurrentHP}");

            if (enemyCurrentHP <= 0)
            {
                EndBattle(playerWon: true);
                return;
            }

            // Passa o turno pra IA
            isPlayerTurn = false;
            battleHUD.SetButtonsInteractable(false);
            StartCoroutine(EnemyTurnCoroutine());
        }

        private void ApplyDamageToPlayer(int damage, bool isCrit, string attackName)
        {
            playerCurrentHP = Mathf.Max(0, playerCurrentHP - damage);
            string critText = isCrit ? " <color=yellow>[CRÍTICO!]</color>" : "";
            battleHUD.ShowMessage($"{enemyPokemonData.pokemonName} usou {attackName}! -{damage} HP{critText}");
            battleHUD.UpdateHP(playerCurrentHP, enemyCurrentHP);

            Debug.Log($"[BattleManager] IA atacou: {damage} de dano. HP jogador: {playerCurrentHP}");

            if (playerCurrentHP <= 0)
            {
                EndBattle(playerWon: false);
                return;
            }

            // Devolve o turno pro jogador
            isPlayerTurn = true;
            battleHUD.SetButtonsInteractable(true);
            battleHUD.ShowMessage($"Sua vez! {playerPokemonData.pokemonName} HP: {playerCurrentHP}");
        }

        // -------------------------------------------------------
        // Turno da IA
        // -------------------------------------------------------

        private IEnumerator EnemyTurnCoroutine()
        {
            battleHUD.ShowMessage($"{enemyPokemonData.pokemonName} está preparando ataque...");
            yield return new WaitForSeconds(enemyPokemonData.aiAttackDelay);

            if (isBattleOver) yield break;

            // IA escolhe ataque: 60% Rápido, 40% Forte
            bool useStrong = Random.value < 0.4f;
            int damage;
            bool isCrit;
            string attackName;

            if (useStrong)
            {
                damage = CalculateDamage(
                    enemyPokemonData.strongAttackMinDamage,
                    enemyPokemonData.strongAttackMaxDamage,
                    enemyPokemonData.criticalChance,
                    enemyPokemonData.criticalMultiplier,
                    out isCrit
                );
                attackName = "Ataque Forte";
            }
            else
            {
                damage = CalculateDamage(
                    enemyPokemonData.quickAttackMinDamage,
                    enemyPokemonData.quickAttackMaxDamage,
                    enemyPokemonData.criticalChance,
                    enemyPokemonData.criticalMultiplier,
                    out isCrit
                );
                attackName = "Ataque Rápido";
            }

            ApplyDamageToPlayer(damage, isCrit, attackName);
        }

        // -------------------------------------------------------
        // Cálculo de Dano
        // -------------------------------------------------------

        private int CalculateDamage(int minDmg, int maxDmg, float critChance, float critMult, out bool isCrit)
        {
            int baseDamage = Random.Range(minDmg, maxDmg + 1);
            isCrit = Random.value < critChance;
            if (isCrit)
                baseDamage = Mathf.RoundToInt(baseDamage * critMult);
            return baseDamage;
        }

        // -------------------------------------------------------
        // Fim de Batalha
        // -------------------------------------------------------

        private void EndBattle(bool playerWon)
        {
            isBattleOver = true;
            battleHUD.SetButtonsInteractable(false);

            if (playerWon)
            {
                battleHUD.ShowMessage($"🏆 {playerPokemonData.pokemonName} venceu!");
                battleHUD.ShowVictoryScreen(true);
                Debug.Log("[BattleManager] Jogador venceu!");
            }
            else
            {
                battleHUD.ShowMessage($"💀 {enemyPokemonData.pokemonName} venceu!");
                battleHUD.ShowVictoryScreen(false);
                Debug.Log("[BattleManager] IA venceu!");
            }
        }

        // -------------------------------------------------------
        // Reiniciar Batalha
        // -------------------------------------------------------

        public void RestartBattle()
        {
            InitializeBattle();
        }
    }
}
