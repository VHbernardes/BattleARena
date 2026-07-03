using UnityEngine;

namespace BattleARena.AI
{
    /// <summary>
    /// Componente de IA — extensível no futuro para comportamentos
    /// mais complexos (escolha baseada em HP, tipo elemental, etc).
    /// A lógica atual de ataque está no BattleManager (EnemyTurnCoroutine).
    /// </summary>
    public class AIController : MonoBehaviour
    {
        // Reservado para expansão futura:
        // - IA que prioriza Ataque Forte quando jogador está com baixo HP
        // - IA que "defende" quando próprio HP está crítico
        // - Sistema elemental de vantagem de tipo
    }
}
