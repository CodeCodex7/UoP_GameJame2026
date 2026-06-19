using System.Collections.Generic;
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
        [SerializeField] private string selectableTeamId = "Player";
        [SerializeField] private float dragSelectionThreshold = 8f;
        [SerializeField] private Color dragFillColor = new Color(0.2f, 0.6f, 1f, 0.15f);
        [SerializeField] private Color dragBorderColor = new Color(0.2f, 0.6f, 1f, 0.8f);

        public ISelectable Selected { get; private set; }
        public IUnit SelectedUnit
        {
            get
            {
                CleanupSelection();
                return selectedUnits.Count > 0 ? selectedUnits[0] : Selected as IUnit;
            }
        }

        public IReadOnlyList<IUnit> SelectedUnits
        {
            get
            {
                CleanupSelection();
                return selectedUnits;
            }
        }

        private readonly List<IUnit> selectedUnits = new List<IUnit>();
        private Vector2 dragStartPosition;
        private Vector2 dragCurrentPosition;
        private bool isDraggingSelection;

        private void Awake()
        {
            if (selectionCamera == null)
            {
                selectionCamera = Camera.main;
            }
        }

        private void Update()
        {
            CleanupSelection();

            if (Input.GetMouseButtonDown(0))
            {
                dragStartPosition = Input.mousePosition;
                dragCurrentPosition = dragStartPosition;
                isDraggingSelection = false;
            }

            if (Input.GetMouseButton(0))
            {
                dragCurrentPosition = Input.mousePosition;
                isDraggingSelection = Vector2.Distance(dragStartPosition, dragCurrentPosition) >= dragSelectionThreshold;
            }

            if (Input.GetMouseButtonUp(0))
            {
                dragCurrentPosition = Input.mousePosition;

                if (isDraggingSelection)
                {
                    SelectUnitsInBox(dragStartPosition, dragCurrentPosition);
                }
                else
                {
                    TrySelectFromMouse();
                }

                isDraggingSelection = false;
            }
        }

        private void OnGUI()
        {
            if (!isDraggingSelection)
            {
                return;
            }

            var rect = GetGuiRect(dragStartPosition, dragCurrentPosition);
            DrawScreenRect(rect, dragFillColor);
            DrawScreenRectBorder(rect, 2f, dragBorderColor);
        }

        public void SelectUnit(IUnit unit)
        {
            Select(unit);
        }

        public void Select(ISelectable selectable)
        {
            CleanupSelection();

            if (Selected == selectable && selectedUnits.Count <= 1)
            {
                return;
            }

            DeselectCurrentSelection();
            Selected = selectable;
            Selected?.Select();

            if (selectable is IUnit unit && IsValidUnit(unit))
            {
                selectedUnits.Add(unit);
            }
        }

        public void DeselectCurrentUnit()
        {
            DeselectCurrentSelection();
        }

        public void DeselectCurrentSelection()
        {
            if (IsValidSelectable(Selected))
            {
                Selected.Deselect();
            }

            foreach (var unit in selectedUnits)
            {
                if (IsValidUnit(unit) && !ReferenceEquals(unit, Selected))
                {
                    unit.Deselect();
                }
            }

            selectedUnits.Clear();
            Selected = null;
        }

        public void HandleContextClickForSelectedUnits(RaycastHit hit)
        {
            CleanupSelection();

            if (selectedUnits.Count == 0)
            {
                SelectedUnit?.HandleContextClick(hit);
                return;
            }

            foreach (var unit in selectedUnits)
            {
                if (!IsValidUnit(unit))
                {
                    continue;
                }

                unit.HandleContextClick(hit);
            }
        }

        private void TrySelectFromMouse()
        {
            CleanupSelection();

            if (selectionCamera == null)
            {
                return;
            }

            var ray = selectionCamera.ScreenPointToRay(Input.mousePosition);

            if (!Physics.Raycast(ray, out var hit, Mathf.Infinity, selectionMask))
            {
                if (deselectWhenClickingEmpty)
                {
                    DeselectCurrentSelection();
                }

                return;
            }

            var unit = hit.collider.GetComponentInParent<IUnit>();
            var selectable = unit as ISelectable ?? hit.collider.GetComponentInParent<ISelectable>();

            if (selectedUnits.Count > 0 && unit != null && CanAnySelectedUnitAttack(unit))
            {
                AttackWithSelectedUnits(unit);
                return;
            }

            if (selectable == null)
            {
                if (deselectWhenClickingEmpty)
                {
                    DeselectCurrentSelection();
                }

                return;
            }

            if (unit != null && !IsSelectableTeam(unit))
            {
                if (deselectWhenClickingEmpty)
                {
                    DeselectCurrentSelection();
                }

                return;
            }

            if (blockNonUnitSelectionWhileUnitSelected && SelectedUnit != null && unit == null)
            {
                if (interactWithNonUnitsWhileUnitSelected)
                {
                    HandleContextClickForSelectedUnits(hit);
                }

                return;
            }

            if (toggleSelectedUnit && Selected == selectable)
            {
                DeselectCurrentSelection();
                return;
            }

            Select(selectable);
        }

        private void SelectUnitsInBox(Vector2 startPosition, Vector2 endPosition)
        {
            if (selectionCamera == null || !Services.TryResolve<GameDataStore>(out var gameDataStore))
            {
                return;
            }

            var selectionRect = GetScreenRect(startPosition, endPosition);
            var unitsInBox = new List<IUnit>();

            foreach (var unit in gameDataStore.Units)
            {
                if (!TryGetUnitTransform(unit, out var unitTransform) || !IsSelectableTeam(unit))
                {
                    continue;
                }

                var screenPosition = selectionCamera.WorldToScreenPoint(unitTransform.position);

                if (screenPosition.z < 0f || !selectionRect.Contains(screenPosition))
                {
                    continue;
                }

                unitsInBox.Add(unit);
            }

            if (unitsInBox.Count == 0)
            {
                if (deselectWhenClickingEmpty)
                {
                    DeselectCurrentSelection();
                }

                return;
            }

            SelectUnits(unitsInBox);
        }

        private void SelectUnits(List<IUnit> units)
        {
            DeselectCurrentSelection();

            foreach (var unit in units)
            {
                if (!IsValidUnit(unit))
                {
                    continue;
                }

                unit.Select();
                selectedUnits.Add(unit);
            }

            Selected = selectedUnits.Count > 0 ? selectedUnits[0] : null;
        }

        private bool CanAnySelectedUnitAttack(IUnit target)
        {
            CleanupSelection();

            foreach (var selectedUnit in selectedUnits)
            {
                if (CanIssueAttackOrder(selectedUnit, target))
                {
                    return true;
                }
            }

            return false;
        }

        private void AttackWithSelectedUnits(IUnit target)
        {
            CleanupSelection();

            foreach (var selectedUnit in selectedUnits)
            {
                if (!CanIssueAttackOrder(selectedUnit, target))
                {
                    continue;
                }

                selectedUnit.AttackOrder(target);
            }
        }

        private static Rect GetScreenRect(Vector2 startPosition, Vector2 endPosition)
        {
            return Rect.MinMaxRect(
                Mathf.Min(startPosition.x, endPosition.x),
                Mathf.Min(startPosition.y, endPosition.y),
                Mathf.Max(startPosition.x, endPosition.x),
                Mathf.Max(startPosition.y, endPosition.y));
        }

        private static Rect GetGuiRect(Vector2 startPosition, Vector2 endPosition)
        {
            var rect = GetScreenRect(startPosition, endPosition);
            rect.y = Screen.height - rect.yMax;
            return rect;
        }

        private static void DrawScreenRect(Rect rect, Color color)
        {
            var previousColor = GUI.color;
            GUI.color = color;
            GUI.DrawTexture(rect, Texture2D.whiteTexture);
            GUI.color = previousColor;
        }

        private static void DrawScreenRectBorder(Rect rect, float thickness, Color color)
        {
            DrawScreenRect(new Rect(rect.xMin, rect.yMin, rect.width, thickness), color);
            DrawScreenRect(new Rect(rect.xMin, rect.yMax - thickness, rect.width, thickness), color);
            DrawScreenRect(new Rect(rect.xMin, rect.yMin, thickness, rect.height), color);
            DrawScreenRect(new Rect(rect.xMax - thickness, rect.yMin, thickness, rect.height), color);
        }

        private static bool CanIssueAttackOrder(IUnit attacker, IUnit target)
        {
            if (!IsValidUnit(attacker) || !IsValidUnit(target))
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

        private bool IsSelectableTeam(IUnit unit)
        {
            if (!IsValidUnit(unit) || unit is not ITeam team)
            {
                return false;
            }

            return string.Equals(team.TeamId, selectableTeamId, System.StringComparison.Ordinal);
        }

        private void CleanupSelection()
        {
            for (var i = selectedUnits.Count - 1; i >= 0; i--)
            {
                if (!IsValidUnit(selectedUnits[i]))
                {
                    selectedUnits.RemoveAt(i);
                }
            }

            if (selectedUnits.Count > 0)
            {
                Selected = selectedUnits[0];
                return;
            }

            if (!IsValidSelectable(Selected))
            {
                Selected = null;
            }
        }

        private static bool TryGetUnitTransform(IUnit unit, out Transform unitTransform)
        {
            unitTransform = null;

            if (!IsValidUnit(unit))
            {
                return false;
            }

            unitTransform = unit.Transform;
            return unitTransform != null;
        }

        private static bool IsValidUnit(IUnit unit)
        {
            if (unit == null)
            {
                return false;
            }

            if (unit is Object unityObject && unityObject == null)
            {
                return false;
            }

            return unit.IsAlive;
        }

        private static bool IsValidSelectable(ISelectable selectable)
        {
            if (selectable == null)
            {
                return false;
            }

            return selectable is not Object unityObject || unityObject != null;
        }
    }
}
