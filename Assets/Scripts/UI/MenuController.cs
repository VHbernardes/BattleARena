using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem;
using System.Collections;
using TMPro;

namespace BattleARena.UI
{
    public class MenuController : MonoBehaviour
    {
        [Header("Referências")]
        public RectTransform logoTransform;
        public CanvasGroup logoCanvasGroup;
        public TextMeshProUGUI tapToStartText;
        public MenuAudioController audioController;

        [Header("Animação de Entrada")]
        public float entryDuration = 1.2f;
        public float entryDelay    = 0.3f;

        [Header("Pulsação do Logo")]
        public float pulseSpeed  = 1.2f;
        public float pulseAmount = 0.04f;

        [Header("Piscar do Texto")]
        public float blinkSpeed = 1.5f;

        [Header("Transição")]
        public string battleSceneName   = "MainScene";
        public float transitionDuration = 0.8f;

        private bool canTap       = false;
        private bool isTransiting = false;
        private Vector3 originalLogoScale;

        void Start()
        {
            if (logoCanvasGroup != null) logoCanvasGroup.alpha = 0f;
            if (tapToStartText != null)  tapToStartText.alpha  = 0f;
            if (logoTransform != null)
            {
                originalLogoScale = logoTransform.localScale;
                logoTransform.localScale = originalLogoScale * 0.5f;
            }

            StartCoroutine(EntrySequence());
        }

        void Update()
        {
            if (canTap && !isTransiting)
            {
                bool tapped = false;

                var touchscreen = Touchscreen.current;
                if (touchscreen != null && touchscreen.primaryTouch.press.wasPressedThisFrame)
                    tapped = true;

                var mouse = Mouse.current;
                if (mouse != null && mouse.leftButton.wasPressedThisFrame)
                    tapped = true;

                if (tapped)
                    StartCoroutine(TransitionToBattle());
            }
        }

        private IEnumerator EntrySequence()
        {
            yield return new WaitForSeconds(entryDelay);

            float elapsed = 0f;
            while (elapsed < entryDuration)
            {
                elapsed += Time.deltaTime;
                float t     = Mathf.Clamp01(elapsed / entryDuration);
                float eased = EaseOutBack(t);

                if (logoCanvasGroup != null)
                    logoCanvasGroup.alpha = Mathf.Clamp01(t * 2f);
                if (logoTransform != null)
                    logoTransform.localScale = Vector3.Lerp(originalLogoScale * 0.5f, originalLogoScale, eased);

                yield return null;
            }

            if (logoCanvasGroup != null) logoCanvasGroup.alpha = 1f;
            if (logoTransform != null)   logoTransform.localScale = originalLogoScale;

            yield return new WaitForSeconds(0.3f);

            if (tapToStartText != null)
            {
                elapsed = 0f;
                while (elapsed < 0.5f)
                {
                    elapsed += Time.deltaTime;
                    tapToStartText.alpha = Mathf.Clamp01(elapsed / 0.5f);
                    yield return null;
                }
            }

            canTap = true;
            StartCoroutine(LogoPulseCoroutine());
            StartCoroutine(TextBlinkCoroutine());
        }

        private IEnumerator LogoPulseCoroutine()
        {
            while (!isTransiting)
            {
                float t = Mathf.Sin(Time.time * pulseSpeed) * pulseAmount;
                if (logoTransform != null)
                    logoTransform.localScale = originalLogoScale * (1f + t);
                yield return null;
            }
        }

        private IEnumerator TextBlinkCoroutine()
        {
            while (!isTransiting)
            {
                float alpha = (Mathf.Sin(Time.time * blinkSpeed * Mathf.PI) + 1f) / 2f;
                if (tapToStartText != null)
                    tapToStartText.alpha = Mathf.Clamp(alpha, 0.2f, 1f);
                yield return null;
            }
        }

        private IEnumerator TransitionToBattle()
        {
            isTransiting = true;
            canTap       = false;

            // Toca som de toque
            if (audioController != null)
                audioController.PlayTouchSound();

            // Fade out da música junto com a transição
            if (audioController != null)
                StartCoroutine(audioController.FadeOutMusic(transitionDuration));

            // Fade out da tela
            float elapsed = 0f;
            while (elapsed < transitionDuration)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.Clamp01(elapsed / transitionDuration);

                if (logoCanvasGroup != null)
                    logoCanvasGroup.alpha = 1f - t;
                if (tapToStartText != null)
                    tapToStartText.alpha = 1f - t;

                yield return null;
            }

            SceneManager.LoadScene(battleSceneName);
        }

        private float EaseOutBack(float t)
        {
            float c1 = 1.70158f;
            float c3 = c1 + 1f;
            return 1f + c3 * Mathf.Pow(t - 1f, 3f) + c1 * Mathf.Pow(t - 1f, 2f);
        }
    }
}