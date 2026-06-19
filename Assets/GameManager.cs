using AI.Goap.UnitAI.Behaviors;
using UnityEngine;
using UnityEngine.Events;

public class GameManager : MonoService<GameManager>
{
    [SerializeField] private UnityEvent onGameOver;

    public bool IsGameOver { get; private set; }

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

    public void GameOver()
    {
        if (IsGameOver)
        {
            return;
        }

        IsGameOver = true;
        onGameOver?.Invoke();
    }

    public void ResetGameOver()
    {
        IsGameOver = false;
    }
}
