using AI.Goap.UnitAI.Behaviors;
using UnityEngine;
using UnityEngine.Events;

public class GameManager : MonoService<GameManager>
{
    [SerializeField] private UnityEvent onGameOver;
    [SerializeField] private UnityEvent onGameWon;

    public bool IsGameOver { get; private set; }
    public bool IsGameWon { get; private set; }

    private void Awake()
    {
        RegisterService();
    }

    private void OnDestroy()
    {
        UnregisterService();
    }

    public bool TryRemoveFromStorageOverall(ResourceTypes resourceType, int amount)
    {
        if (!Services.TryResolve<GameDataStore>(out var gameDataStore))
        {
            return false;
        }

        return gameDataStore.TryRemoveStoredResource(resourceType, amount, out _);
    }

    public bool TryRemoveResourceCost(ResourceCost cost)
    {
        if (!TryGetGameDataStore(out var gameDataStore) || !CanAffordResourceCost(cost))
        {
            return false;
        }

        return TryRemoveIfNeeded(gameDataStore, ResourceTypes.Metal, cost.metal) &&
               TryRemoveIfNeeded(gameDataStore, ResourceTypes.Wood, cost.wood) &&
               TryRemoveIfNeeded(gameDataStore, ResourceTypes.Mushrooms, cost.mushrooms);
    }

    public bool CanAffordResourceCost(ResourceCost cost)
    {
        if (!TryGetGameDataStore(out var gameDataStore))
        {
            return false;
        }

        return cost.metal >= 0 &&
               cost.wood >= 0 &&
               cost.mushrooms >= 0 &&
               gameDataStore.GetStoredAmount(ResourceTypes.Metal) >= cost.metal &&
               gameDataStore.GetStoredAmount(ResourceTypes.Wood) >= cost.wood &&
               gameDataStore.GetStoredAmount(ResourceTypes.Mushrooms) >= cost.mushrooms;
    }

    private static bool TryRemoveIfNeeded(GameDataStore gameDataStore, ResourceTypes resourceType, int amount)
    {
        return amount <= 0 || gameDataStore.TryRemoveStoredResource(resourceType, amount, out _);
    }

    private static bool TryGetGameDataStore(out GameDataStore gameDataStore)
    {
        return Services.TryResolve(out gameDataStore);
    }

    public void GameOver()
    {
        if (IsGameOver || IsGameWon)
        {
            return;
        }
        
        print("GameOver");
        IsGameOver = true;
        onGameOver?.Invoke();
    }

    public void WinGame()
    {
        if (IsGameOver || IsGameWon)
        {
            return;
        }
        print("GameWon");
        IsGameWon = true;
        onGameWon?.Invoke();
    }

    public void ResetGameOver()
    {
        IsGameOver = false;
        IsGameWon = false;
    }
}
