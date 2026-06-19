using AI.Goap.UnitAI.Behaviors;
using CrashKonijn.Agent.Core;
using CrashKonijn.Goap.Runtime;

namespace AI.Goap.UnitAI.Sensors
{
    public class AttackOrderSensor : LocalTargetSensorBase
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
                !brain.TryGetAttackOrderTarget(out var target))
            {
                return null;
            }

            return UnitAIBrain.TryGetDamageTransform(target, out var targetTransform)
                ? new TransformTarget(targetTransform)
                : null;
        }
    }
}
