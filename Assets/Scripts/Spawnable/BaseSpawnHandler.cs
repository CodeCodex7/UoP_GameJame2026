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

    public void InitiateSpawnInteraction(Spawnable spawnable, Action onSpawnHandlerComplete)
    {
        if (!m_costChecker.HasFunds(spawnable))
            return;
        
        m_handlerCompleteCallback = onSpawnHandlerComplete;
        m_currentSpawnable = spawnable;
        
        StartPlacement(spawnable);
    }




    protected abstract void StartPlacement(Spawnable spawnable);

    protected virtual void FinalisePlacement()
    {
        m_currentSpawnable = null;
        m_handlerCompleteCallback = null;
        m_handlerCompleteCallback?.Invoke();
    }
}
