using System;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting.Antlr3.Runtime.Tree;
using UnityEngine;
using UnityEngine.Pool;


public class BuildingMenuUI : MonoBehaviour
{

    public SpawnableObjectMenuEntry m_menuEntryPrefab;
    public RectTransform m_menuEntryParentObject;
    
    private SpawnManager m_spawnManagerRef;
    private GameObjectPool<SpawnableObjectMenuEntry> m_menuEntryPool;
    private List<SpawnableObjectMenuEntry> m_entries;
    
    private void Start()
    {
        m_spawnManagerRef = Services.Resolve<SpawnManager>();
        m_menuEntryPool = new GameObjectPool<SpawnableObjectMenuEntry>(m_menuEntryPrefab.gameObject, m_menuEntryParentObject, 5, 10);
    }

    private void OnEnable()
    {
        m_entries = m_spawnManagerRef.SpawnableBuildings.Select(x =>
        {
            SpawnableObjectMenuEntry ret = m_menuEntryPool.GetObject(Vector3.zero);
            ret.Init(x);
            return ret;
        }).ToList();
    }
}