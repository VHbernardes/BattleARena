using UnityEngine;
using System.Collections;

namespace BattleARena.Animation
{
    public class PokemonAnimator : MonoBehaviour
    {
        public enum PokemonType { Biped, Quadruped, Flying }

        [Header("Tipo de Pokémon")]
        public PokemonType pokemonType = PokemonType.Biped;

        [Header("Configuração de Idle")]
        public float idleBobSpeed   = 1.5f;
        public float idleBobAmount  = 0.002f;
        public float idleTiltAmount = 2f;

        [Header("Configuração de Ataque")]
        public float attackMoveDistance = 0.05f;
        public float attackDuration     = 0.5f;
        public Vector3 attackDirection  = Vector3.forward;

        [Header("Configuração de Hit")]
        public float hitRecoilDistance = 0.02f;
        public float hitDuration       = 0.3f;

        [Header("Configuração de Morte")]
        public float deathDuration = 1.2f;

        private Vector3 originalLocalPosition;
        private Quaternion originalLocalRotation;
        private Vector3 originalLocalScale;
        private bool isPlayingAction = false;
        private Coroutine idleCoroutine;

        void Awake()
        {
            originalLocalPosition = transform.localPosition;
            originalLocalRotation = transform.localRotation;
            originalLocalScale    = transform.localScale;
            // StartIdle();
        }

        void OnEnable()
        {
            // Rede de seguranca: toda reativacao volta ao estado neutro.
            // Coroutines sao destruidas ao desativar, entao a idle PRECISA
            // ser reiniciada aqui, e nao no Reset.
            transform.localPosition = originalLocalPosition;
            transform.localRotation = originalLocalRotation;
            isPlayingAction = false;
            StartIdle();
        }

        public void StartIdle()
        {
            if (idleCoroutine != null) StopCoroutine(idleCoroutine);
            switch (pokemonType)
            {
                case PokemonType.Biped:     idleCoroutine = StartCoroutine(IdleBipedCoroutine());     break;
                case PokemonType.Quadruped: idleCoroutine = StartCoroutine(IdleQuadrupedCoroutine()); break;
                case PokemonType.Flying:    idleCoroutine = StartCoroutine(IdleFlyingCoroutine());    break;
            }
        }

        public void StopIdle()
        {
            if (idleCoroutine != null)
            {
                StopCoroutine(idleCoroutine);
                idleCoroutine = null;
            }
        }

        private IEnumerator IdleBipedCoroutine()
        {
            float time = 0f;
            while (true)
            {
                if (!isPlayingAction)
                {
                    time += Time.deltaTime * idleBobSpeed;
                    float bobY = Mathf.Sin(time) * idleBobAmount;
                    transform.localPosition = originalLocalPosition + new Vector3(0, bobY, 0);
                    float tilt = Mathf.Sin(time * 0.7f) * idleTiltAmount;
                    transform.localRotation = originalLocalRotation * Quaternion.Euler(0, 0, tilt);
                }
                yield return null;
            }
        }

        private IEnumerator IdleQuadrupedCoroutine()
        {
            float time = 0f;
            Vector3 baseScale = transform.localScale;
            while (true)
            {
                if (!isPlayingAction)
                {
                    time += Time.deltaTime * idleBobSpeed;
                    float breathe = 1f + Mathf.Sin(time) * 0.03f;
                    transform.localScale    = new Vector3(baseScale.x, baseScale.y * breathe, baseScale.z);
                    float bobY = Mathf.Sin(time) * (idleBobAmount * 0.5f);
                    transform.localPosition = originalLocalPosition + new Vector3(0, bobY, 0);
                }
                yield return null;
            }
        }

        private IEnumerator IdleFlyingCoroutine()
        {
            float time = 0f;
            while (true)
            {
                if (!isPlayingAction)
                {
                    time += Time.deltaTime * (idleBobSpeed * 0.8f);
                    float floatY = Mathf.Sin(time) * (idleBobAmount * 3f);
                    transform.localPosition = originalLocalPosition + new Vector3(0, floatY, 0);
                    float tilt = Mathf.Sin(time * 0.5f) * (idleTiltAmount * 0.5f);
                    transform.localRotation = originalLocalRotation * Quaternion.Euler(tilt, 0, 0);
                }
                yield return null;
            }
        }

        public void PlayAttack()
        {
            if (isPlayingAction) return;
            StartCoroutine(AttackCoroutine());
        }

