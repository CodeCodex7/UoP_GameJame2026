using System;

public class UnitSpawnHandler : BaseSpawnHandler
{
    private SpawnableBarracks m_currentSpawner;

    protected override void Start()
    {
        base.Start();
    }

    protected override void StartPlacement(Spawnable spawnable)
    {
        throw new NotImplementedException();
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