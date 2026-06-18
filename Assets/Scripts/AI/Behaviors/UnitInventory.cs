using System.Collections.Generic;
using UnityEngine;

namespace AI.Goap.UnitAI.Behaviors
{
    public class UnitInventory : MonoBehaviour
    {
        [SerializeField] private int maxTotalAmount;
        [SerializeField] private List<ResourceStack> resources = new List<ResourceStack>();

        public IReadOnlyList<ResourceStack> Resources => resources;
        public int MaxTotalAmount => maxTotalAmount;
        public int TotalAmount
        {
            get
            {
                var total = 0;

                foreach (var resource in resources)
                {
                    total += resource.amount;
                }

                return total;
            }
        }

        public bool HasAnyResource => TotalAmount > 0;

        public int GetAmount(ResourceTypes resourceType)
        {
            var total = 0;

            foreach (var resource in resources)
            {
                if (resource.resourceType == resourceType)
                {
                    total += resource.amount;
                }
            }

            return total;
        }

        public bool TryAdd(ResourceStack stack)
        {
            if (!stack.IsValid)
            {
                return false;
            }

            if (maxTotalAmount > 0 && TotalAmount + stack.amount > maxTotalAmount)
            {
                return false;
            }

            for (var i = 0; i < resources.Count; i++)
            {
                if (resources[i].resourceType != stack.resourceType)
                {
                    continue;
                }

                resources[i] = new ResourceStack(stack.resourceType, resources[i].amount + stack.amount);
                return true;
            }

            resources.Add(stack);
            return true;
        }

        public bool TryRemove(ResourceTypes resourceType, int amount, out ResourceStack removedStack)
        {
            removedStack = default;

            if (amount <= 0)
            {
                return false;
            }

            for (var i = 0; i < resources.Count; i++)
            {
                var resource = resources[i];

                if (resource.resourceType != resourceType || resource.amount <= 0)
                {
                    continue;
                }

                var removedAmount = Mathf.Min(amount, resource.amount);
                removedStack = new ResourceStack(resourceType, removedAmount);
                resource.amount -= removedAmount;

                if (resource.amount <= 0)
                {
                    resources.RemoveAt(i);
                }
                else
                {
                    resources[i] = resource;
                }

                return true;
            }

            return false;
        }

        public bool TryRemoveExact(ResourceTypes resourceType, int amount, out ResourceStack removedStack)
        {
            removedStack = default;

            if (amount <= 0 || GetAmount(resourceType) < amount)
            {
                return false;
            }

            var remainingAmount = amount;

            for (var i = resources.Count - 1; i >= 0 && remainingAmount > 0; i--)
            {
                var resource = resources[i];

                if (resource.resourceType != resourceType || resource.amount <= 0)
                {
                    continue;
                }

                var removedAmount = Mathf.Min(remainingAmount, resource.amount);
                resource.amount -= removedAmount;
                remainingAmount -= removedAmount;

                if (resource.amount <= 0)
                {
                    resources.RemoveAt(i);
                }
                else
                {
                    resources[i] = resource;
                }
            }

            removedStack = new ResourceStack(resourceType, amount);
            return true;
        }

        public bool TryRemoveFirst(out ResourceStack removedStack)
        {
            removedStack = default;

            if (resources.Count == 0)
            {
                return false;
            }

            var firstResource = resources[0];
            return TryRemove(firstResource.resourceType, firstResource.amount, out removedStack);
        }

        public bool TryPeekFirst(out ResourceStack resourceStack)
        {
            resourceStack = default;

            if (resources.Count == 0)
            {
                return false;
            }

            resourceStack = resources[0];
            return resourceStack.IsValid;
        }

        public void Clear()
        {
            resources.Clear();
        }
    }
}
