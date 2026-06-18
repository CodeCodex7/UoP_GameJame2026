using System;
using UnityEngine;
using TMPro;
namespace DefaultNamespace
{
    public class BuildingUI : MonoBehaviour
    {
        public TextMeshProUGUI Name;
        
        private void OnEnable()
        {
            Name.text = Services.Resolve<UIScreenController>().CurrentSelectionContext.SourceObject.name;
        }
    }
}