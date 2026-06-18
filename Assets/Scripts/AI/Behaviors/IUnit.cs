using UnityEngine;

namespace AI.Goap.UnitAI.Behaviors
{
    public interface ISelectable
    {
        Transform Transform { get; }
        bool IsSelected { get; }

        void Select();
        void Deselect();
    }

    public interface IUnit : ISelectable
    {
        void MoveOrder(Vector3 targetPosition);
        void GatherOrder(IResource resource);
        void HandleContextClick(RaycastHit hit);
    }

    public interface IUnitActionTarget
    {
        bool TryIssueOrder(IUnit unit, RaycastHit hit);
    }

    public interface IResource
    {
        Transform Transform { get; }
        ResourcesBase Resource { get; }
        ResourceTypes ResourceType { get; }
        string ResourceId { get; }
        int AmountRemaining { get; }
        bool IsDepleted { get; }
        bool TryHarvest(int requestedAmount, out ResourceStack harvestedResource);
    }

    public interface IStorageBuilding
    {
        Transform Transform { get; }
        string StorageId { get; }
        bool CanAccept(ResourceStack resourceStack);
        bool Deposit(ResourceStack resourceStack);
        int GetStoredAmount(ResourceTypes resourceType);
        bool TryRemove(ResourceTypes resourceType, int amount, out ResourceStack removedResource);
    }

    public interface ITeam
    {
        string TeamId { get; }
    }

    public interface IUnitSelectionListener
    {
        void OnUnitSelected(IUnit unit);
        void OnUnitDeselected(IUnit unit);
    }
}
