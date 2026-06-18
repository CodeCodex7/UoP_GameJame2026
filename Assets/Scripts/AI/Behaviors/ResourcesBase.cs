using System;
using UnityEngine;

namespace AI.Goap.UnitAI.Behaviors
{
    public enum ResourceTypes : byte
    {
        Wood = 0,
        Metal = 1,
        Mushrooms = 2
    }

    [Serializable]
    public class ResourcesBase
    {
        [SerializeField] private ResourceTypes resourceType;
        [SerializeField] private string description;

        public int ResourceID => (int)resourceType;
        public ResourceTypes ResourceType
        {
            get => resourceType;
            set => resourceType = value;
        }

        public string Name => ResourceName(resourceType);
        public string Description
        {
            get => description;
            set => description = value;
        }

        public static string ResourceName(ResourceTypes type)
        {
            return type.ToString();
        }

        public string ResourceName(int id)
        {
            if (!Enum.IsDefined(typeof(ResourceTypes), id))
            {
                return string.Empty;
            }

            return ResourceName((ResourceTypes)id);
        }
    }

    [Serializable]
    public struct ResourceStack
    {
        public ResourceTypes resourceType;
        public int amount;

        public ResourceStack(ResourceTypes resourceType, int amount)
        {
            this.resourceType = resourceType;
            this.amount = amount;
        }

        public string ResourceName => ResourcesBase.ResourceName(resourceType);
        public bool IsValid => amount > 0;
    }
}
