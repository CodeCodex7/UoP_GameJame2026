using AI.Goap.UnitAI.Behaviors;
using UnityEngine;
using UnityEngine.Events;

namespace DefaultNamespace
{
    public class ResourceNode : MonoBehaviour ,IResource ,IUnitActionTarget
    {
        [SerializeField] private ResourcesBase resource = new ResourcesBase();
        [SerializeField] private int minAmount =  5;
        [SerializeField] private int maxAmount = 100;
        [SerializeField] private int amountRemaining = 100;
        [SerializeField] private UnityEvent onDepleted;
        [SerializeField] private bool deleteOnDepleted;

        
        
        private bool hasHandledDepletion;

        public Transform Transform => transform;
        public ResourcesBase Resource => resource;
        public ResourceTypes ResourceType => resource.ResourceType;
        public string ResourceId => resource.Name;
        public int AmountRemaining => amountRemaining;
        public bool IsDepleted => amountRemaining <= 0;

        private void Awake()
        {
            RandomiseAmount();
        }

        private void OnValidate()
        {
            minAmount = Mathf.Max(0, minAmount);
            maxAmount = Mathf.Max(minAmount, maxAmount);
        }

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

        public void RandomiseAmount()
        {
            amountRemaining = Random.Range(minAmount, maxAmount + 1);
            hasHandledDepletion = amountRemaining <= 0;
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

            if (IsDepleted)
            {
                HandleDepleted();
            }

            return harvestedAmount > 0;
        }

        private void HandleDepleted()
        {
            if (hasHandledDepletion)
            {
                return;
            }

            hasHandledDepletion = true;
            onDepleted?.Invoke();

            if (deleteOnDepleted)
            {
                Destroy(gameObject);
            }
        }
    }
}
