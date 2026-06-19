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

    public bool HasFunds(Spawnable spawnable)
    {
        return
            m_dataStore.GetStoredAmount(ResourceTypes.Metal) >= spawnable.Cost.metal &&
            m_dataStore.GetStoredAmount(ResourceTypes.Wood) >= spawnable.Cost.wood &&
            m_dataStore.GetStoredAmount(ResourceTypes.Mushrooms) >= spawnable.Cost.mushrooms;
    }
}