        private IEnumerator AttackCoroutine()
        {
            isPlayingAction = true;
            Vector3 startPos  = originalLocalPosition;
            Vector3 targetPos = originalLocalPosition + attackDirection * attackMoveDistance;

            float advanceDuration = attackDuration * 0.4f;
            float holdDuration    = attackDuration * 0.1f;
            float returnDuration  = attackDuration * 0.5f;

            float elapsed = 0f;
            while (elapsed < advanceDuration)
            {
                elapsed += Time.deltaTime;
                transform.localPosition = Vector3.Lerp(startPos, targetPos, EaseOutCubic(Mathf.Clamp01(elapsed / advanceDuration)));
                yield return null;
            }

            yield return new WaitForSeconds(holdDuration);

            elapsed = 0f;
            while (elapsed < returnDuration)
            {
                elapsed += Time.deltaTime;
                transform.localPosition = Vector3.Lerp(targetPos, startPos, EaseInOutQuad(Mathf.Clamp01(elapsed / returnDuration)));
                yield return null;
            }

            transform.localPosition = startPos;
            isPlayingAction = false;
        }

        public void PlayHit()
        {
            if (isPlayingAction) return;
            StartCoroutine(HitCoroutine());
        }

        private IEnumerator HitCoroutine()
        {
            isPlayingAction = true;

            // Som de dano
            if (Audio.BattleAudioController.Instance != null)
                Audio.BattleAudioController.Instance.PlayDano();

            Vector3 startPos  = originalLocalPosition;
            Vector3 recoilPos = originalLocalPosition - attackDirection * hitRecoilDistance;

            float recoilDuration = hitDuration * 0.3f;
            float returnDuration = hitDuration * 0.7f;

            float elapsed = 0f;
            while (elapsed < recoilDuration)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.Clamp01(elapsed / recoilDuration);
                transform.localPosition = Vector3.Lerp(startPos, recoilPos, EaseOutCubic(t));
                float flash = Mathf.PingPong(elapsed * 20f, 1f);
                GetComponentInChildren<Renderer>()?.material.SetColor("_Color", Color.Lerp(Color.white, Color.red, flash * 0.5f));
                yield return null;
            }

            elapsed = 0f;
            while (elapsed < returnDuration)
            {
                elapsed += Time.deltaTime;
                transform.localPosition = Vector3.Lerp(recoilPos, startPos, EaseInOutQuad(Mathf.Clamp01(elapsed / returnDuration)));
                yield return null;
            }

            transform.localPosition = startPos;
            GetComponentInChildren<Renderer>()?.material.SetColor("_Color", Color.white);
            isPlayingAction = false;
        }

        public void PlayDeath(System.Action onComplete = null)
        {
            StartCoroutine(DeathCoroutine(onComplete));
        }

        private IEnumerator DeathCoroutine(System.Action onComplete)
        {
            StopIdle();
            isPlayingAction = true;

            Vector3 startScale       = transform.localScale;
            Vector3 startPosition    = transform.localPosition;
            Quaternion startRotation = transform.localRotation;

            float elapsed      = 0f;
            float spinDuration = deathDuration * 0.6f;
            float fadeDuration = deathDuration * 0.4f;

            Quaternion fallRotation = startRotation * Quaternion.Euler(0, 0, -90f);
            Vector3 fallPosition    = startPosition - new Vector3(0, originalLocalScale.y * 0.5f, 0);

            while (elapsed < spinDuration)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.Clamp01(elapsed / spinDuration);
                transform.localRotation = Quaternion.Lerp(startRotation, fallRotation, EaseInOutQuad(t));
                transform.localPosition = Vector3.Lerp(startPosition, fallPosition, EaseInOutQuad(t));

                float flash = Mathf.PingPong(elapsed * 15f, 1f);
                foreach (var r in GetComponentsInChildren<Renderer>())
                    r.material.SetColor("_Color", Color.Lerp(Color.white, Color.red, flash * 0.6f));

                yield return null;
            }

            yield return new WaitForSeconds(0.1f);

            elapsed = 0f;
            while (elapsed < fadeDuration)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.Clamp01(elapsed / fadeDuration);
                transform.localScale = Vector3.Lerp(startScale, Vector3.zero, EaseInOutQuad(t));
                yield return null;
            }

            transform.localScale = Vector3.zero;
            isPlayingAction = false;
            gameObject.SetActive(false);
            onComplete?.Invoke();
        }

        public void ResetState()
        {
            StopAllCoroutines();
            idleCoroutine   = null;      // <<< o handle estava ficando pendurado
            isPlayingAction = false;

            transform.localPosition = originalLocalPosition;
            transform.localRotation = originalLocalRotation;
            transform.localScale    = originalLocalScale;

            foreach (var r in GetComponentsInChildren<Renderer>())
                r.material.SetColor("_Color", Color.white);

            // StartIdle() removido: quem cuida disso agora e o OnEnable()
        }

        private float EaseOutCubic(float t)  => 1f - Mathf.Pow(1f - t, 3f);
        private float EaseInOutQuad(float t) => t < 0.5f ? 2f * t * t : 1f - Mathf.Pow(-2f * t + 2f, 2f) / 2f;
    }
}