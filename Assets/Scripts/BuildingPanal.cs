using UnityEngine;

namespace DefaultNamespace
{
    public class BuildingPanal : MonoBehaviour ,IUIScreenContextReceiver
    {
        public void SetSelectionContext(UISelectionContext context)
        {
            Debug.Log(context.SourceObject.name);
        }
    }
}