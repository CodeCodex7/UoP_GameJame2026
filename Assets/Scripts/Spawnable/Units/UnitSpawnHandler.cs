using UnityEngine;

public class UnitSpawnHandler : BaseSpawnHandler
{
    private SpawnableBarracks m_currentSpawner;

    protected override void Start()
    {
        base.Start();
    }

    protected override void StartPlacement(Spawnable spawnable)
    {
        if (!Services.TryResolve<CostChecker>(out var costChecker))
        {
            Debug.LogWarning($"Could not spawn {spawnable.DisplayName}. CostChecker service is not registered.");
            FinalisePlacement();
            return;
        }

        if (!costChecker.TrySpendCost(spawnable))
        {
            Debug.LogWarning($"Could not spawn {spawnable.DisplayName}. Not enough resources.");
            FinalisePlacement();
            return;
        }

        var spawnTransform = m_currentSpawner != null ? m_currentSpawner.transform : transform;
        Instantiate(spawnable.gameObject, spawnTransform.position, spawnTransform.rotation);
        FinalisePlacement();
    }

    protected override void FinalisePlacement()
    {
        base.FinalisePlacement();
    }

    public void SetCurrentBarracks(SpawnableBarracks currentSpawner)
    {
        m_currentSpawner = currentSpawner;
    }
}
