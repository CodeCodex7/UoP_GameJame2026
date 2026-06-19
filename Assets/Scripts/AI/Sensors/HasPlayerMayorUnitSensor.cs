using AI.Goap.UnitAI.Behaviors;
using CrashKonijn.Agent.Core;
using CrashKonijn.Goap.Core;
using CrashKonijn.Goap.Runtime;

namespace AI.Goap.UnitAI.Sensors
{
    public class HasPlayerMayorUnitSensor : LocalWorldSensorBase
    {
        public override void Created()
        {
        }

        public override void Update()
        {
        }

        public override SenseValue Sense(IActionReceiver agent, IComponentReference references)
        {
            if (!agent.Transform.TryGetComponent<UnitAIBrain>(out var brain) ||
                !brain.IsEnemyTeam() ||
                !Services.TryResolve<GameDataStore>(out var gameDataStore))
            {
                return 0;
            }

            return gameDataStore.TryGetClosestRivalUnitOfType(brain, UnitType.Mayor, float.PositiveInfinity, out _) ? 1 : 0;
        }
    }
}
