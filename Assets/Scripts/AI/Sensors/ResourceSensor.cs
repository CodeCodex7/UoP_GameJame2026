using AI.Goap.UnitAI.Behaviors;
using CrashKonijn.Agent.Core;
using CrashKonijn.Goap.Runtime;
using UnityEngine;

namespace AI.Goap.UnitAI.Sensors
{
    public class ResourceSensor : LocalTargetSensorBase
    {
        public override void Created()
        {
        }

        public override void Update()
        {
        }

        public override ITarget Sense(IActionReceiver agent, IComponentReference references, ITarget existingTarget)
        {
            var radius = float.MaxValue;

            if (agent.Transform.TryGetComponent<UnitAIBrain>(out var brain))
            {
                if (brain.TryGetGatherOrderTarget(out var orderedResource))
                {
                    return new TransformTarget(orderedResource.Transform);
                }

                radius = brain.AutoHarvestRadius;
            }

            if (!Services.TryResolve<GameDataStore>(out var gameDataStore) ||
                !gameDataStore.TryGetClosestResource(agent.Transform.position, radius, out var closestResource))
            {
                return null;
            }

            return new TransformTarget(closestResource.Transform);
        }
    }
}
