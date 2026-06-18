using UnityEngine;

namespace AI.Goap.UnitAI.Behaviors
{
    public class UnitSelectionController : MonoBehaviour
    {
        [SerializeField] private Camera selectionCamera;
        [SerializeField] private LayerMask selectionMask = ~0;
        [SerializeField] private bool deselectWhenClickingEmpty = true;
        [SerializeField] private bool toggleSelectedUnit = true;
        [SerializeField] private bool blockNonUnitSelectionWhileUnitSelected = true;
        [SerializeField] private bool interactWithNonUnitsWhileUnitSelected = true;

        public ISelectable Selected { get; private set; }
        public IUnit SelectedUnit => Selected as IUnit;

        private void Awake()
        {
            if (selectionCamera == null)
            {
                selectionCamera = Camera.main;
            }
        }

        private void Update()
        {
            if (!Input.GetMouseButtonDown(0))
            {
                return;
            }

            TrySelectFromMouse();
        }

        public void SelectUnit(IUnit unit)
        {
            Select(unit);
        }

        public void Select(ISelectable selectable)
        {
            if (Selected == selectable)
            {
                return;
            }

            DeselectCurrentUnit();
            Selected = selectable;
            Selected?.Select();
        }

        public void DeselectCurrentUnit()
        {
            Selected?.Deselect();
            Selected = null;
        }

        private void TrySelectFromMouse()
        {
            if (selectionCamera == null)
            {
                return;
            }

            var ray = selectionCamera.ScreenPointToRay(Input.mousePosition);

            if (!Physics.Raycast(ray, out var hit, Mathf.Infinity, selectionMask))
            {
                if (deselectWhenClickingEmpty)
                {
                    DeselectCurrentUnit();
                }

                return;
            }

            var unit = hit.collider.GetComponentInParent<IUnit>();
            var selectable = unit as ISelectable ?? hit.collider.GetComponentInParent<ISelectable>();

            if (SelectedUnit != null && unit != null && SelectedUnit != unit && CanIssueAttackOrder(SelectedUnit, unit))
            {
                SelectedUnit.AttackOrder(unit);
                return;
            }

            if (selectable == null)
            {
                if (deselectWhenClickingEmpty)
                {
                    DeselectCurrentUnit();
                }

                return;
            }

            if (blockNonUnitSelectionWhileUnitSelected && SelectedUnit != null && unit == null)
            {
                if (interactWithNonUnitsWhileUnitSelected)
                {
                    SelectedUnit.HandleContextClick(hit);
                }

                return;
            }

            if (toggleSelectedUnit && Selected == selectable)
            {
                DeselectCurrentUnit();
                return;
            }

            Select(selectable);
        }

        private static bool CanIssueAttackOrder(IUnit attacker, IUnit target)
        {
            if (attacker == null || target == null || !attacker.IsAlive || !target.IsAlive)
            {
                return false;
            }

            if (attacker is ITeam attackerTeam &&
                target is ITeam targetTeam &&
                string.Equals(attackerTeam.TeamId, targetTeam.TeamId, System.StringComparison.Ordinal))
            {
                return false;
            }

            return true;
        }
    }
}
