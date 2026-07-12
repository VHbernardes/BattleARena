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

        // Animadores definidos dinamicamente pelo BattleSetupManager
        private PokemonAnimator playerAnimator;
        private PokemonAnimator enemyAnimator;

        private PokemonData playerPokemonData;
        private PokemonData enemyPokemonData;

        private int playerCurrentHP;
        private int enemyCurrentHP;

        private bool isPlayerTurn    = true;
        private bool isBattleOver    = false;
        private bool isBattleRunning = false;

        void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this;
        }

        public void SetPokemonData(PokemonData player, PokemonData enemy)
        {
            playerPokemonData = player;
            enemyPokemonData  = enemy;
        }

        /// <summary>Define os animadores dinamicamente — chamado pelo BattleSetupManager.</summary>
        public void SetAnimators(PokemonAnimator player, PokemonAnimator enemy)
        {
            playerAnimator = player;
            enemyAnimator  = enemy;
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

        private IEnumerator PlayerAttackCoroutine(int damage, bool isCrit, string attackName)
        {
            battleHUD.SetButtonsInteractable(false);

            if (playerAnimator != null) playerAnimator.PlayAttack();
            yield return new WaitForSeconds(0.25f);
            if (enemyAnimator != null) enemyAnimator.PlayHit();

            enemyCurrentHP = Mathf.Max(0, enemyCurrentHP - damage);
            string critText = isCrit ? " [CRITICO!]" : "";
            battleHUD.ShowMessage($"{playerPokemonData.pokemonName} usou {attackName}! -{damage} HP{critText}");
            battleHUD.UpdateHP(playerCurrentHP, enemyCurrentHP);

            yield return new WaitForSeconds(0.3f);

            if (enemyCurrentHP <= 0) { EndBattle(playerWon: true); yield break; }

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

            if (enemyAnimator != null) enemyAnimator.PlayAttack();
            yield return new WaitForSeconds(0.25f);
            if (playerAnimator != null) playerAnimator.PlayHit();

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

        private int CalculateDamage(int minDmg, int maxDmg, float critChance, float critMult, out bool isCrit)
        {
            int baseDamage = Random.Range(minDmg, maxDmg + 1);
            isCrit = Random.value < critChance;
            if (isCrit) baseDamage = Mathf.RoundToInt(baseDamage * critMult);
            return baseDamage;
        }

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

        public void RestartBattle()
        {
            isBattleRunning = false;
            AR.BattleSetupManager.Instance.ResetSetup();
        }
    }
}