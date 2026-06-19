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
        m_menuReference.Initialise(Services.Resolve<SpawnManager>().GetSpawnForType(SpawnableType.Unit), OnMenuUICloseCallback);
    }

    private void OnMenuUICloseCallback()
    {
    }
}
