using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class BuildingMenuUI : MonoService<BuildingMenuUI>
{
    [SerializeField] private SpawnableObjectMenuEntry m_menuEntryPrefab;
    [SerializeField] private RectTransform m_menuEntryParentObject;
    [SerializeField] private GridLayoutGroup m_menuEntryGridLayout;
    [SerializeField] private RectTransform m_spawnMenuUI;
    [SerializeField] private TextMeshProUGUI m_detailDescription;
    [SerializeField] private TextMeshProUGUI m_metalCost;
    [SerializeField] private TextMeshProUGUI m_woodCost;
    [SerializeField] private TextMeshProUGUI m_mushroomCost;

    
    private Action m_closeCallback;
    private List<SpawnableObjectMenuEntry> m_entries;

    private GameObjectPool<SpawnableObjectMenuEntry> m_menuEntryPool;
    private bool m_menuIsOpen;
    
    private void Awake()
    {
        RegisterService();
        m_menuEntryPool =
            new GameObjectPool<SpawnableObjectMenuEntry>(m_menuEntryPrefab.gameObject, m_menuEntryParentObject, 5, 10);
        CloseMenu();
    }

    private void OnDestroy()
    {
        UnregisterService();
    }

    public void Initialise(SpawnInfo spawnInfo, Action closeCallback)
    {
        if (m_menuIsOpen)
            CloseMenu();

        m_menuIsOpen = true;
        m_closeCallback = closeCallback;
        m_spawnInfo = spawnInfo;
        m_entries = spawnInfo.Catalogue.Select(x =>
        {
            var ret = m_menuEntryPool.GetObject(Vector3.zero);
            ret.Init(this, x);
            return ret;
        }).ToList();
        m_spawnMenuUI.gameObject.SetActive(true);
        SetupDetailView(null);
    }

    public void CloseMenu()
    {
        if (m_entries != null)
            foreach (var spawnableObjectMenuEntry in m_entries)
                m_menuEntryPool.ReleaseObject(spawnableObjectMenuEntry.gameObject);
        
        m_spawnMenuUI.gameObject.SetActive(false);
        m_closeCallback?.Invoke();
        m_closeCallback = null;
        m_menuIsOpen = false;
    }

    public void SetupDetailView(SpawnableObjectMenuEntry entry)
    {
        m_detailDescription.text = $"<b>{entry?.Spawnable.DisplayName}</b> \n{entry?.Spawnable.Description}";
        m_metalCost.text = $"<b>Metal: </b>{entry?.Spawnable.Cost.metal.ToString()}";
        m_woodCost.text = $"<b>Wood: </b>{entry?.Spawnable.Cost.wood.ToString()}";
        m_mushroomCost.text = $"<b>Mushrooms: </b>{entry?.Spawnable.Cost.mushrooms.ToString()}";
    }


    private SpawnableObjectMenuEntry m_currentHover;
    private bool m_hasItemSelected;
    private SpawnInfo m_spawnInfo;

    public void PointerOverEntry(SpawnableObjectMenuEntry entry, PointerEventData eventData)
    {
        m_currentHover = entry;
        if (m_hasItemSelected)
            return;
        
        Debug.Log($"PointerOver {entry.GetHashCode()}");
        SetupDetailView(entry);
    }

    public void PointerExitEntry(SpawnableObjectMenuEntry entry, PointerEventData eventData)
    {
        m_currentHover = null;
        if (m_hasItemSelected)
            return;
        
        Debug.Log($"PointerExitEntry {entry.GetHashCode()}");
        SetupDetailView(null);
    }
    
    public void PointerDown(SpawnableObjectMenuEntry spawnableObjectMenuEntry, PointerEventData eventData)
    {
        Debug.Log($"Selected {spawnableObjectMenuEntry.GetHashCode()}");
        m_hasItemSelected = true;
        
        m_spawnInfo.SpawnHandler.InitiateSpawnInteraction(spawnableObjectMenuEntry.Spawnable, OnSpawnHandlerComplete);
    }

    private void OnSpawnHandlerComplete()
    {
        m_hasItemSelected = false;
        if (m_currentHover != null)
            SetupDetailView(m_currentHover);
    }
}