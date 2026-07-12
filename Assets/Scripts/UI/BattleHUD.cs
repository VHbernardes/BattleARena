using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace BattleARena.UI
{
    public class BattleHUD : MonoBehaviour
    {
        [Header("Tela de Scan (antes da batalha)")]
        public GameObject scanScreen;
        public TextMeshProUGUI scanMessageText;

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

        private readonly Color colorHigh   = new Color(0.2f, 0.8f, 0.2f);
        private readonly Color colorMedium = new Color(1.0f, 0.8f, 0.0f);
        private readonly Color colorLow    = new Color(0.9f, 0.1f, 0.1f);

        private int playerMaxHP;
        private int enemyMaxHP;
        private bool isInScanPhase = true;

        // -------------------------------------------------------
        // Tela de Scan
        // -------------------------------------------------------

        public void ShowScanScreen(bool show)
        {
            isInScanPhase = show;

            // Scan screen
            if (scanScreen != null)
                scanScreen.SetActive(show);

            // Painéis de HP — só durante batalha
            if (playerHPSlider != null)
                playerHPSlider.transform.parent.gameObject.SetActive(!show);
            if (enemyHPSlider != null)
                enemyHPSlider.transform.parent.gameObject.SetActive(!show);

            // Mensagem de batalha — só durante batalha
            if (battleMessageText != null)
                battleMessageText.gameObject.SetActive(!show);

            // Botões — só durante batalha
            if (quickAttackButton != null)
                quickAttackButton.gameObject.SetActive(!show);
            if (strongAttackButton != null)
                strongAttackButton.gameObject.SetActive(!show);

            // Garante que VictoryScreen está escondido ao voltar pro scan
            if (show && victoryScreen != null)
                victoryScreen.SetActive(false);
        }

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

            quickAttackButton.onClick.RemoveAllListeners();
            strongAttackButton.onClick.RemoveAllListeners();
            quickAttackButton.onClick.AddListener(Battle.BattleManager.Instance.PlayerQuickAttack);
            strongAttackButton.onClick.AddListener(Battle.BattleManager.Instance.PlayerStrongAttack);

            if (restartButton != null)
            {
                restartButton.onClick.RemoveAllListeners();
                restartButton.onClick.AddListener(Battle.BattleManager.Instance.RestartBattle);
            }
        }

        // -------------------------------------------------------
        // Atualizar HP
        // -------------------------------------------------------

        public void UpdateHP(int playerHP, int enemyHP)
        {
            playerHPSlider.value = playerHP;
            playerHPText.text    = $"{playerHP} / {playerMaxHP}";
            UpdateHPBarColor(playerHPFill, playerHP, playerMaxHP);

            enemyHPSlider.value = enemyHP;
            enemyHPText.text    = $"{enemyHP} / {enemyMaxHP}";
            UpdateHPBarColor(enemyHPFill, enemyHP, enemyMaxHP);
        }

        private void UpdateHPBarColor(Image fill, int current, int max)
        {
            if (fill == null) return;
            float ratio = (float)current / max;
            if      (ratio > 0.5f)  fill.color = colorHigh;
            else if (ratio > 0.25f) fill.color = colorMedium;
            else                    fill.color = colorLow;
        }

        // -------------------------------------------------------
        // Mensagens
        // -------------------------------------------------------

        public void ShowMessage(string message)
        {
            if (isInScanPhase)
            {
                if (scanMessageText != null)
                    scanMessageText.text = message;
            }
            else
            {
                if (battleMessageText != null)
                    battleMessageText.text = message;
            }
        }

        // -------------------------------------------------------
        // Botões
        // -------------------------------------------------------

        public void SetButtonsInteractable(bool interactable)
        {
            quickAttackButton.interactable  = interactable;
            strongAttackButton.interactable = interactable;
        }

        // -------------------------------------------------------
        // Vitória/Derrota
        // -------------------------------------------------------

        public void ShowVictoryScreen(bool playerWon)
        {
            if (victoryScreen == null) return;
            victoryScreen.SetActive(true);
            if (victoryText != null)
                victoryText.text = playerWon ? "VITORIA!" : "DERROTA!";
        }
    }
}