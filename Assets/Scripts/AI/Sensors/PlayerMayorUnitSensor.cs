using AI.Goap.UnitAI.Behaviors;
using CrashKonijn.Agent.Core;
using CrashKonijn.Goap.Runtime;

namespace AI.Goap.UnitAI.Sensors
{
    public class PlayerMayorUnitSensor : LocalTargetSensorBase
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
                !brain.IsEnemyTeam() ||
                !Services.TryResolve<GameDataStore>(out var gameDataStore) ||
                !gameDataStore.TryGetClosestRivalUnitOfType(brain, UnitType.Mayor, float.PositiveInfinity, out var mayorUnit))
            {
                return null;
            }

            return new TransformTarget(mayorUnit.Transform);
        }
    }
}
