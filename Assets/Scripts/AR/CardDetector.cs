using UnityEngine;
using Vuforia;

namespace BattleARena.AR
{
    public class CardDetector : MonoBehaviour
    {
        [Header("Dados do Pokémon desta carta")]
        public Battle.PokemonData pokemonData;

        [Tooltip("Modelo 3D filho deste Image Target")]
        public GameObject pokemonModel;

        private ObserverBehaviour observer;
        private bool hasBeenAssigned = false;
        private PokemonSpawnAnimation spawnAnimation;

        void Start()
        {
            observer = GetComponent<ObserverBehaviour>();
            if (observer != null)
                observer.OnTargetStatusChanged += OnTargetStatusChanged;

            // Pega o componente de animação
            if (pokemonModel != null)
            {
                spawnAnimation = pokemonModel.GetComponent<PokemonSpawnAnimation>();
                // Começa invisível com scale zero
                pokemonModel.transform.localScale = Vector3.zero;
                pokemonModel.SetActive(false);
            }
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

            if (isTracked && !hasBeenAssigned)
            {
                bool assigned = BattleSetupManager.Instance.RegisterCard(this);
                if (assigned)
                {
                    hasBeenAssigned = true;
                    // Ativa o modelo e dispara a animação
                    if (pokemonModel != null)
                    {
                        pokemonModel.transform.localScale = Vector3.zero;
                        pokemonModel.SetActive(true);
                        if (spawnAnimation != null)
                            pokemonModel.GetComponent<MonoBehaviour>().StartCoroutine(
                                spawnAnimation.PlaySpawnAnimation()
                            );
                    }
                }
            }

            // Depois de registrado, mostra/esconde com tracking
            if (hasBeenAssigned && pokemonModel != null)
            {
                if (!isTracked)
                    pokemonModel.SetActive(false);
                else if (!pokemonModel.activeSelf)
                    pokemonModel.SetActive(true);
            }
        }

        public void Reset()
        {
            hasBeenAssigned = false;
            if (pokemonModel != null)
            {
                pokemonModel.transform.localScale = Vector3.zero;
                pokemonModel.SetActive(false);
                if (spawnAnimation != null)
                    spawnAnimation.ResetAnimation();
            }
        }
    }
}