using UnityEngine;

namespace AI.Goap.UnitAI.Behaviors
{
    [RequireComponent(typeof(UnitAIBrain))]
    public class EnemyMayorGuardBrain : MonoBehaviour
    {
        [SerializeField] private UnitAIBrain mayorOverride;
        [SerializeField] private float followDistance = 5f;
        [SerializeField] private float returnDistance = 7f;
        [SerializeField, Tooltip("Distance from the guard where it can switch from following to attacking an enemy.")]
        private float aggroRadius = 18f;
        [SerializeField, Tooltip("Maximum distance from the mayor that the guard or its target can be before the guard breaks chase.")]
        private float leashDistance = 24f;
        [SerializeField] private float orderRefreshInterval = 0.5f;

        private UnitAIBrain guard;
        private UnitAIBrain currentMayor;
        private IUnit currentTarget;
        private float nextThinkTime;
        private Vector3 lastMoveTarget;

        private void Awake()
        {
            guard = GetComponent<UnitAIBrain>();
        }

        private void Update()
        {
            if (guard == null || !guard.IsAlive)
            {
                return;
            }

            if (!TryGetMayor(out currentMayor))
            {
                guard.CompleteAttackOrder();
                currentTarget = null;
                return;
            }

            if (ShouldBreakChase())
            {
                guard.CompleteAttackOrder();
                currentTarget = null;
                MoveTowardMayor(returnDistance);
                return;
            }

            if (TryGetAttackTarget(out var target))
            {
                AttackTarget(target);
                return;
            }

            if (Time.time < nextThinkTime)
            {
                return;
            }

            nextThinkTime = Time.time + Mathf.Max(0.1f, orderRefreshInterval);
            currentTarget = null;
            FollowMayor();
        }

        private void OnValidate()
        {
            followDistance = Mathf.Max(0.1f, followDistance);
            returnDistance = Mathf.Max(followDistance, returnDistance);
            aggroRadius = Mathf.Max(0.1f, aggroRadius);
            leashDistance = Mathf.Max(aggroRadius, leashDistance);
            orderRefreshInterval = Mathf.Max(0.1f, orderRefreshInterval);
        }

        private bool TryGetMayor(out UnitAIBrain mayor)
        {
            mayor = null;

            if (mayorOverride != null && mayorOverride.IsAlive)
            {
                mayor = mayorOverride;
                return true;
            }

            if (!Services.TryResolve<GameDataStore>(out var gameDataStore))
            {
                return false;
            }

            var closestDistanceSqr = float.MaxValue;

            foreach (var unit in gameDataStore.Units)
            {
                if (unit is not UnitAIBrain candidate ||
                    candidate == guard ||
                    !candidate.IsAlive ||
                    candidate.Unittype != UnitType.EnemyMayor ||
                    !SameTeam(guard, candidate))
                {
                    continue;
                }

                var distanceSqr = (candidate.transform.position - transform.position).sqrMagnitude;

                if (distanceSqr > closestDistanceSqr)
                {
                    continue;
                }

                closestDistanceSqr = distanceSqr;
                mayor = candidate;
            }

            return mayor != null;
        }

        private bool TryGetAttackTarget(out IUnit target)
        {
            target = null;

            if (!Services.TryResolve<GameDataStore>(out var gameDataStore) ||
                !gameDataStore.TryGetClosestRivalUnit(guard, aggroRadius, out var rival))
            {
                return false;
            }

            if (!IsInsideMayorLeash(rival.Transform.position))
            {
                return false;
            }

            target = rival;
            return true;
        }

        private void AttackTarget(IUnit target)
        {
            if (ReferenceEquals(currentTarget, target) &&
                guard.TryGetAttackOrderTarget(out var attackTarget) &&
                ReferenceEquals(attackTarget, target))
            {
                return;
            }

            currentTarget = target;
            lastMoveTarget = Vector3.positiveInfinity;
            guard.AttackOrder(target);
        }

        private bool ShouldBreakChase()
        {
            if (!guard.TryGetAttackOrderTarget(out var target) ||
                !UnitAIBrain.TryGetDamageTransform(target, out var targetTransform))
            {
                currentTarget = null;
                return false;
            }

            return !IsInsideMayorLeash(transform.position) || !IsInsideMayorLeash(targetTransform.position);
        }

        private void FollowMayor()
        {
            var distanceToMayor = Vector3.Distance(transform.position, currentMayor.transform.position);

            if (distanceToMayor <= followDistance)
            {
                return;
            }

            MoveTowardMayor(followDistance);
        }

        private void MoveTowardMayor(float desiredDistance)
        {
            var mayorPosition = currentMayor.transform.position;
            var awayFromMayor = transform.position - mayorPosition;

            if (awayFromMayor.sqrMagnitude < 0.01f)
            {
                awayFromMayor = -currentMayor.transform.forward;
            }

            var targetPosition = mayorPosition + awayFromMayor.normalized * Mathf.Max(1f, desiredDistance * 0.5f);

            if ((lastMoveTarget - targetPosition).sqrMagnitude < 1f)
            {
                return;
            }

            lastMoveTarget = targetPosition;
            guard.MoveOrder(targetPosition);
        }

        private bool IsInsideMayorLeash(Vector3 position)
        {
            return currentMayor != null &&
                   Vector3.Distance(position, currentMayor.transform.position) <= leashDistance;
        }

        private static bool SameTeam(ITeam first, ITeam second)
        {
            return first != null &&
                   second != null &&
                   string.Equals(first.TeamId, second.TeamId, System.StringComparison.Ordinal);
        }
    }
}
