using UnityEngine;

namespace BattleARena.AR
{
    /// <summary>
    /// Garante que o modelo 3D do Pokémon apareça sempre centralizado
    /// e com escala fixa sobre o Image Target, suavizando instabilidades
    /// de tracking (jitter) através de interpolação.
    /// </summary>
    public class TargetStabilizer : MonoBehaviour
    {
        [Header("Configuração de Posição")]
        [Tooltip("Offset de posição local em relação ao centro do Image Target")]
        public Vector3 positionOffset = Vector3.zero;

        [Header("Configuração de Escala")]
        [Tooltip("Escala fixa que o modelo deve manter")]
        public Vector3 fixedScale = new Vector3(0.002f, 0.002f, 0.002f);

        [Header("Suavização")]
        [Tooltip("Velocidade de suavização do movimento (maior = mais rápido)")]
        [Range(1f, 30f)]
        public float smoothSpeed = 15f;

        [Tooltip("Ativa suavização de posição para reduzir tremores (jitter)")]
        public bool smoothPosition = true;

        private Vector3 targetLocalPosition;

        void Start()
        {
            // Aplica a escala fixa uma vez no início
            transform.localScale = fixedScale;
            targetLocalPosition = positionOffset;
            transform.localPosition = positionOffset;
        }

        void Update()
        {
            // Garante que a escala nunca varie, mesmo com jitter de tracking
            if (transform.localScale != fixedScale)
            {
                transform.localScale = fixedScale;
            }

            // Suaviza a posição para reduzir tremores quando o tracking
            // tem pequenas variações frame a frame
            if (smoothPosition)
            {
                transform.localPosition = Vector3.Lerp(
                    transform.localPosition,
                    targetLocalPosition,
                    Time.deltaTime * smoothSpeed
                );
            }
            else
            {
                transform.localPosition = targetLocalPosition;
            }
        }
    }
}
