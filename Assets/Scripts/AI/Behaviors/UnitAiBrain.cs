﻿using System;
 using AI.Goap.UnitAI.Goal;
 using AI.Goap.UnitAI.Factories;
 using CrashKonijn.Goap.Runtime;
using UnityEngine;

namespace AI.Goap.UnitAI.Behaviors
{
    
    
    public enum OrderType{ Move,Action,Wait,Die}
    public enum UnitType {Worker,Fighter,Civilain}
    [RequireComponent(typeof(GoapActionProvider))]
    [RequireComponent(typeof(UnitInventory))]
    public class UnitAIBrain : MonoBehaviour, IUnit, ITeam
    {

        public GoapActionProvider provider;
        public GoapBehaviour behaviour;
        public AgentTargetMove agentTargetMove;

        //Debug for Test purposes
        //public sceens Selector;

        public Vector3 OrderMoveTarget;
        
        
        public bool HasOrder = false;
        public bool IsSelected { get; private set; }
        public Transform Transform => transform;
        public string TeamId => teamId;
        public float AutoHarvestRadius => autoHarvestRadius;
        public UnitInventory Inventory => inventory;
        public bool HasCarriedResource => inventory != null && inventory.HasAnyResource;
        public bool HasGatherOrder => gatherOrderTarget != null && !gatherOrderTarget.IsDepleted;

        [SerializeField] private string teamId = "Player";
        [SerializeField] private GameObject selectionColorTarget;
        [SerializeField] private Color selectedColor = Color.red;
        [SerializeField] private float autoHarvestIdleDelay = 10f;
        [SerializeField] private float autoHarvestRadius = 12f;
        [SerializeField] private int harvestCarryAmount = 1;
        [SerializeField] private UnitInventory inventory;
        
        private Renderer[] renderers;
        private Color[][] originalColors;
        private float idleTimer;
        private bool hasRequestedAutoHarvest;
        private IResource gatherOrderTarget;

        public UnitType Unittype;
        public int HarvestCarryAmount => harvestCarryAmount;
        
        private void Awake()
        {
            this.provider = this.GetComponent<GoapActionProvider>();
            agentTargetMove = this.GetComponent<AgentTargetMove>();
            inventory = GetComponent<UnitInventory>();

            if (inventory == null)
            {
                inventory = gameObject.AddComponent<UnitInventory>();
            }

            switch (Unittype)
            {
                case UnitType.Worker:
                {
                    CreateAgent("Worker");
                }
                    break;

                case UnitType.Civilain:
                {
                    CreateAgent("IdleAgent");
                }
                    break;
                
                case UnitType.Fighter:
                {
                    CreateAgent("Fighter");
                }
                    break;
                
                default:
                        CreateAgent("IdleAgent");
                    break;
            }
            
            CacheRendererColors();

        }

        void Start()
        {
            switch (Unittype)
            {
                case UnitType.Worker:
                {
                    provider.RequestGoal<IdleGoal>(true);
                }
                    break;

                case UnitType.Civilain:
                {
                    provider.RequestGoal<WanderGoal>(true);
                }
                    break;
                
                case UnitType.Fighter:
                {
                    CreateAgent("Fighter");
                }
                    break;
                
                default:
                    provider.RequestGoal<WanderGoal>(true);
                    break;
            }
            
            
        }


        void Update()
        {
            UpdateAutoHarvest();
        }

        void CreateAgent(string UnitType)
        {
            var agenttype = behaviour.GetAgentType(UnitType);
            provider.AgentType = agenttype;
        }
        
        
        public void IssueOrder()
        {
            HasOrder = true;
            idleTimer = 0f;
            hasRequestedAutoHarvest = false;
        }

        public void Select()
        {
            IsSelected = true;
            SetRendererColor(selectedColor);
            NotifySelectionListeners(true);
        }

        public void Deselect()
        {
            IsSelected = false;
            RestoreRendererColors();
            NotifySelectionListeners(false);
        }

        public void MoveOrder(Vector3 targetPosition)
        {
            ClearGatherOrder();
            IssueOrder();
            SetMoveOrderTarget(targetPosition, true);
        }

        public void GatherOrder(IResource resource)
        {
            if (Unittype != UnitType.Worker || resource == null || resource.IsDepleted)
            {
                return;
            }

            gatherOrderTarget = resource;
            IssueOrder();

            var receiver = provider.Receiver;
            var hadAction = receiver?.ActionState.Action != null;

            if (hadAction)
            {
                receiver.StopAction(false);
            }

            if (HasCarriedResource)
            {
                RequestDepositResource();
            }
            else
            {
                provider.RequestGoal<HarvestNearestGoal>(true);
            }

            if (hadAction)
            {
                provider.ResolveAction();
            }
        }

        private void SetMoveOrderTarget(Vector3 targetPosition, bool interruptCurrentAction)
        {
            OrderMoveTarget = targetPosition;

            var receiver = provider.Receiver;
            var hadAction = interruptCurrentAction && receiver?.ActionState.Action != null;

            if (hadAction)
            {
                receiver.StopAction(false);
            }

            provider.RequestGoal<MoveOrderGoal>(true);

            if (hadAction)
            {
                provider.ResolveAction();
            }
        }

