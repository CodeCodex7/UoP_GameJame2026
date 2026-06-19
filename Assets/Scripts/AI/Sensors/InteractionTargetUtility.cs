using CrashKonijn.Agent.Core;
using CrashKonijn.Goap.Runtime;
using UnityEngine;
using UnityEngine.AI;

namespace AI.Goap.UnitAI.Sensors
{
    public static class InteractionTargetUtility
    {
        private const float DefaultTargetRadius = 1.5f;
        private const float ExtraDistance = 1.25f;
        private const float NavMeshSampleRadius = 2f;

        public static ITarget CreateAroundTarget(IActionReceiver agent, Transform target)
        {
            if (agent == null || target == null)
            {
                return null;
            }

            var targetPosition = target.position;
            var direction = agent.Transform.position - targetPosition;
            direction.y = 0f;

            if (direction.sqrMagnitude < 0.01f)
            {
                direction = -target.forward;
                direction.y = 0f;
            }

            direction.Normalize();

            var distance = GetTargetRadius(target) + ExtraDistance;
            var desiredPosition = targetPosition + direction * distance;

            if (NavMesh.SamplePosition(desiredPosition, out var hit, NavMeshSampleRadius, NavMesh.AllAreas))
            {
                desiredPosition = hit.position;
            }

            return new PositionTarget(desiredPosition);
        }

        private static float GetTargetRadius(Transform target)
        {
            var colliders = target.GetComponentsInChildren<Collider>();

            if (colliders.Length == 0)
            {
                return DefaultTargetRadius;
            }

            var bounds = colliders[0].bounds;

            for (var i = 1; i < colliders.Length; i++)
            {
                bounds.Encapsulate(colliders[i].bounds);
            }

            return Mathf.Max(bounds.extents.x, bounds.extents.z, DefaultTargetRadius);
        }
    }
}
