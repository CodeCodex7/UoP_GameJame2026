using CrashKonijn.Agent.Core;
using CrashKonijn.Goap.Runtime;
using AI.Goap.UnitAI.Behaviors;
using UnityEngine;
using UnityEngine.AI;

namespace AI.Goap.UnitAI.Sensors
{
    public static class InteractionTargetUtility
    {
        private const float DefaultTargetRadius = 1.5f;
        private const float ExtraDistance = 1.25f;
        private const float NavMeshSampleRadius = 2f;
        private const float OccupiedRadius = 1.25f;
        private const int CandidateCount = 16;

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
            var desiredPosition = FindUnblockedPoint(agent, targetPosition, direction, distance);

            return new PositionTarget(desiredPosition);
        }

        private static Vector3 FindUnblockedPoint(IActionReceiver agent, Vector3 targetPosition, Vector3 preferredDirection, float distance)
        {
            var bestPosition = targetPosition + preferredDirection * distance;
            var bestScore = float.MinValue;

            for (var i = 0; i < CandidateCount; i++)
            {
                var angle = (360f / CandidateCount) * i;
                var direction = Quaternion.Euler(0f, angle, 0f) * preferredDirection;
                var candidate = targetPosition + direction * distance;

                if (!NavMesh.SamplePosition(candidate, out var hit, NavMeshSampleRadius, NavMesh.AllAreas))
                {
                    continue;
                }

                var score = Vector3.Dot(direction, preferredDirection) * 2f;
                score += GetUnitClearanceScore(agent, hit.position);

                if (score <= bestScore)
                {
                    continue;
                }

                bestScore = score;
                bestPosition = hit.position;
            }

            if (bestScore > float.MinValue)
            {
                return bestPosition;
            }

            return NavMesh.SamplePosition(bestPosition, out var fallbackHit, NavMeshSampleRadius, NavMesh.AllAreas)
                ? fallbackHit.position
                : bestPosition;
        }

        private static float GetUnitClearanceScore(IActionReceiver agent, Vector3 position)
        {
            if (!Services.TryResolve<GameDataStore>(out var gameDataStore))
            {
                return 0f;
            }

            var score = 0f;

            foreach (var unit in gameDataStore.Units)
            {
                if (unit == null || unit.Transform == null || ReferenceEquals(unit.Transform, agent.Transform))
                {
                    continue;
                }

                var distance = Vector3.Distance(position, unit.Transform.position);

                if (distance < OccupiedRadius)
                {
                    score -= 10f;
                    continue;
                }

                score += Mathf.Min(distance, 4f) * 0.1f;
            }

            return score;
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
