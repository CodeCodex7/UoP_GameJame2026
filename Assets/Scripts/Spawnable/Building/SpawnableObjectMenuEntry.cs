using System;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Serialization;
using UnityEngine.UI;

[RequireComponent(typeof(Button))]
public class SpawnableObjectMenuEntry : MonoBehaviour
{
    [SerializeField] private RawImage m_displayPicture;
    [SerializeField] private TextMeshProUGUI m_metalCost;
    [SerializeField] private TextMeshProUGUI m_woodCost;
    [SerializeField] private TextMeshProUGUI m_mushroomCost;

    
    private BuildingMenuUI m_parentUI;
    [HideInInspector] public Spawnable Spawnable;

    private CostChecker m_costChecker;
    private Button m_button;
    
    private void Start()
    {
        m_button = GetComponent<Button>();
        m_costChecker = Services.Resolve<CostChecker>();
    }

    public void Init(BuildingMenuUI parentUI, Spawnable spawnable)
    {
        this.m_parentUI = parentUI;
        this.Spawnable = spawnable;

        m_displayPicture.texture = spawnable.UIImage;
        m_metalCost.text = spawnable.Cost.metal.ToString();
        m_woodCost.text = spawnable.Cost.wood.ToString();
        m_mushroomCost.text = spawnable.Cost.mushrooms.ToString();
        
    }


    private void Update()
    {
        m_button.interactable = m_costChecker.HasFunds(Spawnable);
    }

    public void PointerOver(BaseEventData eventData)
    {
        m_parentUI.PointerOverEntry(this, eventData as PointerEventData);
    }

    public void PointerExit(BaseEventData eventData)
    {
        m_parentUI.PointerExitEntry(this, eventData as PointerEventData);

    }
    
    public void PointerDown(BaseEventData eventData)
    {
        m_parentUI.PointerDown(this, eventData as PointerEventData);
    }

    
    
}