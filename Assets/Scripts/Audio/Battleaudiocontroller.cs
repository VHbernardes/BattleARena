using UnityEngine;
using System.Collections;

namespace BattleARena.Audio
{
    /// <summary>
    /// Controla todo o áudio da cena de batalha (MainScene).
    /// Música de fundo em loop com volume baixo (ambiente),
    /// e SFX para surgimento, dano, vitória e derrota.
    ///
    /// A cena NÃO é recarregada entre partidas: BattleManager.RestartBattle()
    /// apenas reseta o estado via BattleSetupManager.ResetSetup(). Por isso o
    /// reinício da música depende de uma chamada explícita a RestartBattleMusic().
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

        // Handle único das coroutines de música. Garante que fade-in e fade-out
        // nunca disputem musicSource.volume ao mesmo tempo.
        private Coroutine musicRoutine;

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

        void OnDestroy()
        {
            if (Instance == this) Instance = null;
        }

        void Start()
        {
            RestartBattleMusic();
        }

        // ---------- Música ----------

        /// <summary>
        /// Reinicia a BGM do zero, com fade-in. Idempotente: pode ser chamado
        /// quantas vezes for preciso sem sobrepor áudio nem duplicar coroutines.
        /// Chamado no Start() e por BattleManager.RestartBattle().
        /// </summary>
        public void RestartBattleMusic()
        {
            if (musicaBatalha == null) return;

            StopMusicRoutine();

            musicSource.Stop();          // limpa o estado deixado pelo fade-out
            musicSource.clip   = musicaBatalha;
            musicSource.time   = 0f;     // volta ao início do clipe
            musicSource.volume = 0f;
            musicSource.loop   = true;

            musicRoutine = StartCoroutine(FadeInMusic());
        }

        /// <summary>Fade-out e parada da música. Seguro chamar se já estiver parada.</summary>
        public void StopBattleMusic()
        {
            if (!musicSource.isPlaying) return;

            StopMusicRoutine();
            musicRoutine = StartCoroutine(FadeOutMusic());
        }

        private void StopMusicRoutine()
        {
            if (musicRoutine != null)
            {
                StopCoroutine(musicRoutine);
                musicRoutine = null;
            }
        }

        private IEnumerator FadeInMusic()
        {
            musicSource.Play();

            float elapsed = 0f;
            while (elapsed < fadeInDuration)
            {
                elapsed += Time.unscaledDeltaTime;
                musicSource.volume = Mathf.Lerp(0f, volumeMusica, elapsed / fadeInDuration);
                yield return null;
            }

            musicSource.volume = volumeMusica;
            musicRoutine = null;
        }

        private IEnumerator FadeOutMusic()
        {
            float startVolume = musicSource.volume;
            float elapsed     = 0f;

            while (elapsed < fadeOutDuration)
            {
                elapsed += Time.unscaledDeltaTime;
                musicSource.volume = Mathf.Lerp(startVolume, 0f, elapsed / fadeOutDuration);
                yield return null;
            }

            musicSource.volume = 0f;
            musicSource.Stop();
            musicRoutine = null;
        }

        // ---------- SFX ----------

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
            StopBattleMusic();
            if (somVitoria != null)
                sfxSource.PlayOneShot(somVitoria, volumeSFX);
        }

        public void PlayDerrota()
        {
            StopBattleMusic();
            if (somDerrota != null)
                sfxSource.PlayOneShot(somDerrota, volumeSFX);
        }
    }
}