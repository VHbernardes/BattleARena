using UnityEngine;
using BattleARena.Battle;

namespace BattleARena.AR
{
    /// <summary>
    /// Gerencia a fase de setup antes da batalha:
    /// aguarda as duas cartas serem lidas e inicia a batalha automaticamente.
    /// Primeira carta lida = jogador. Segunda carta lida = IA.
    /// </summary>
    public class BattleSetupManager : MonoBehaviour
    {
        public static BattleSetupManager Instance { get; private set; }

        [Header("Referências")]
        public BattleManager battleManager;
        public UI.BattleHUD battleHUD;

        // Cartas registradas
        private PokemonData playerData = null;
        private PokemonData enemyData  = null;
        private bool battleStarted     = false;

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
            // Mostra HUD inicial pedindo leitura das cartas
            battleHUD.ShowScanScreen(true);
            battleHUD.ShowMessage("Aponte a camera para a sua carta!");
            battleHUD.SetButtonsInteractable(false);
            Debug.Log("[BattleSetupManager] Aguardando leitura das cartas...");
        }

        /// <summary>
        /// Chamado pelo CardDetector quando uma carta é detectada.
        /// Retorna true se a carta foi aceita (não estava registrada ainda).
        /// </summary>
        public bool RegisterCard(CardDetector detector)
        {
            if (battleStarted) return false;

            if (playerData == null)
            {
                playerData = detector.pokemonData;
                battleHUD.ShowMessage($"{playerData.pokemonName} detectado!\nAponte para a carta do oponente!");
                Debug.Log($"[BattleSetupManager] Jogador: {playerData.pokemonName}");
                return true;
            }
            else if (enemyData == null && detector.pokemonData != playerData)
            {
                enemyData = detector.pokemonData;
                battleHUD.ShowMessage($"{enemyData.pokemonName} detectado!\nIniciando batalha...");
                Debug.Log($"[BattleSetupManager] Inimigo: {enemyData.pokemonName}");
                StartBattle();
                return true;
            }

            return false;
        }

        private void StartBattle()
        {
            battleStarted = true;
            battleHUD.ShowScanScreen(false);

            // Passa os dados dinâmicos pro BattleManager
            battleManager.SetPokemonData(playerData, enemyData);
            battleManager.StartBattle();

            Debug.Log("[BattleSetupManager] Batalha iniciada!");
        }

        /// <summary>Reseta o setup para nova batalha.</summary>
        public void ResetSetup()
        {
            playerData    = null;
            enemyData     = null;
            battleStarted = false;

            battleHUD.ShowScanScreen(true);
            battleHUD.SetButtonsInteractable(false);
            battleHUD.ShowMessage("Aponte a camera para a sua carta!");

            // Reseta todos os CardDetectors na cena
            foreach (var detector in FindObjectsOfType<CardDetector>())
                detector.Reset();
        }
    }
}