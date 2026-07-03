using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace BattleARena.UI
{
    /// <summary>
    /// Controla todos os elementos visuais da HUD de batalha:
    /// barras de HP, mensagens, botões e tela de vitória/derrota.
    /// </summary>
    public class BattleHUD : MonoBehaviour
    {
        [Header("HP do Jogador")]
        public Slider playerHPSlider;
        public TextMeshProUGUI playerHPText;
        public TextMeshProUGUI playerNameText;
        public Image playerHPFill;

        [Header("HP do Inimigo")]
        public Slider enemyHPSlider;
        public TextMeshProUGUI enemyHPText;
        public TextMeshProUGUI enemyNameText;
        public Image enemyHPFill;

        [Header("Mensagem de Batalha")]
        public TextMeshProUGUI battleMessageText;

        [Header("Botões de Ação")]
        public Button quickAttackButton;
        public Button strongAttackButton;

        [Header("Tela de Vitória/Derrota")]
        public GameObject victoryScreen;
        public TextMeshProUGUI victoryText;
        public Button restartButton;

        // Cores da barra de HP
        private readonly Color colorHigh   = new Color(0.2f, 0.8f, 0.2f); // Verde
        private readonly Color colorMedium = new Color(1.0f, 0.8f, 0.0f); // Amarelo
        private readonly Color colorLow    = new Color(0.9f, 0.1f, 0.1f); // Vermelho

        // HP máximo (guardado para calcular percentual)
        private int playerMaxHP;
        private int enemyMaxHP;

        // -------------------------------------------------------
        // Inicialização
        // -------------------------------------------------------

        public void Initialize(string playerName, int playerMaxHP, string enemyName, int enemyMaxHP)
        {
            this.playerMaxHP = playerMaxHP;
            this.enemyMaxHP  = enemyMaxHP;

            playerNameText.text = playerName;
            enemyNameText.text  = enemyName;

            playerHPSlider.maxValue = playerMaxHP;
            enemyHPSlider.maxValue  = enemyMaxHP;

            if (victoryScreen != null)
                victoryScreen.SetActive(false);

            // Conecta botões ao BattleManager
            quickAttackButton.onClick.RemoveAllListeners();
            strongAttackButton.onClick.RemoveAllListeners();
            quickAttackButton.onClick.AddListener(Battle.BattleManager.Instance.PlayerQuickAttack);
            strongAttackButton.onClick.AddListener(Battle.BattleManager.Instance.PlayerStrongAttack);

            if (restartButton != null)
                restartButton.onClick.AddListener(Battle.BattleManager.Instance.RestartBattle);
        }

        // -------------------------------------------------------
        // Atualizar HP
        // -------------------------------------------------------

        public void UpdateHP(int playerHP, int enemyHP)
        {
            // Jogador
            playerHPSlider.value = playerHP;
            playerHPText.text    = $"{playerHP} / {playerMaxHP}";
            UpdateHPBarColor(playerHPFill, playerHP, playerMaxHP);

            // Inimigo
            enemyHPSlider.value = enemyHP;
            enemyHPText.text    = $"{enemyHP} / {enemyMaxHP}";
            UpdateHPBarColor(enemyHPFill, enemyHP, enemyMaxHP);
        }

        private void UpdateHPBarColor(Image fill, int current, int max)
        {
            if (fill == null) return;
            float ratio = (float)current / max;
            if      (ratio > 0.5f) fill.color = colorHigh;
            else if (ratio > 0.25f) fill.color = colorMedium;
            else                    fill.color = colorLow;
        }

        // -------------------------------------------------------
        // Mensagem de Batalha
        // -------------------------------------------------------

        public void ShowMessage(string message)
        {
            if (battleMessageText != null)
                battleMessageText.text = message;
        }

        // -------------------------------------------------------
        // Controle dos Botões
        // -------------------------------------------------------

        public void SetButtonsInteractable(bool interactable)
        {
            quickAttackButton.interactable  = interactable;
            strongAttackButton.interactable = interactable;
        }

        // -------------------------------------------------------
        // Tela de Vitória/Derrota
        // -------------------------------------------------------

        public void ShowVictoryScreen(bool playerWon)
        {
            if (victoryScreen == null) return;
            victoryScreen.SetActive(true);
            victoryText.text = playerWon
                ? "🏆 VITÓRIA!"
                : "💀 DERROTA!";
        }
    }
}
