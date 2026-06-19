using AI.Goap.UnitAI.Behaviors;

public class CostChecker : MonoService<CostChecker>
{
    private GameDataStore m_dataStore;

    private void Awake()
    {
        RegisterService();
    }

    private void OnDestroy()
    {
        UnregisterService();
    }
    
    private void Start()
    {
        m_dataStore = Services.Resolve<GameDataStore>();
    }   

    public bool HasFunds(ISpawnable spawnable)
    {
        if (spawnable == null)
        {
            return false;
        }

        if (!TryGetDataStore(out var dataStore))
        {
            return false;
        }

        return
            dataStore.GetStoredAmount(ResourceTypes.Metal) >= spawnable.Cost.metal &&
            dataStore.GetStoredAmount(ResourceTypes.Wood) >= spawnable.Cost.wood &&
            dataStore.GetStoredAmount(ResourceTypes.Mushrooms) >= spawnable.Cost.mushrooms;
    }

    public bool TrySpendCost(ISpawnable spawnable)
    {
        if (!HasFunds(spawnable) || !TryGetDataStore(out var dataStore))
        {
            return false;
        }

        return TryRemoveIfNeeded(dataStore, ResourceTypes.Metal, spawnable.Cost.metal) &&
               TryRemoveIfNeeded(dataStore, ResourceTypes.Wood, spawnable.Cost.wood) &&
               TryRemoveIfNeeded(dataStore, ResourceTypes.Mushrooms, spawnable.Cost.mushrooms);
    }

    private bool TryGetDataStore(out GameDataStore dataStore)
    {
        if (m_dataStore != null)
        {
            dataStore = m_dataStore;
            return true;
        }

        if (Services.TryResolve<GameDataStore>(out m_dataStore))
        {
            dataStore = m_dataStore;
            return true;
        }

        dataStore = null;
        return false;
    }

    private static bool TryRemoveIfNeeded(GameDataStore dataStore, ResourceTypes resourceType, int amount)
    {
        return amount <= 0 || dataStore.TryRemoveStoredResource(resourceType, amount, out _);
    }
}
