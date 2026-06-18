using System.Collections.Generic;
using UnityEngine;

namespace AI.Goap.UnitAI.Behaviors
{
    [DefaultExecutionOrder(-10000)]
    public class GameDataStore : MonoService<GameDataStore>
    {
        private const string ServiceObjectName = "GameDataStore";

        private readonly HashSet<IResource> resources = new HashSet<IResource>();
        private readonly HashSet<IStorageBuilding> storageBuildings = new HashSet<IStorageBuilding>();
        private readonly HashSet<IUnit> units = new HashSet<IUnit>();

        public IReadOnlyCollection<IResource> Resources => resources;
        public IReadOnlyCollection<IStorageBuilding> StorageBuildings => storageBuildings;
        public IReadOnlyCollection<IUnit> Units => units;

        private void Awake()
        {
            DontDestroyOnLoad(gameObject);
            RegisterService();
        }

        private void OnDestroy()
        {
            UnregisterService();
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void CreateServiceBeforeSceneLoad()
        {
            if (Services.TryResolve<GameDataStore>(out _))
            {
                return;
            }

            var serviceObject = new GameObject(ServiceObjectName);
            serviceObject.AddComponent<GameDataStore>();
        }

        public void RegisterResource(IResource resource)
        {
            if (resource == null)
            {
                return;
            }

            resources.Add(resource);
        }

        public void UnregisterResource(IResource resource)
        {
            if (resource == null)
            {
                return;
            }

            resources.Remove(resource);
        }

        public void RegisterStorage(IStorageBuilding storageBuilding)
        {
            if (storageBuilding == null)
            {
                return;
            }

            storageBuildings.Add(storageBuilding);
        }

        public void UnregisterStorage(IStorageBuilding storageBuilding)
        {
            if (storageBuilding == null)
            {
                return;
            }

            storageBuildings.Remove(storageBuilding);
        }

        public void RegisterUnit(IUnit unit)
        {
            if (unit == null)
            {
                return;
            }

            units.Add(unit);
        }

        public void UnregisterUnit(IUnit unit)
        {
            if (unit == null)
            {
                return;
            }

            units.Remove(unit);
        }

        public int GetStoredAmount(ResourceTypes resourceType)
        {
            var total = 0;

            foreach (var storageBuilding in storageBuildings)
            {
                if (storageBuilding == null)
                {
                    continue;
                }

                total += storageBuilding.GetStoredAmount(resourceType);
            }

            return total;
        }

        public bool TryRemoveStoredResource(ResourceTypes resourceType, int amount, out ResourceStack removedResource)
        {
            removedResource = default;

            if (amount <= 0 || GetStoredAmount(resourceType) < amount)
            {
                return false;
            }

            var remainingAmount = amount;

            foreach (var storageBuilding in storageBuildings)
            {
                if (storageBuilding == null || remainingAmount <= 0)
                {
                    continue;
                }

                var availableAmount = storageBuilding.GetStoredAmount(resourceType);

                if (availableAmount <= 0)
                {
                    continue;
                }

                var amountToRemove = Mathf.Min(remainingAmount, availableAmount);

                if (!storageBuilding.TryRemove(resourceType, amountToRemove, out _))
                {
                    continue;
                }

                remainingAmount -= amountToRemove;
            }

            removedResource = new ResourceStack(resourceType, amount);
            return remainingAmount == 0;
        }

        public bool TryGetClosestResource(Vector3 position, float radius, out IResource closestResource)
        {
            closestResource = null;
            var closestDistanceSqr = radius * radius;

            foreach (var resource in resources)
            {
                if (resource == null || resource.IsDepleted || resource.Transform == null)
                {
                    continue;
                }

                var distanceSqr = (resource.Transform.position - position).sqrMagnitude;

                if (distanceSqr > closestDistanceSqr)
                {
                    continue;
                }

                closestDistanceSqr = distanceSqr;
                closestResource = resource;
            }

            return closestResource != null;
        }

        public bool TryGetClosestStorage(Vector3 position, out IStorageBuilding closestStorage)
        {
            closestStorage = null;
            var closestDistanceSqr = float.MaxValue;

            foreach (var storageBuilding in storageBuildings)
            {
                if (storageBuilding == null || storageBuilding.Transform == null)
                {
                    continue;
                }

                var distanceSqr = (storageBuilding.Transform.position - position).sqrMagnitude;

                if (distanceSqr > closestDistanceSqr)
                {
                    continue;
                }

                closestDistanceSqr = distanceSqr;
                closestStorage = storageBuilding;
            }

            return closestStorage != null;
        }

        public bool TryGetClosestStorageForResource(Vector3 position, ResourceStack resourceStack, out IStorageBuilding closestStorage)
        {
            closestStorage = null;
            var closestDistanceSqr = float.MaxValue;

            foreach (var storageBuilding in storageBuildings)
            {
                if (storageBuilding == null || storageBuilding.Transform == null || !storageBuilding.CanAccept(resourceStack))
                {
                    continue;
                }

                var distanceSqr = (storageBuilding.Transform.position - position).sqrMagnitude;

                if (distanceSqr > closestDistanceSqr)
                {
                    continue;
                }

                closestDistanceSqr = distanceSqr;
                closestStorage = storageBuilding;
            }

            return closestStorage != null;
        }

        public bool TryGetClosestRivalUnit(IUnit attacker, float radius, out IUnit closestRivalUnit)
        {
            closestRivalUnit = null;

            if (attacker == null || attacker.Transform == null)
            {
                return false;
            }

            var attackerTeam = attacker as ITeam;
            var closestDistanceSqr = radius * radius;
            var position = attacker.Transform.position;

            foreach (var unit in units)
            {
                if (unit == null || unit == attacker || !unit.IsAlive || unit.Transform == null)
                {
                    continue;
                }

                if (attackerTeam != null &&
                    unit is ITeam unitTeam &&
                    string.Equals(attackerTeam.TeamId, unitTeam.TeamId, System.StringComparison.Ordinal))
                {
                    continue;
                }

                var distanceSqr = (unit.Transform.position - position).sqrMagnitude;

                if (distanceSqr > closestDistanceSqr)
                {
                    continue;
                }

                closestDistanceSqr = distanceSqr;
                closestRivalUnit = unit;
            }

            return closestRivalUnit != null;
        }
    }
}
