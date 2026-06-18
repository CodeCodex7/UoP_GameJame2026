using AI.Goap.UnitAI.Behaviors;
using CrashKonijn.Agent.Core;
using CrashKonijn.Goap.Runtime;

namespace AI.Goap.UnitAI.Sensors
{
    public class StorageSensor : LocalTargetSensorBase
    {
        public override void Created()
        {
        }

        public override void Update()
        {
        }

        public override ITarget Sense(IActionReceiver agent, IComponentReference references, ITarget existingTarget)
        {
            var brain = agent.Transform.GetComponent<UnitAIBrain>();

            if (brain == null || brain.Inventory == null || !brain.Inventory.TryPeekFirst(out var carriedResource))
            {
                return null;
            }

            if (!Services.TryResolve<GameDataStore>(out var gameDataStore) ||
                !gameDataStore.TryGetClosestStorageForResource(agent.Transform.position, carriedResource, out var closestStorage))
            {
                return null;
            }

            return new TransformTarget(closestStorage.Transform);
        }
    }
}