        private void UpdateAutoHarvest()
        {
            if (Unittype != UnitType.Worker || HasOrder || HasCarriedResource)
            {
                idleTimer = 0f;
                return;
            }

            if (hasRequestedAutoHarvest)
            {
                return;
            }

            idleTimer += Time.deltaTime;

            if (idleTimer < autoHarvestIdleDelay)
            {
                return;
            }

            if (!Services.TryResolve<GameDataStore>(out var gameDataStore) ||
                !gameDataStore.TryGetClosestResource(transform.position, autoHarvestRadius, out _))
            {
                return;
            }

            hasRequestedAutoHarvest = true;
            provider.RequestGoal<HarvestNearestGoal>(true);
        }

        public void HandleContextClick(RaycastHit hit)
        {
            var actionTarget = hit.collider.GetComponentInParent<IUnitActionTarget>();

            if (actionTarget != null && actionTarget.TryIssueOrder(this, hit))
            {
                return;
            }

            MoveOrder(hit.point);
        }

        public void CompleteAutoHarvestCycle()
        {
            idleTimer = 0f;
            hasRequestedAutoHarvest = false;
        }

        public bool TryGetGatherOrderTarget(out IResource resource)
        {
            resource = null;

            if (gatherOrderTarget == null)
            {
                return false;
            }

            if (gatherOrderTarget.IsDepleted || gatherOrderTarget.Transform == null)
            {
                ClearGatherOrder();
                return false;
            }

            resource = gatherOrderTarget;
            return true;
        }

        public void ContinueGatherOrderOrComplete()
        {
            if (!TryGetGatherOrderTarget(out _))
            {
                CompleteGatherOrder();
                return;
            }

            provider.RequestGoal<HarvestNearestGoal>(true);
        }

        public void CompleteGatherOrder()
        {
            ClearGatherOrder();
            HasOrder = false;
            CompleteAutoHarvestCycle();
        }

        public void HandleHarvestTargetUnavailable()
        {
            if (HasGatherOrder)
            {
                CompleteGatherOrder();
                return;
            }

            CompleteAutoHarvestCycle();
        }

        public void ClearGatherOrder()
        {
            gatherOrderTarget = null;
        }

        public bool AddCarriedResource(ResourceStack resourceStack)
        {
            if (inventory == null || !inventory.TryAdd(resourceStack))
            {
                return false;
            }

            idleTimer = 0f;
            hasRequestedAutoHarvest = false;
            return true;
        }

        public void ClearCarriedResource()
        {
            inventory?.Clear();
            CompleteAutoHarvestCycle();
        }

        public void RequestDepositResource()
        {
            if (!HasCarriedResource)
            {
                return;
            }

            if (inventory == null || !inventory.TryPeekFirst(out var carriedResource))
            {
                return;
            }

            if (!Services.TryResolve<GameDataStore>(out var gameDataStore) ||
                !gameDataStore.TryGetClosestStorageForResource(transform.position, carriedResource, out _))
            {
                Debug.LogWarning($"Worker is carrying {carriedResource.ResourceName} but no compatible storage building is registered.");
                return;
            }

            provider.RequestGoal<DepositResourceGoal>(true);
        }

        private void CacheRendererColors()
        {
            var colorTarget = selectionColorTarget != null ? selectionColorTarget : gameObject;
            renderers = colorTarget.GetComponentsInChildren<Renderer>();
            originalColors = new Color[renderers.Length][];

            for (var rendererIndex = 0; rendererIndex < renderers.Length; rendererIndex++)
            {
                var materials = renderers[rendererIndex].materials;
                originalColors[rendererIndex] = new Color[materials.Length];

                for (var materialIndex = 0; materialIndex < materials.Length; materialIndex++)
                {
                    originalColors[rendererIndex][materialIndex] = materials[materialIndex].color;
                }
            }
        }

        private void SetRendererColor(Color color)
        {
            if (renderers == null)
            {
                CacheRendererColors();
            }

            foreach (var unitRenderer in renderers)
            {
                foreach (var material in unitRenderer.materials)
                {
                    material.color = color;
                }
            }
        }

        private void RestoreRendererColors()
        {
            if (renderers == null || originalColors == null)
            {
                return;
            }

            for (var rendererIndex = 0; rendererIndex < renderers.Length; rendererIndex++)
            {
                var materials = renderers[rendererIndex].materials;

                for (var materialIndex = 0; materialIndex < materials.Length; materialIndex++)
                {
                    materials[materialIndex].color = originalColors[rendererIndex][materialIndex];
                }
            }
        }

        private void NotifySelectionListeners(bool selected)
        {
            var behaviours = GetComponentsInChildren<MonoBehaviour>(true);

            foreach (var behaviour in behaviours)
            {
                if (behaviour is not IUnitSelectionListener listener)
                {
                    continue;
                }

                if (selected)
                {
                    listener.OnUnitSelected(this);
                }
                else
                {
                    listener.OnUnitDeselected(this);
                }
            }
        }
        
    }
}
