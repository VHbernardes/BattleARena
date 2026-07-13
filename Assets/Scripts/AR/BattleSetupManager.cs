using UnityEngine;
using BattleARena.Battle;

namespace BattleARena.AR
{
    public class BattleSetupManager : MonoBehaviour
    {
        public static BattleSetupManager Instance { get; private set; }

        [Header("Referências")]
        public BattleManager battleManager;
        public UI.BattleHUD battleHUD;

        private PokemonData playerData     = null;
        private PokemonData enemyData      = null;
        private Animation.PokemonAnimator playerAnimator = null;
        private Animation.PokemonAnimator enemyAnimator  = null;
        private bool battleStarted = false;

        void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this;
        }

        void OnDestroy()
        {
            if (Instance == this) Instance = null;
        }

        void Start()
        {
            battleHUD.ShowScanScreen(true);
            battleHUD.ShowMessage("Aponte a camera para a sua carta!");
            battleHUD.SetButtonsInteractable(false);
        }

        public bool RegisterCard(CardDetector detector)
        {
            if (battleStarted) return false;

            if (playerData == null)
            {
                playerData     = detector.pokemonData;
                playerAnimator = detector.pokemonAnimator;
                battleHUD.ShowMessage($"{playerData.pokemonName} detectado!\nAponte para a carta do oponente!");
                Debug.Log($"[BattleSetupManager] Jogador: {playerData.pokemonName}");
                return true;
            }
            else if (enemyData == null && detector.pokemonData != playerData)
            {
                enemyData     = detector.pokemonData;
                enemyAnimator = detector.pokemonAnimator;
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

            // Passa dados E animadores dinamicamente
            battleManager.SetPokemonData(playerData, enemyData);
            battleManager.SetAnimators(playerAnimator, enemyAnimator);
            battleManager.StartBattle();
        }

        public void ResetSetup()
        {
            playerData     = null;
            enemyData      = null;
            playerAnimator = null;
            enemyAnimator  = null;
            battleStarted  = false;

            // A cena não é recarregada entre partidas, então o Start() do
            // BattleAudioController não roda de novo. Este é o gatilho que
            // religa a BGM (do início, com fade-in) a cada nova batalha.
            if (Audio.BattleAudioController.Instance != null)
                Audio.BattleAudioController.Instance.RestartBattleMusic();

            battleHUD.ShowScanScreen(true);
            battleHUD.SetButtonsInteractable(false);
            battleHUD.ShowMessage("Aponte a camera para a sua carta!");

            // FindObjectsOfType está deprecado nas versões recentes da Unity.
            foreach (var detector in FindObjectsByType<CardDetector>(FindObjectsSortMode.None))
                detector.Reset();
        }
    }
}