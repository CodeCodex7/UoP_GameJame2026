using AI.Goap.UnitAI.Behaviors;
using UnityEngine;

public class UISelectionContext
{
    public UISelectionContext(GameObject sourceObject, IUnit unit)
        : this(sourceObject, unit as ISelectable)
    {
    }

    public UISelectionContext(GameObject sourceObject, ISelectable selectable)
    {
        SourceObject = sourceObject;
        Selectable = selectable;
        Unit = selectable as IUnit;
        Transform = sourceObject != null ? sourceObject.transform : selectable?.Transform;
        Team = sourceObject != null ? sourceObject.GetComponentInParent<ITeam>() : selectable as ITeam;
        Resource = sourceObject != null ? sourceObject.GetComponentInParent<IResource>() : null;
        Components = sourceObject != null ? sourceObject.GetComponentsInParent<MonoBehaviour>() : new MonoBehaviour[0];
    }

    public GameObject SourceObject { get; }
    public Transform Transform { get; }
    public ISelectable Selectable { get; }
    public IUnit Unit { get; }
    public ITeam Team { get; }
    public IResource Resource { get; }
    public MonoBehaviour[] Components { get; }

    public bool TryGetComponent<T>(out T component)
    {
        if (SourceObject == null)
        {
            component = default;
            return false;
        }

        component = SourceObject.GetComponentInParent<T>();
        return component != null;
    }
}

public interface IUIScreenContextReceiver
{
    void SetSelectionContext(UISelectionContext context);
}
