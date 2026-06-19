using AI.Goap.UnitAI.Behaviors;
using CrashKonijn.Agent.Core;
using CrashKonijn.Goap.Runtime;
using UnityEngine;
using UnityEngine.AI;

namespace AI.Goap.UnitAI.Sensors
{
    public class FleeTargetSensor : LocalTargetSensorBase
    {
        private const float NavMeshSampleRadius = 5f;

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
                !gameDataStore.TryGetClosestRivalUnit(brain, brain.FleeSearchRadius, out var threat) ||
                threat.Transform == null)
            {
                return new PositionTarget(agent.Transform.position);
            }

            var awayDirection = agent.Transform.position - threat.Transform.position;

            if (awayDirection.sqrMagnitude < 0.01f)
            {
                var random = Random.insideUnitCircle.normalized;
                awayDirection = new Vector3(random.x, 0f, random.y);
            }

            var targetPosition = agent.Transform.position + awayDirection.normalized * brain.FleeDistance;

            if (NavMesh.SamplePosition(targetPosition, out var hit, NavMeshSampleRadius, NavMesh.AllAreas))
            {
                targetPosition = hit.position;
            }

            return new PositionTarget(targetPosition);
        }
    }
}
