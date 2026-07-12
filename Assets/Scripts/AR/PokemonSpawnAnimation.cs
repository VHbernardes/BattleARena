using UnityEngine;
using System.Collections;

namespace BattleARena.AR
{
    public class PokemonSpawnAnimation : MonoBehaviour
    {
        [Header("Configuração")]
        public Vector3 finalScale = new Vector3(0.01f, 0.01f, 0.01f);
        public float duration = 0.8f;
        [Range(1f, 1.5f)]
        public float overshootAmount = 1.3f;

        [Header("Flash de Luz")]
        [Tooltip("Material branco com Rendering Mode Fade — deixa vazio para desativar")]
        public Material flashMaterial;
        public float flashDuration = 0.12f;

        private Renderer[] renderers;
        private Material[][] originalMaterials;

        void Awake()
        {
            // Guarda renderers e materiais originais
            renderers = GetComponentsInChildren<Renderer>();
            originalMaterials = new Material[renderers.Length][];
            for (int i = 0; i < renderers.Length; i++)
                originalMaterials[i] = renderers[i].materials;
        }

        public void ResetAnimation()
        {
            transform.localScale = Vector3.zero;
        }

        public IEnumerator PlaySpawnAnimation()
        {
            transform.localScale = Vector3.zero;
            yield return null;

            // Dispara flash no início
            if (flashMaterial != null)
                StartCoroutine(FlashCoroutine());

            float elapsed = 0f;
            float growDuration   = duration * 0.6f;
            float shrinkDuration = duration * 0.4f;
            Vector3 overshootScale = finalScale * overshootAmount;

            // Fase 1: cresce de zero até overshoot
            while (elapsed < growDuration)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.Clamp01(elapsed / growDuration);
                transform.localScale = Vector3.Lerp(Vector3.zero, overshootScale, EaseOutCubic(t));
                yield return null;
            }

            // Fase 2: volta do overshoot ao tamanho final
            elapsed = 0f;
            while (elapsed < shrinkDuration)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.Clamp01(elapsed / shrinkDuration);
                transform.localScale = Vector3.Lerp(overshootScale, finalScale, EaseInOutQuad(t));
                yield return null;
            }

            transform.localScale = finalScale;
        }

        private IEnumerator FlashCoroutine()
        {
            if (flashMaterial == null || renderers == null) yield break;

            // Aplica material branco
            foreach (var r in renderers)
            {
                Material[] flashMats = new Material[r.materials.Length];
                for (int i = 0; i < flashMats.Length; i++)
                    flashMats[i] = flashMaterial;
                r.materials = flashMats;
            }

            yield return new WaitForSeconds(flashDuration);

            // Restaura materiais originais
            for (int i = 0; i < renderers.Length; i++)
            {
                if (renderers[i] != null)
                    renderers[i].materials = originalMaterials[i];
            }
        }

        private float EaseOutCubic(float t) => 1f - Mathf.Pow(1f - t, 3f);
        private float EaseInOutQuad(float t) => t < 0.5f ? 2f * t * t : 1f - Mathf.Pow(-2f * t + 2f, 2f) / 2f;
    }
}