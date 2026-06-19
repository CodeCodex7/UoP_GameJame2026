using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class BuildingSpawnHandler : BaseSpawnHandler
{

    public Material GhostMaterial;
    
    private class SpawnData
    {
        public GameObjectPool<Spawnable> pool;
        public List<Spawnable> objects;

        public SpawnData(Spawnable spawnable, Transform parent )
        {
            objects = new List<Spawnable>();
            pool = new GameObjectPool<Spawnable>(spawnable.gameObject, parent, 5);
        }
    }

    private Dictionary<Spawnable, SpawnData> m_spawnDataPerPrefab = new Dictionary<Spawnable, SpawnData>();

    private SpawnData m_currentSpawnDataForSelection;
    private Spawnable m_currentSelection;
    private int m_groundLayer;

    protected override void Start()
    {
        base.Start();
        m_groundLayer = LayerMask.NameToLayer("Ground");
    }

    protected override void StartPlacement(Spawnable spawnable)
    {
        if (!m_spawnDataPerPrefab.TryGetValue(spawnable, out var spawnData))
        {
            spawnData = new SpawnData(spawnable, transform);
            m_spawnDataPerPrefab.Add(spawnable, spawnData);
        }
        
        if (!GetWorldCoordinatesOfObject(out var hitInfo))
        {
            Debug.LogError("Failed to translate current screen position to world position. Giving up");
            FinaliseWithoutPlace();
            return;
        }
        
        m_currentSpawnDataForSelection = spawnData;
        m_currentSelection = spawnData.pool.GetObject(hitInfo.point);

        if (m_currentSelection is SpawnableBuilding building)
            building.SetGhostMode(true, GhostMaterial);
    }

    private void Update()
    {
        if (m_currentSpawnable == null)
            return;

        if (Input.GetMouseButtonDown(1))
        {
            FinaliseWithoutPlace();
            return;
        }

        if (Camera.main == null)
            return;

        bool rayHit = GetWorldCoordinatesOfObject(out var hitInfo);
        if (!rayHit)
        {
            Debug.LogError("Failed to translate current screen position to world position. Giving up");
            FinaliseWithoutPlace();
            return;
        }

        m_currentSelection.transform.position = hitInfo.point;

        if (Input.GetMouseButtonDown(0))
        {
            if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject())
            {
                return;
            }
            
            FinaliseAndPlace();
        }
    }

    private bool GetWorldCoordinatesOfObject(out RaycastHit hitInfo)
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        return Physics.Raycast(ray, out  hitInfo, 1000, 1 << m_groundLayer, QueryTriggerInteraction.Ignore);
    }

    private void FinaliseAndPlace()
    {
        if (!Services.TryResolve<CostChecker>(out var costChecker))
        {
            Debug.LogWarning($"Could not place {m_currentSpawnable.DisplayName}. CostChecker service is not registered.");
            FinaliseWithoutPlace();
            return;
        }

        if (!costChecker.TrySpendCost(m_currentSpawnable))
        {
            Debug.LogWarning($"Could not place {m_currentSpawnable.DisplayName}. Not enough resources.");
            FinaliseWithoutPlace();
            return;
        }

        m_currentSpawnDataForSelection.objects.Add(m_currentSelection);

        if (m_currentSelection is SpawnableBuilding building)
            building.SetGhostMode(false, GhostMaterial);
        m_currentSelection = null;
        m_currentSpawnDataForSelection = null;
        FinalisePlacement();
    }

    private void FinaliseWithoutPlace()
    {
        if (m_currentSelection is SpawnableBuilding building)
            building.SetGhostMode(false, GhostMaterial);

        if (m_currentSelection != null)
            m_currentSpawnDataForSelection?.pool.ReleaseObject(m_currentSelection.gameObject);

        m_currentSelection = null;
        m_currentSpawnDataForSelection = null;
        FinalisePlacement();
    }
    
}
