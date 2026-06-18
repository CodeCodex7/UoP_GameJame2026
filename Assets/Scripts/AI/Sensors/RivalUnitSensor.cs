using AI.Goap.UnitAI.Behaviors;
using CrashKonijn.Agent.Core;
using CrashKonijn.Goap.Runtime;

namespace AI.Goap.UnitAI.Sensors
{
    public class RivalUnitSensor : LocalTargetSensorBase
    {
        public override void Created()
        {
        }

        public override void Update()
        {
        }

        public override ITarget Sense(IActionReceiver agent, IComponentReference references, ITarget existingTarget)
        {
            if (!agent.Transform.TryGetComponent<UnitAIBrain>(out var brain) ||
                !Services.TryResolve<GameDataStore>(out var gameDataStore) ||
                !gameDataStore.TryGetClosestRivalUnit(brain, brain.AttackSearchRadius, out var rivalUnit))
            {
                return null;
            }

            return new TransformTarget(rivalUnit.Transform);
        }
    }
}
