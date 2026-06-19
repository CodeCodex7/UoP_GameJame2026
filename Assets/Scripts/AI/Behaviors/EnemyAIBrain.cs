using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AI.Goap.UnitAI.Behaviors
{
    public class EnemyAIBrain : MonoBehaviour
    {
        [SerializeField] private GameObject enemyUnitPrefab;
        [SerializeField] private Transform spawnPoint;
        [SerializeField] private List<Transform> spawnPoints = new List<Transform>();
        [SerializeField] private float initialSpawnDelay = 180f;
        [SerializeField] private float spawnInterval = 8f;
        [SerializeField] private float targetRecheckInterval = 0.5f;
        [SerializeField] private int maxLivingUnits = 6;
        [SerializeField] private bool spawnOnStart = true;

        private readonly List<UnitAIBrain> livingUnits = new List<UnitAIBrain>();
        private readonly Dictionary<UnitAIBrain, IUnit> assignedLocalTargets = new Dictionary<UnitAIBrain, IUnit>();
        private Coroutine productionRoutine;
        private Coroutine targetingRoutine;
        private int nextSpawnPointIndex;

        private void OnEnable()
        {
            productionRoutine = StartCoroutine(ProduceUnits());
            targetingRoutine = StartCoroutine(RecheckTargets());
        }

        private void OnDisable()
        {
            if (productionRoutine != null)
            {
                StopCoroutine(productionRoutine);
                productionRoutine = null;
            }

            if (targetingRoutine != null)
            {
                StopCoroutine(targetingRoutine);
                targetingRoutine = null;
            }
        }

        private IEnumerator ProduceUnits()
        {
            if (initialSpawnDelay > 0f)
            {
                yield return new WaitForSeconds(initialSpawnDelay);
            }

            if (spawnOnStart)
            {
                SpawnEnemyUnit();
            }

            var wait = new WaitForSeconds(spawnInterval);

            while (enabled)
            {
                yield return wait;

                TrimDeadUnits();

                if (livingUnits.Count >= maxLivingUnits)
                {
                    continue;
                }

                SpawnEnemyUnit();
            }
        }

        private IEnumerator RecheckTargets()
        {
            var wait = new WaitForSeconds(Mathf.Max(0.1f, targetRecheckInterval));

            while (enabled)
            {
                yield return wait;
                TrimDeadUnits();

                if (!Services.TryResolve<GameDataStore>(out var gameDataStore))
                {
                    continue;
                }

                foreach (var unit in livingUnits)
                {
                    if (unit == null || !unit.IsAlive)
                    {
                        continue;
                    }

                    if (gameDataStore.TryGetClosestRivalUnit(unit, unit.AttackSearchRadius, out var localTarget))
                    {
                        AssignLocalTarget(unit, localTarget);
                        continue;
                    }

                    ResumeMayorHunt(unit);
                }
            }
        }

        private void AssignLocalTarget(UnitAIBrain unit, IUnit localTarget)
        {
            if (assignedLocalTargets.TryGetValue(unit, out var assignedTarget) &&
                ReferenceEquals(assignedTarget, localTarget) &&
                unit.TryGetAttackOrderTarget(out var currentAttackTarget) &&
                ReferenceEquals(currentAttackTarget, localTarget))
            {
                return;
            }

            assignedLocalTargets[unit] = localTarget;
            unit.AttackOrder(localTarget);
        }

        private void ResumeMayorHunt(UnitAIBrain unit)
        {
            if (!assignedLocalTargets.Remove(unit))
            {
                return;
            }

            unit.HuntPlayerMayor();
        }

        private void SpawnEnemyUnit()
        {
            if (enemyUnitPrefab == null)
            {
                return;
            }

            var source = GetNextSpawnPoint();
            var unitObject = Instantiate(enemyUnitPrefab, source.position, source.rotation);

            if (!unitObject.TryGetComponent<UnitAIBrain>(out var unit))
            {
                unit = unitObject.GetComponentInChildren<UnitAIBrain>();
            }

            if (unit == null)
            {
                Debug.LogWarning($"{enemyUnitPrefab.name} was spawned by EnemyAIBrain but does not contain a UnitAIBrain.");
                return;
            }

            unit.Unittype = UnitType.Fighter;
            livingUnits.Add(unit);

            StartCoroutine(RequestMayorHuntWhenReady(unit));
        }

        private Transform GetNextSpawnPoint()
        {
            if (spawnPoints != null && spawnPoints.Count > 0)
            {
                for (var i = 0; i < spawnPoints.Count; i++)
                {
                    var index = nextSpawnPointIndex % spawnPoints.Count;
                    nextSpawnPointIndex = (nextSpawnPointIndex + 1) % spawnPoints.Count;

                    if (spawnPoints[index] != null)
                    {
                        return spawnPoints[index];
                    }
                }
            }

            return spawnPoint != null ? spawnPoint : transform;
        }

        private IEnumerator RequestMayorHuntWhenReady(UnitAIBrain unit)
        {
            yield return null;

            if (unit == null || !unit.IsAlive || unit.provider == null)
            {
                yield break;
            }

            unit.HuntPlayerMayor();
        }

        private void TrimDeadUnits()
        {
            for (var i = livingUnits.Count - 1; i >= 0; i--)
            {
                var unit = livingUnits[i];

                if (unit == null || !unit.IsAlive)
                {
                    livingUnits.RemoveAt(i);

                    if (unit != null)
                    {
                        assignedLocalTargets.Remove(unit);
                    }
                }
            }
        }
    }
}
