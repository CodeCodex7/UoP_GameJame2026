using AI.Goap.UnitAI.Behaviors;
using UnityEngine;
using UnityEngine.Events;

public class UIScreenInteractionController : MonoBehaviour, IUnitSelectionListener
{
    [SerializeField] private GameObject contextSource;
    [SerializeField] private string showOnSelectedKey;
    [SerializeField] private string hideOnSelectedKey;
    [SerializeField] private string showOnDeselectedKey;
    [SerializeField] private string hideOnDeselectedKey;
    [SerializeField] private bool sendContextOnSelected = true;
    [SerializeField] private UnityEvent onSelectedUIHandled;
    [SerializeField] private UnityEvent onDeselectedUIHandled;

    public void OnUnitSelected(IUnit unit)
    {
        var context = sendContextOnSelected ? CreateSelectionContext(unit) : null;
        HandleUI(showOnSelectedKey, hideOnSelectedKey, context);
        onSelectedUIHandled?.Invoke();
    }

    public void OnUnitDeselected(IUnit unit)
    {
        HandleUI(showOnDeselectedKey, hideOnDeselectedKey, null);
        onDeselectedUIHandled?.Invoke();
    }

    public void ShowScreen(string key)
    {
        if (Services.TryResolve<UIScreenController>(out var uiController))
        {
            uiController.ShowScreen(key);
        }
    }

    public void HideScreen(string key)
    {
        if (Services.TryResolve<UIScreenController>(out var uiController))
        {
            uiController.HideScreen(key);
        }
    }

    public void ShowScreenWithContext(string key)
    {
        if (Services.TryResolve<UIScreenController>(out var uiController))
        {
            uiController.ShowScreen(key, CreateSelectionContext(null));
        }
    }

    private void HandleUI(string showKey, string hideKey, UISelectionContext context)
    {
        if (!Services.TryResolve<UIScreenController>(out var uiController))
        {
            return;
        }

        if (!string.IsNullOrWhiteSpace(hideKey))
        {
            uiController.HideScreen(hideKey);
        }

        if (!string.IsNullOrWhiteSpace(showKey))
        {
            if (context != null)
            {
                uiController.ShowScreen(showKey, context);
            }
            else
            {
                uiController.ShowScreen(showKey);
            }
        }
    }

    private UISelectionContext CreateSelectionContext(IUnit unit)
    {
        var source = contextSource != null ? contextSource : gameObject;
        return new UISelectionContext(source, unit);
    }
}
