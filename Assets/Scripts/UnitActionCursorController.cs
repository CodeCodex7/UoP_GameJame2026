using AI.Goap.UnitAI.Behaviors;
using UnityEngine;
using UnityEngine.EventSystems;

public class UnitActionCursorController : MonoBehaviour
{
    [SerializeField] private UnitSelectionController unitSelectionController;
    [SerializeField] private Camera cursorCamera;
    [SerializeField] private LayerMask cursorMask = ~0;
    [SerializeField] private bool ignoreWhenPointerOverUI = true;
    [SerializeField] private CursorMode cursorMode = CursorMode.Auto;
    [SerializeField] private Vector2 cursorHotspot;
    [SerializeField] private Texture2D defaultCursor;
    [SerializeField] private Texture2D moveCursor;
    [SerializeField] private Texture2D harvestCursor;
    [SerializeField] private Texture2D attackCursor;
    [SerializeField] private Texture2D interactCursor;
    [SerializeField] private Texture2D blockedCursor;

    private UnitCursorType currentCursorType = UnitCursorType.Default;

    private void Awake()
    {
        if (unitSelectionController == null)
        {
            unitSelectionController = FindFirstObjectByType<UnitSelectionController>();
        }

        if (cursorCamera == null)
        {
            cursorCamera = Camera.main;
        }
    }

    private void Update()
    {
        var cursorType = GetCursorType();

        if (cursorType == currentCursorType)
        {
            return;
        }

        currentCursorType = cursorType;
        Cursor.SetCursor(GetTexture(cursorType), cursorHotspot, cursorMode);
    }

    private UnitCursorType GetCursorType()
    {
        if (unitSelectionController == null || unitSelectionController.SelectedUnit == null)
        {
            return UnitCursorType.Default;
        }

        if (ignoreWhenPointerOverUI && EventSystem.current != null && EventSystem.current.IsPointerOverGameObject())
        {
            return UnitCursorType.Default;
        }

        if (cursorCamera == null)
        {
            return UnitCursorType.Default;
        }

        var ray = cursorCamera.ScreenPointToRay(Input.mousePosition);

        return Physics.Raycast(ray, out var hit, Mathf.Infinity, cursorMask)
            ? unitSelectionController.SelectedUnit.GetContextCursor(hit)
            : UnitCursorType.Default;
    }

    private Texture2D GetTexture(UnitCursorType cursorType)
    {
        return cursorType switch
        {
            UnitCursorType.Move => moveCursor,
            UnitCursorType.Harvest => harvestCursor,
            UnitCursorType.Attack => attackCursor,
            UnitCursorType.Interact => interactCursor,
            UnitCursorType.Blocked => blockedCursor,
            _ => defaultCursor
        };
    }

    private void OnDisable()
    {
        currentCursorType = UnitCursorType.Default;
        Cursor.SetCursor(defaultCursor, cursorHotspot, cursorMode);
    }
}
