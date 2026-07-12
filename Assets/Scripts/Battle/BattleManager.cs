using UnityEngine;
using System.Collections;
using BattleARena.UI;
using BattleARena.Animation;

namespace BattleARena.Battle
{
    public class BattleManager : MonoBehaviour
    {
        public static BattleManager Instance { get; private set; }

        [Header("Referências de UI")]
        public BattleHUD battleHUD;

        [Header("Referências de Animação")]
        [Tooltip("PokemonAnimator do modelo do jogador")]
        public PokemonAnimator playerAnimator;
        [Tooltip("PokemonAnimator do modelo do inimigo")]
        public PokemonAnimator enemyAnimator;

        private PokemonData playerPokemonData;
        private PokemonData enemyPokemonData;

        private int playerCurrentHP;
        private int enemyCurrentHP;

        private bool isPlayerTurn   = true;
        private bool isBattleOver   = false;
        private bool isBattleRunning = false;

        void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this;
        }

        // -------------------------------------------------------
        // Setup Dinâmico
        // -------------------------------------------------------

        public void SetPokemonData(PokemonData player, PokemonData enemy)
        {
            playerPokemonData = player;
            enemyPokemonData  = enemy;
        }

        public void StartBattle()
        {
            if (playerPokemonData == null || enemyPokemonData == null)
            {
                Debug.LogError("[BattleManager] PokemonData não definido!");
                return;
            }

            playerCurrentHP  = playerPokemonData.baseHP;
            enemyCurrentHP   = enemyPokemonData.baseHP;
            isBattleOver     = false;
            isPlayerTurn     = true;
            isBattleRunning  = true;

            battleHUD.Initialize(
                playerPokemonData.pokemonName, playerPokemonData.baseHP,
                enemyPokemonData.pokemonName,  enemyPokemonData.baseHP
            );

            battleHUD.UpdateHP(playerCurrentHP, enemyCurrentHP);
            battleHUD.SetButtonsInteractable(true);
            battleHUD.ShowMessage($"Batalha! {playerPokemonData.pokemonName} VS {enemyPokemonData.pokemonName}");
        }

        // -------------------------------------------------------
        // Ações do Jogador
        // -------------------------------------------------------

        public void PlayerQuickAttack()
        {
            if (!isPlayerTurn || isBattleOver || !isBattleRunning) return;
            int damage = CalculateDamage(
                playerPokemonData.quickAttackMinDamage,
                playerPokemonData.quickAttackMaxDamage,
                playerPokemonData.criticalChance,
                playerPokemonData.criticalMultiplier,
                out bool isCrit
            );
            StartCoroutine(PlayerAttackCoroutine(damage, isCrit, "Ataque Rapido"));
        }

        public void PlayerStrongAttack()
        {
            if (!isPlayerTurn || isBattleOver || !isBattleRunning) return;
            int damage = CalculateDamage(
                playerPokemonData.strongAttackMinDamage,
                playerPokemonData.strongAttackMaxDamage,
                playerPokemonData.criticalChance,
                playerPokemonData.criticalMultiplier,
                out bool isCrit
            );
            StartCoroutine(PlayerAttackCoroutine(damage, isCrit, "Ataque Forte"));
        }

        // -------------------------------------------------------
        // Coroutines de Ataque com Animação
        // -------------------------------------------------------

        private IEnumerator PlayerAttackCoroutine(int damage, bool isCrit, string attackName)
        {
            battleHUD.SetButtonsInteractable(false);

            // Animação de ataque do jogador
            if (playerAnimator != null)
                playerAnimator.PlayAttack();

            // Espera metade da animação de ataque antes de aplicar dano
            yield return new WaitForSeconds(0.25f);

            // Animação de hit no inimigo
            if (enemyAnimator != null)
                enemyAnimator.PlayHit();

            // Aplica dano
            enemyCurrentHP = Mathf.Max(0, enemyCurrentHP - damage);
            string critText = isCrit ? " [CRITICO!]" : "";
            battleHUD.ShowMessage($"{playerPokemonData.pokemonName} usou {attackName}! -{damage} HP{critText}");
            battleHUD.UpdateHP(playerCurrentHP, enemyCurrentHP);

            // Espera animação terminar
            yield return new WaitForSeconds(0.3f);

            if (enemyCurrentHP <= 0) { EndBattle(playerWon: true); yield break; }

            // Turno da IA
            isPlayerTurn = false;
            StartCoroutine(EnemyTurnCoroutine());
        }

        private IEnumerator EnemyTurnCoroutine()
        {
            battleHUD.ShowMessage($"{enemyPokemonData.pokemonName} esta preparando ataque...");
            yield return new WaitForSeconds(enemyPokemonData.aiAttackDelay);
            if (isBattleOver) yield break;

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
                attackName = "Ataque Rapido";
            }

            // Animação de ataque do inimigo
            if (enemyAnimator != null)
                enemyAnimator.PlayAttack();

            yield return new WaitForSeconds(0.25f);

            // Animação de hit no jogador
            if (playerAnimator != null)
                playerAnimator.PlayHit();

            // Aplica dano
            playerCurrentHP = Mathf.Max(0, playerCurrentHP - damage);
            string critText = isCrit ? " [CRITICO!]" : "";
            battleHUD.ShowMessage($"{enemyPokemonData.pokemonName} usou {attackName}! -{damage} HP{critText}");
            battleHUD.UpdateHP(playerCurrentHP, enemyCurrentHP);

            yield return new WaitForSeconds(0.3f);

            if (playerCurrentHP <= 0) { EndBattle(playerWon: false); yield break; }

            isPlayerTurn = true;
            battleHUD.SetButtonsInteractable(true);
            battleHUD.ShowMessage($"Sua vez! {playerPokemonData.pokemonName} HP: {playerCurrentHP}");
        }

        // -------------------------------------------------------
        // Cálculo de Dano
        // -------------------------------------------------------

        private int CalculateDamage(int minDmg, int maxDmg, float critChance, float critMult, out bool isCrit)
        {
            int baseDamage = Random.Range(minDmg, maxDmg + 1);
            isCrit = Random.value < critChance;
            if (isCrit) baseDamage = Mathf.RoundToInt(baseDamage * critMult);
            return baseDamage;
        }

        // -------------------------------------------------------
        // Fim de Batalha
        // -------------------------------------------------------

        private void EndBattle(bool playerWon)
        {
            isBattleOver    = true;
            isBattleRunning = false;
            battleHUD.SetButtonsInteractable(false);
            battleHUD.ShowVictoryScreen(playerWon);
            battleHUD.ShowMessage(playerWon
                ? $"VITORIA! {playerPokemonData.pokemonName} venceu!"
                : $"DERROTA! {enemyPokemonData.pokemonName} venceu!");
        }

        // -------------------------------------------------------
        // Reiniciar
        // -------------------------------------------------------

        public void RestartBattle()
        {
            isBattleRunning = false;
            AR.BattleSetupManager.Instance.ResetSetup();
        }
    }
}