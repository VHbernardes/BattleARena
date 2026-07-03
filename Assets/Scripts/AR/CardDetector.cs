using UnityEngine;
using Vuforia;

namespace BattleARena.AR
{
    /// <summary>
    /// Detecta quando uma carta entra no campo de visão e notifica
    /// o BattleManager. A primeira carta detectada vira o jogador,
    /// a segunda vira a IA.
    /// </summary>
    public class CardDetector : MonoBehaviour
    {
        public enum CardOwner { Player, Enemy }

        [Header("Configuração")]
        [Tooltip("Define se essa carta pertence ao jogador ou à IA")]
        public CardOwner cardOwner = CardOwner.Player;

        [Tooltip("GameObject do Pokémon filho deste Image Target")]
        public GameObject pokemonModel;

        private ObserverBehaviour observer;

        void Start()
        {
            observer = GetComponent<ObserverBehaviour>();
            if (observer != null)
            {
                observer.OnTargetStatusChanged += OnTargetStatusChanged;
            }

            // Começa invisível — aparece só quando a carta é detectada
            if (pokemonModel != null)
                pokemonModel.SetActive(false);
        }

        void OnDestroy()
        {
            if (observer != null)
                observer.OnTargetStatusChanged -= OnTargetStatusChanged;
        }

        private void OnTargetStatusChanged(ObserverBehaviour behaviour, TargetStatus status)
        {
            bool isTracked = status.Status == Status.TRACKED ||
                             status.Status == Status.EXTENDED_TRACKED;

            if (pokemonModel != null)
                pokemonModel.SetActive(isTracked);

            if (isTracked)
            {
                Debug.Log($"[CardDetector] Carta detectada: {cardOwner}");
            }
        }
    }
}
