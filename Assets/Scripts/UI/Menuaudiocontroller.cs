using UnityEngine;
using System.Collections;

namespace BattleARena.UI
{
    /// <summary>
    /// Controla o áudio da tela de menu:
    /// - Música de fundo em loop
    /// - Som de toque ao iniciar
    /// </summary>
    public class MenuAudioController : MonoBehaviour
    {
        [Header("Áudio")]
        public AudioClip musicaFundo;
        public AudioClip somToque;

        [Header("Configuração")]
        [Range(0f, 1f)] public float volumeMusica = 0.7f;
        [Range(0f, 1f)] public float volumeSom    = 1.0f;

        private AudioSource musicSource;
        private AudioSource sfxSource;

        void Awake()
        {
            // Cria dois AudioSources separados — um pra música, um pra SFX
            musicSource = gameObject.AddComponent<AudioSource>();
            sfxSource   = gameObject.AddComponent<AudioSource>();

            // Configura música
            musicSource.clip        = musicaFundo;
            musicSource.loop        = true;
            musicSource.volume      = volumeMusica;
            musicSource.playOnAwake = false;

            // Configura SFX
            sfxSource.loop        = false;
            sfxSource.volume      = volumeSom;
            sfxSource.playOnAwake = false;
        }

        void Start()
        {
            // Inicia música com fade in
            StartCoroutine(FadeInMusic());
        }

        private IEnumerator FadeInMusic()
        {
            if (musicaFundo == null) yield break;

            musicSource.volume = 0f;
            musicSource.Play();

            float elapsed = 0f;
            float fadeDuration = 1.5f;

            while (elapsed < fadeDuration)
            {
                elapsed += Time.deltaTime;
                musicSource.volume = Mathf.Lerp(0f, volumeMusica, elapsed / fadeDuration);
                yield return null;
            }

            musicSource.volume = volumeMusica;
        }

        /// <summary>Toca o som de toque — chamado pelo MenuController ao tocar na tela.</summary>
        public void PlayTouchSound()
        {
            if (somToque != null)
                sfxSource.PlayOneShot(somToque, volumeSom);
        }

        /// <summary>Fade out da música antes de trocar de cena.</summary>
        public IEnumerator FadeOutMusic(float duration)
        {
            float startVolume = musicSource.volume;
            float elapsed = 0f;

            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                musicSource.volume = Mathf.Lerp(startVolume, 0f, elapsed / duration);
                yield return null;
            }

            musicSource.Stop();
        }
    }
}