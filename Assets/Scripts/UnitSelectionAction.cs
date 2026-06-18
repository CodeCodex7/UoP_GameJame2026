using AI.Goap.UnitAI.Behaviors;
using UnityEngine;
using UnityEngine.Events;

public class UnitSelectionAction : MonoBehaviour, ISelectable, IUnitSelectionListener
{
    [SerializeField] private GameObject contextSource;
    [SerializeField] private string showUIScreenKey;
    [SerializeField] private string hideUIScreenKey;
    [SerializeField] private UnityEvent onSelected;
    [SerializeField] private UnityEvent onDeselected;

    public bool IsSelected { get; private set; }
    public Transform Transform => transform;

    public void Select()
    {
        IsSelected = true;
        HandleSelected(null);
    }

    public void Deselect()
    {
        IsSelected = false;
        HandleDeselected();
    }

    public void OnUnitSelected(IUnit unit)
    {
        IsSelected = true;
        HandleSelected(unit);
    }

    public void OnUnitDeselected(IUnit unit)
    {
        IsSelected = false;
        HandleDeselected();
    }

    private void HandleSelected(IUnit unit)
    {
        if (Services.TryResolve<UIScreenController>(out var uiController))
        {
            var context = CreateSelectionContext(unit);
            uiController.SetSelectionContext(context);

            if (!string.IsNullOrWhiteSpace(showUIScreenKey))
            {
                uiController.ShowScreen(showUIScreenKey, context);
            }
        }

        onSelected?.Invoke();
        NotifySelectionListeners(unit, true);
    }

    private void HandleDeselected()
    {
        if (!string.IsNullOrWhiteSpace(hideUIScreenKey) &&
            Services.TryResolve<UIScreenController>(out var uiController))
        {
            uiController.HideScreen(hideUIScreenKey);
        }

        onDeselected?.Invoke();
        NotifySelectionListeners(null, false);
    }

    private UISelectionContext CreateSelectionContext(IUnit unit)
    {
        var source = contextSource != null ? contextSource : gameObject;
        return new UISelectionContext(source, unit);
    }

    private void NotifySelectionListeners(IUnit unit, bool selected)
    {
        var behaviours = GetComponentsInChildren<MonoBehaviour>(true);

        foreach (var behaviour in behaviours)
        {
            if (behaviour == this || behaviour is not IUnitSelectionListener listener)
            {
                continue;
            }

            if (selected)
            {
                listener.OnUnitSelected(unit);
            }
            else
            {
                listener.OnUnitDeselected(unit);
            }
        }
    }
}
