using System;
using AI.Goap.UnitAI.Behaviors;
using UnityEngine;
using UnityEngine.EventSystems;

public abstract class BaseSpawnHandler : MonoBehaviour
{
    private Action m_handlerCompleteCallback;
    protected Spawnable m_currentSpawnable;
    private CostChecker m_costChecker;


    protected virtual void Start()
    {
        m_costChecker = Services.Resolve<CostChecker>();
    }

    public bool InitiateSpawnInteraction(Spawnable spawnable, Action onSpawnHandlerComplete)
    {
        if (!m_costChecker.HasFunds(spawnable))
            return false;
        
        m_handlerCompleteCallback = onSpawnHandlerComplete;
        m_currentSpawnable = spawnable;
        
        StartPlacement(spawnable);
        return true;
    }




    protected abstract void StartPlacement(Spawnable spawnable);

    protected virtual void FinalisePlacement()
    {
        var handlerCompleteCallback = m_handlerCompleteCallback;

        m_currentSpawnable = null;
        m_handlerCompleteCallback = null;
        handlerCompleteCallback?.Invoke();
    }
}
