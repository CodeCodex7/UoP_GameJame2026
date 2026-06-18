using AI.Goap.UnitAI.Behaviors;
using UnityEngine;

public class StorageBuilding : MonoBehaviour, IStorageBuilding
{
    [SerializeField] private string storageId = "Storage";
    [SerializeField] private UnitInventory inventory;
    [SerializeField] private bool limitToOneResourceType;
    [SerializeField] private ResourceTypes acceptedResourceType;

    public Transform Transform => transform;
    public string StorageId => storageId;
    public UnitInventory Inventory => inventory;
    public bool LimitToOneResourceType => limitToOneResourceType;
    public ResourceTypes AcceptedResourceType => acceptedResourceType;

    private void Awake()
    {
        inventory = GetComponent<UnitInventory>();

        if (inventory == null)
        {
            inventory = gameObject.AddComponent<UnitInventory>();
        }
    }

    private void OnEnable()
    {
        Services.ResolveWhenValid<GameDataStore>(RegisterStorage);
    }

    private void OnDisable()
    {
        if (Services.TryResolve<GameDataStore>(out var gameDataStore))
        {
            gameDataStore.UnregisterStorage(this);
        }
    }

    private void RegisterStorage()
    {
        if (!isActiveAndEnabled)
        {
            return;
        }

        if (Services.TryResolve<GameDataStore>(out var gameDataStore))
        {
            gameDataStore.RegisterStorage(this);
        }
    }

    public bool Deposit(ResourceStack resourceStack)
    {
        if (!CanAccept(resourceStack))
        {
            Debug.Log($"{name} rejected {resourceStack.ResourceName}. This storage only accepts {acceptedResourceType}.");
            return false;
        }

        if (inventory == null)
        {
            inventory = GetComponent<UnitInventory>();
        }

        if (inventory == null || !inventory.TryAdd(resourceStack))
        {
            return false;
        }

        Debug.Log($"Deposited {resourceStack.amount} {resourceStack.ResourceName} into {name}.");
        return true;
    }

    public bool CanAccept(ResourceStack resourceStack)
    {
        return resourceStack.IsValid
               && (!limitToOneResourceType || resourceStack.resourceType == acceptedResourceType);
    }

    public int GetStoredAmount(ResourceTypes resourceType)
    {
        if (inventory == null)
        {
            inventory = GetComponent<UnitInventory>();
        }

        return inventory != null ? inventory.GetAmount(resourceType) : 0;
    }

    public bool TryRemove(ResourceTypes resourceType, int amount, out ResourceStack removedResource)
    {
        removedResource = default;

        if (inventory == null)
        {
            inventory = GetComponent<UnitInventory>();
        }

        return inventory != null && inventory.TryRemoveExact(resourceType, amount, out removedResource);
    }
}
