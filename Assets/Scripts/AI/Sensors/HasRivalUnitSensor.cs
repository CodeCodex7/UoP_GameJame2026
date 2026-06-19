using AI.Goap.UnitAI.Behaviors;
using CrashKonijn.Agent.Core;
using CrashKonijn.Goap.Core;
using CrashKonijn.Goap.Runtime;

namespace AI.Goap.UnitAI.Sensors
{
    public class HasRivalUnitSensor : LocalWorldSensorBase
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
                !Services.TryResolve<GameDataStore>(out var gameDataStore))
            {
                return 0;
            }

            return gameDataStore.TryGetClosestRivalUnit(brain, brain.AttackSearchRadius, out _) ? 1 : 0;
        }
    }
}
