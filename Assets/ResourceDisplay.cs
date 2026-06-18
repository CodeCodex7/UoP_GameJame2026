using AI.Goap.UnitAI.Behaviors;
using TMPro;
using UnityEngine;

public class ResourceDisplay : MonoBehaviour
{
    
    public TextMeshProUGUI AmountText;

    private int _totalWood,_totalMetal,_totalMushrooms;


    private void Awake()
    {
        _totalWood = 0;
        _totalMetal = 0;
        _totalMushrooms = 0;
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
        GetResourcesAmmounts();
        
        AmountText.text = $"Wood:{_totalWood}/ Metal{_totalMetal}/ Mushrooms{_totalMushrooms}";
    }

    private void GetResourcesAmmounts()
    {
        if (!Services.TryResolve<GameDataStore>(out var gameDataStore))
        {
            return;
        }
            
        _totalWood = 0;
        _totalMetal = 0;
        _totalMushrooms = 0;
        
        
        foreach (var storage in gameDataStore.StorageBuildings)
        {
            var inv =  storage.Transform.GetComponent<UnitInventory>();

            foreach (var resource in inv.Resources)
            {
                switch (resource.resourceType)
                {
                    case ResourceTypes.Wood:
                        _totalWood += resource.amount;
                        break;
                    
                    case ResourceTypes.Metal:
                        _totalMetal += resource.amount;
                        break;
                    
                    case ResourceTypes.Mushrooms:
                        _totalMushrooms += resource.amount;
                        break;
                    
                }
            }
        }
    }

}
