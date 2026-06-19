using AI.Goap.UnitAI.Behaviors;
using CrashKonijn.Agent.Core;
using CrashKonijn.Goap.Runtime;

namespace AI.Goap.UnitAI.Sensors
{
    public class HarvestOrderSensor : LocalTargetSensorBase
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
                !brain.TryGetGatherOrderTarget(out var orderedResource))
            {
                return null;
            }

            return InteractionTargetUtility.CreateAroundTarget(agent, orderedResource.Transform);
        }
    }
}
