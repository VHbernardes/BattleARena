using UnityEngine;
using System.Collections;

namespace BattleARena.Animation
{
    /// <summary>
    /// Controla todas as animações de batalha do Pokémon via código.
    /// Funciona sem rig ou clips de animação — usa Transform puro.
    /// </summary>
    public class PokemonAnimator : MonoBehaviour
    {
        public enum PokemonType { Biped, Quadruped, Flying }

        [Header("Tipo de Pokémon")]
        public PokemonType pokemonType = PokemonType.Biped;

        [Header("Configuração de Idle")]
        public float idleBobSpeed    = 1.5f;   // velocidade do balanço
        public float idleBobAmount   = 0.002f; // amplitude do balanço Y
        public float idleTiltAmount  = 2f;     // graus de inclinação lateral

        [Header("Configuração de Ataque")]
        public float attackMoveDistance = 0.05f; // distância que avança
        public float attackDuration     = 0.5f;  // duração total do ataque
        public Vector3 attackDirection  = Vector3.forward; // direção do ataque

        [Header("Configuração de Hit")]
        public float hitRecoilDistance = 0.02f; // distância que recua
        public float hitDuration       = 0.3f;

        // Estado interno
        private Vector3 originalLocalPosition;
        private Quaternion originalLocalRotation;
        private bool isPlayingAction = false;
        private Coroutine idleCoroutine;

        void Start()
        {
            originalLocalPosition = transform.localPosition;
            originalLocalRotation = transform.localRotation;
            StartIdle();
        }

        // -------------------------------------------------------
        // Idle
        // -------------------------------------------------------

        public void StartIdle()
        {
            if (idleCoroutine != null)
                StopCoroutine(idleCoroutine);

            switch (pokemonType)
            {
                case PokemonType.Biped:
                    idleCoroutine = StartCoroutine(IdleBipedCoroutine());
                    break;
                case PokemonType.Quadruped:
                    idleCoroutine = StartCoroutine(IdleQuadrupedCoroutine());
                    break;
                case PokemonType.Flying:
                    idleCoroutine = StartCoroutine(IdleFlyingCoroutine());
                    break;
            }
        }

        /// <summary>Bípede: balanço lateral suave + leve bob vertical</summary>
        private IEnumerator IdleBipedCoroutine()
        {
            float time = 0f;
            while (true)
            {
                if (!isPlayingAction)
                {
                    time += Time.deltaTime * idleBobSpeed;

                    // Bob vertical
                    float bobY = Mathf.Sin(time) * idleBobAmount;
                    transform.localPosition = originalLocalPosition + new Vector3(0, bobY, 0);

                    // Inclinação lateral suave
                    float tilt = Mathf.Sin(time * 0.7f) * idleTiltAmount;
                    transform.localRotation = originalLocalRotation * Quaternion.Euler(0, 0, tilt);
                }
                yield return null;
            }
        }

        /// <summary>Quadrúpede: respiração (scale Y) + bob mais rasteiro</summary>
        private IEnumerator IdleQuadrupedCoroutine()
        {
            float time = 0f;
            Vector3 baseScale = transform.localScale;
            while (true)
            {
                if (!isPlayingAction)
                {
                    time += Time.deltaTime * idleBobSpeed;

                    // Respiração via scale Y sutil
                    float breathe = 1f + Mathf.Sin(time) * 0.03f;
                    transform.localScale = new Vector3(baseScale.x, baseScale.y * breathe, baseScale.z);

                    // Bob vertical pequeno
                    float bobY = Mathf.Sin(time) * (idleBobAmount * 0.5f);
                    transform.localPosition = originalLocalPosition + new Vector3(0, bobY, 0);
                }
                yield return null;
            }
        }

        /// <summary>Voador: flutuação vertical + leve rotação</summary>
        private IEnumerator IdleFlyingCoroutine()
        {
            float time = 0f;
            while (true)
            {
                if (!isPlayingAction)
                {
                    time += Time.deltaTime * (idleBobSpeed * 0.8f);

                    // Flutuação vertical maior
                    float floatY = Mathf.Sin(time) * (idleBobAmount * 3f);
                    transform.localPosition = originalLocalPosition + new Vector3(0, floatY, 0);

                    // Leve inclinação de voo
                    float tilt = Mathf.Sin(time * 0.5f) * (idleTiltAmount * 0.5f);
                    transform.localRotation = originalLocalRotation * Quaternion.Euler(tilt, 0, 0);
                }
                yield return null;
            }
        }

        // -------------------------------------------------------
        // Ataque
        // -------------------------------------------------------

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

            // Fase 1: avança
            float elapsed = 0f;
            while (elapsed < advanceDuration)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.Clamp01(elapsed / advanceDuration);
                transform.localPosition = Vector3.Lerp(startPos, targetPos, EaseOutCubic(t));
                yield return null;
            }

            // Fase 2: pausa no impacto
            yield return new WaitForSeconds(holdDuration);

            // Fase 3: volta
            elapsed = 0f;
            while (elapsed < returnDuration)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.Clamp01(elapsed / returnDuration);
                transform.localPosition = Vector3.Lerp(targetPos, startPos, EaseInOutQuad(t));
                yield return null;
            }

            transform.localPosition = startPos;
            isPlayingAction = false;
        }

        // -------------------------------------------------------
        // Hit (toma dano)
        // -------------------------------------------------------

        public void PlayHit()
        {
            if (isPlayingAction) return;
            StartCoroutine(HitCoroutine());
        }

        private IEnumerator HitCoroutine()
        {
            isPlayingAction = true;

            Vector3 startPos  = originalLocalPosition;
            Vector3 recoilPos = originalLocalPosition - attackDirection * hitRecoilDistance;

            float recoilDuration = hitDuration * 0.3f;
            float returnDuration = hitDuration * 0.7f;

            // Recua
            float elapsed = 0f;
            while (elapsed < recoilDuration)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.Clamp01(elapsed / recoilDuration);
                transform.localPosition = Vector3.Lerp(startPos, recoilPos, EaseOutCubic(t));

                // Flash vermelho piscando
                float flash = Mathf.PingPong(elapsed * 20f, 1f);
                GetComponentInChildren<Renderer>()?.material.SetColor(
                    "_Color", Color.Lerp(Color.white, Color.red, flash * 0.5f)
                );
                yield return null;
            }

            // Volta
            elapsed = 0f;
            while (elapsed < returnDuration)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.Clamp01(elapsed / returnDuration);
                transform.localPosition = Vector3.Lerp(recoilPos, startPos, EaseInOutQuad(t));
                yield return null;
            }

            transform.localPosition = startPos;

            // Restaura cor original
            GetComponentInChildren<Renderer>()?.material.SetColor("_Color", Color.white);

            isPlayingAction = false;
        }

        // -------------------------------------------------------
        // Easing
        // -------------------------------------------------------

        private float EaseOutCubic(float t)  => 1f - Mathf.Pow(1f - t, 3f);
        private float EaseInOutQuad(float t) => t < 0.5f ? 2f * t * t : 1f - Mathf.Pow(-2f * t + 2f, 2f) / 2f;
    }
}