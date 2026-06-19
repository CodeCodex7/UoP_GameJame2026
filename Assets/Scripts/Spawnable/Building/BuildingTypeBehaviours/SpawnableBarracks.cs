using UnityEngine;

public class SpawnableBarracks : SpawnableBuilding
{

    private BuildingMenuUI m_menuReference;

    private void Start()
    {
        m_menuReference = Services.Resolve<BuildingMenuUI>();
    }
    
    public void OpenMenu()
    {
        var spawnInfo = Services.Resolve<SpawnManager>().GetSpawnForType(SpawnableType.Unit);
        if (spawnInfo.SpawnHandler is UnitSpawnHandler unitSpawnHandler)
            unitSpawnHandler.SetCurrentBarracks(this);
        m_menuReference.Initialise(spawnInfo, OnMenuUICloseCallback);
    }

    private void OnMenuUICloseCallback()
    {
    }
}
