using System;
using AI.Goap.UnitAI.Behaviors;
using UnityEngine;
using UnityEngine.EventSystems;

public abstract class BaseSpawnHandler : MonoBehaviour
{
    private Action m_handlerCompleteCallback;
    protected Spawnable m_currentSpawnable;

    public void InitiateSpawnInteraction(Spawnable spawnable, Action onSpawnHandlerComplete)
    {
        if (!HasFunds(spawnable))
            return;
        
        m_handlerCompleteCallback = onSpawnHandlerComplete;
        m_currentSpawnable = spawnable;
        
        StartPlacement(spawnable);
    }


    private bool HasFunds(Spawnable spawnable)
    {
        return true;
        var gameDataStore = Services.Resolve<GameDataStore>();
        return
            gameDataStore.GetStoredAmount(ResourceTypes.Metal) >= spawnable.Cost.metal &&
            gameDataStore.GetStoredAmount(ResourceTypes.Wood) >= spawnable.Cost.wood &&
            gameDataStore.GetStoredAmount(ResourceTypes.Mushrooms) >= spawnable.Cost.mushrooms;
    }

    protected abstract void StartPlacement(Spawnable spawnable);

    protected virtual void FinalisePlacement()
    {
        m_currentSpawnable = null;
        m_handlerCompleteCallback = null;
        m_handlerCompleteCallback?.Invoke();
    }
}
