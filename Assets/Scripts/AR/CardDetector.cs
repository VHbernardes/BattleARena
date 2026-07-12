using UnityEngine;
using Vuforia;
using BattleARena.Animation;

namespace BattleARena.AR
{
    public class CardDetector : MonoBehaviour
    {
        [Header("Dados do Pokémon desta carta")]
        public Battle.PokemonData pokemonData;

        [Tooltip("Modelo 3D filho deste Image Target")]
        public GameObject pokemonModel;

        [HideInInspector] public PokemonAnimator pokemonAnimator;

        private ObserverBehaviour observer;
        private bool hasBeenAssigned = false;
        private PokemonSpawnAnimation spawnAnimation;

        void Start()
        {
            observer = GetComponent<ObserverBehaviour>();
            if (observer != null)
                observer.OnTargetStatusChanged += OnTargetStatusChanged;

            if (pokemonModel != null)
            {
                spawnAnimation  = pokemonModel.GetComponent<PokemonSpawnAnimation>();
                pokemonAnimator = pokemonModel.GetComponent<PokemonAnimator>();
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
                // ResetState restaura posição, rotação e scale originais
                if (pokemonAnimator != null)
                    pokemonAnimator.ResetState();

                // Reseta animação de spawn
                if (spawnAnimation != null)
                    spawnAnimation.ResetAnimation();

                // Reseta cor de todos os renderers
                foreach (var r in pokemonModel.GetComponentsInChildren<Renderer>())
                    r.material.SetColor("_Color", Color.white);

                // Esconde o modelo
                pokemonModel.transform.localScale = Vector3.zero;
                pokemonModel.SetActive(false);
            }
        }
    }
}