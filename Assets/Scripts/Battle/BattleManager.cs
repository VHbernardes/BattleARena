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

            // Mostra vantagem elemental inicial
            float mult = ElementalSystem.GetMultiplier(playerPokemonData.tipo, enemyPokemonData.tipo);
            string elementalHint = mult >= 2f
                ? $"Voce tem vantagem elemental contra {enemyPokemonData.pokemonName}!"
                : mult <= 0.5f
                    ? $"Cuidado! {enemyPokemonData.pokemonName} tem vantagem elemental!"
                    : $"Batalha! {playerPokemonData.pokemonName} VS {enemyPokemonData.pokemonName}";

            battleHUD.Initialize(
                playerPokemonData.pokemonName, playerPokemonData.baseHP,
                enemyPokemonData.pokemonName,  enemyPokemonData.baseHP
            );

            battleHUD.UpdateHP(playerCurrentHP, enemyCurrentHP);
            battleHUD.SetButtonsInteractable(true);
            battleHUD.ShowMessage(elementalHint);
        }

        // -------------------------------------------------------
        // Ações do Jogador
        // -------------------------------------------------------

        public void PlayerQuickAttack()
        {
            if (!isPlayerTurn || isBattleOver || !isBattleRunning) return;
            int damage = CalculateDamageWithElemental(
                playerPokemonData.quickAttackMinDamage,
                playerPokemonData.quickAttackMaxDamage,
                playerPokemonData.tipo,
                enemyPokemonData.tipo,
                playerPokemonData.criticalChance,
                playerPokemonData.criticalMultiplier,
                out bool isCrit,
                out float elementalMult
            );
            StartCoroutine(PlayerAttackCoroutine(damage, isCrit, elementalMult, "Ataque Rapido"));
        }

        public void PlayerStrongAttack()
        {
            if (!isPlayerTurn || isBattleOver || !isBattleRunning) return;
            int damage = CalculateDamageWithElemental(
                playerPokemonData.strongAttackMinDamage,
                playerPokemonData.strongAttackMaxDamage,
                playerPokemonData.tipo,
                enemyPokemonData.tipo,
                playerPokemonData.criticalChance,
                playerPokemonData.criticalMultiplier,
                out bool isCrit,
                out float elementalMult
            );
            StartCoroutine(PlayerAttackCoroutine(damage, isCrit, elementalMult, "Ataque Forte"));
        }

        // -------------------------------------------------------
        // Coroutines de Ataque
        // -------------------------------------------------------

        private IEnumerator PlayerAttackCoroutine(int damage, bool isCrit, float elementalMult, string attackName)
        {
            battleHUD.SetButtonsInteractable(false);

            if (playerAnimator != null) playerAnimator.PlayAttack();
            yield return new WaitForSeconds(0.25f);
            if (enemyAnimator != null) enemyAnimator.PlayHit();

            enemyCurrentHP = Mathf.Max(0, enemyCurrentHP - damage);

            string msg = BuildAttackMessage(playerPokemonData.pokemonName, attackName, damage, isCrit, elementalMult);
            battleHUD.ShowMessage(msg);
            battleHUD.UpdateHP(playerCurrentHP, enemyCurrentHP);

            yield return new WaitForSeconds(0.3f);

            if (enemyCurrentHP <= 0)
            {
                StartCoroutine(EndBattleCoroutine(playerWon: true));
                yield break;
            }

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
            float elementalMult;
            string attackName;

            if (useStrong)
            {
                damage = CalculateDamageWithElemental(
                    enemyPokemonData.strongAttackMinDamage,
                    enemyPokemonData.strongAttackMaxDamage,
                    enemyPokemonData.tipo,
                    playerPokemonData.tipo,
                    enemyPokemonData.criticalChance,
                    enemyPokemonData.criticalMultiplier,
                    out isCrit, out elementalMult
                );
                attackName = "Ataque Forte";
            }
            else
            {
                damage = CalculateDamageWithElemental(
                    enemyPokemonData.quickAttackMinDamage,
                    enemyPokemonData.quickAttackMaxDamage,
                    enemyPokemonData.tipo,
                    playerPokemonData.tipo,
                    enemyPokemonData.criticalChance,
                    enemyPokemonData.criticalMultiplier,
                    out isCrit, out elementalMult
                );
                attackName = "Ataque Rapido";
            }

            if (enemyAnimator != null) enemyAnimator.PlayAttack();
            yield return new WaitForSeconds(0.25f);
            if (playerAnimator != null) playerAnimator.PlayHit();

            playerCurrentHP = Mathf.Max(0, playerCurrentHP - damage);

            string msg = BuildAttackMessage(enemyPokemonData.pokemonName, attackName, damage, isCrit, elementalMult);
            battleHUD.ShowMessage(msg);
            battleHUD.UpdateHP(playerCurrentHP, enemyCurrentHP);

            yield return new WaitForSeconds(0.3f);

            if (playerCurrentHP <= 0)
            {
                StartCoroutine(EndBattleCoroutine(playerWon: false));
                yield break;
            }

            isPlayerTurn = true;
            battleHUD.SetButtonsInteractable(true);
            battleHUD.ShowMessage($"Sua vez! {playerPokemonData.pokemonName} HP: {playerCurrentHP}");
        }

        // -------------------------------------------------------
        // Cálculo de Dano com Elemental
        // -------------------------------------------------------

        private int CalculateDamageWithElemental(
            int minDmg, int maxDmg,
            ElementalSystem.PokemonType attacker,
            ElementalSystem.PokemonType defender,
            float critChance, float critMult,
            out bool isCrit, out float elementalMult)
        {
            int baseDamage = Random.Range(minDmg, maxDmg + 1);

            // Crítico
            isCrit = Random.value < critChance;
            if (isCrit) baseDamage = Mathf.RoundToInt(baseDamage * critMult);

            // Elemental
            elementalMult = ElementalSystem.GetMultiplier(attacker, defender);
            baseDamage    = Mathf.RoundToInt(baseDamage * elementalMult);

            return baseDamage;
        }

        private string BuildAttackMessage(string attacker, string attackName, int damage, bool isCrit, float elementalMult)
        {
            string msg = $"{attacker} usou {attackName}! -{damage} HP";
            if (isCrit)           msg += " [CRITICO!]";
            if (elementalMult >= 2f)   msg += " [SUPER EFICAZ!]";
            else if (elementalMult <= 0.5f) msg += " [Pouco eficaz...]";
            return msg;
        }

        // -------------------------------------------------------
        // Fim de Batalha
        // -------------------------------------------------------

        private IEnumerator EndBattleCoroutine(bool playerWon)
        {
            isBattleOver    = true;
            isBattleRunning = false;
            battleHUD.SetButtonsInteractable(false);

            string loserName = playerWon ? enemyPokemonData.pokemonName : playerPokemonData.pokemonName;
            battleHUD.ShowMessage($"{loserName} foi derrotado!");

            bool deathComplete = false;
            PokemonAnimator loserAnimator = playerWon ? enemyAnimator : playerAnimator;

            if (loserAnimator != null)
                loserAnimator.PlayDeath(() => deathComplete = true);
            else
                deathComplete = true;

            yield return new WaitUntil(() => deathComplete);
            yield return new WaitForSeconds(0.5f);

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