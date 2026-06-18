using AI.Goap.UnitAI.Behaviors;
using UnityEngine;

namespace DefaultNamespace
{
    public class ResourceNode : MonoBehaviour ,IResource ,IUnitActionTarget
    {
        [SerializeField] private ResourcesBase resource = new ResourcesBase();
        [SerializeField] private int amountRemaining = 100;

        public Transform Transform => transform;
        public ResourcesBase Resource => resource;
        public ResourceTypes ResourceType => resource.ResourceType;
        public string ResourceId => resource.Name;
        public int AmountRemaining => amountRemaining;
        public bool IsDepleted => amountRemaining <= 0;

        private void OnEnable()
        {
            Services.ResolveWhenValid<GameDataStore>(RegisterResource);
        }

        private void OnDisable()
        {
            if (Services.TryResolve<GameDataStore>(out var gameDataStore))
            {
                gameDataStore.UnregisterResource(this);
            }
        }

        private void RegisterResource()
        {
            if (!isActiveAndEnabled)
            {
                return;
            }

            if (Services.TryResolve<GameDataStore>(out var gameDataStore))
            {
                gameDataStore.RegisterResource(this);
            }
        }
        
        public bool TryIssueOrder(IUnit unit, RaycastHit hit)
        {
            unit.GatherOrder(this);
            return true;
        }

        public bool TryHarvest(int requestedAmount, out ResourceStack harvestedResource)
        {
            harvestedResource = default;

            if (IsDepleted || requestedAmount <= 0)
            {
                return false;
            }

            var harvestedAmount = Mathf.Min(requestedAmount, amountRemaining);
            amountRemaining -= harvestedAmount;
            harvestedResource = new ResourceStack(resource.ResourceType, harvestedAmount);
            return harvestedAmount > 0;
        }
    }
}
