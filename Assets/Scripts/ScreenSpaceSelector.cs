using AI.Goap.UnitAI.Behaviors;
using UnityEngine;

namespace DefaultNamespace
{
    public class ScreenSpaceSelector : MonoBehaviour
    {
        public UnitSelectionController unitSelectionController;

        // Start is called once before the first execution of Update after the MonoBehaviour is created
        void Start()
        {
            if (unitSelectionController == null)
            {
                unitSelectionController = FindFirstObjectByType<UnitSelectionController>();
            }
        }

        // Update is called once per frame
        void Update()
        {
            if (Input.GetMouseButtonDown(1)) // Right mouse button
            {
                if (TryGetMouseWorldHit(out var hit))
                {
                    Debug.Log(hit.point);
                    unitSelectionController?.HandleContextClickForSelectedUnits(hit);
                }
            
            }
        }

        public bool TryGetMouseWorldHit(out RaycastHit hit)
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            return Physics.Raycast(ray, out hit);
        }

        public bool TryGetMouseWorldPosition(out Vector3 position)
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

            if (Physics.Raycast(ray, out RaycastHit hit))
            {
                position = hit.point;
                return true;
            }

            position = Vector3.zero;
            return false;
        }
    }
}
