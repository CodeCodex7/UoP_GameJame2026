using AI.Goap.UnitAI.Behaviors;
using UnityEngine;

public class GameManager : MonoService<GameManager>
{
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
}
