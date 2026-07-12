using UnityEngine;
using System.Collections;

namespace BattleARena.Audio
{
    /// <summary>
    /// Controla todo o áudio da cena de batalha.
    /// Música de fundo em loop com volume baixo (ambiente),
    /// e SFX para surgimento, dano, vitória e derrota.
    /// </summary>
    public class BattleAudioController : MonoBehaviour
    {
        public static BattleAudioController Instance { get; private set; }

        [Header("Música de Fundo")]
        public AudioClip musicaBatalha;
        [Range(0f, 1f)] public float volumeMusica = 0.25f;
        public float fadeInDuration  = 2.0f;
        public float fadeOutDuration = 1.5f;

        [Header("SFX")]
        public AudioClip somSurgimento;
        public AudioClip somDano;
        public AudioClip somVitoria;
        public AudioClip somDerrota;
        [Range(0f, 1f)] public float volumeSFX = 0.9f;

        private AudioSource musicSource;
        private AudioSource sfxSource;

        void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this;

            musicSource = gameObject.AddComponent<AudioSource>();
            sfxSource   = gameObject.AddComponent<AudioSource>();

            musicSource.clip        = musicaBatalha;
            musicSource.loop        = true;
            musicSource.volume      = 0f;
            musicSource.playOnAwake = false;

            sfxSource.loop        = false;
            sfxSource.volume      = volumeSFX;
            sfxSource.playOnAwake = false;
        }

        void Start()
        {
            StartCoroutine(FadeInMusic());
        }

        private IEnumerator FadeInMusic()
        {
            if (musicaBatalha == null) yield break;
            musicSource.Play();
            float elapsed = 0f;
            while (elapsed < fadeInDuration)
            {
                elapsed += Time.deltaTime;
                musicSource.volume = Mathf.Lerp(0f, volumeMusica, elapsed / fadeInDuration);
                yield return null;
            }
            musicSource.volume = volumeMusica;
        }

        public IEnumerator FadeOutMusic()
        {
            float startVolume = musicSource.volume;
            float elapsed     = 0f;
            while (elapsed < fadeOutDuration)
            {
                elapsed += Time.deltaTime;
                musicSource.volume = Mathf.Lerp(startVolume, 0f, elapsed / fadeOutDuration);
                yield return null;
            }
            musicSource.Stop();
        }

        public void PlaySurgimento()
        {
            if (somSurgimento != null)
                sfxSource.PlayOneShot(somSurgimento, volumeSFX);
        }

        public void PlayDano()
        {
            if (somDano != null)
                sfxSource.PlayOneShot(somDano, volumeSFX);
        }

        public void PlayVitoria()
        {
            StartCoroutine(FadeOutMusic());
            if (somVitoria != null)
                sfxSource.PlayOneShot(somVitoria, volumeSFX);
        }

        public void PlayDerrota()
        {
            StartCoroutine(FadeOutMusic());
            if (somDerrota != null)
                sfxSource.PlayOneShot(somDerrota, volumeSFX);
        }
    }
}