﻿using System;
 using AI.Goap.UnitAI.Goal;
 using AI.Goap.UnitAI.Factories;
 using CrashKonijn.Goap.Runtime;
using UnityEngine;
using UnityEngine.Events;

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
        public bool HasAttackOrder => attackOrderTarget != null && attackOrderTarget.IsAlive;
        public int MaxHp => maxHp;
        public int CurrentHp => currentHp;
        public bool IsAlive => currentHp > 0;
        public float AttackSearchRadius => attackSearchRadius;
        public int AttackDamage => attackDamage;

        [SerializeField] private string teamId = "Player";
        [SerializeField] private GameObject selectionColorTarget;
        [SerializeField] private Color selectedColor = Color.red;
        [SerializeField] private int maxHp = 100;
        [SerializeField] private int currentHp;
        [SerializeField] private bool disableOnDeath;
        [SerializeField] private bool deleteOnDeath = true;
        
        [SerializeField] private UnityEvent onDeath;
        [SerializeField] private float attackSearchRadius = 15f;
        [SerializeField] private int attackDamage = 10;
        [SerializeField] private float autoHarvestIdleDelay = 10f;
        [SerializeField] private float autoHarvestRadius = 12f;
        [SerializeField] private int harvestCarryAmount = 1;
        [SerializeField] private UnitInventory inventory;
        
        private Renderer[] renderers;
        private Color[][] originalColors;
        private float idleTimer;
        private bool hasRequestedAutoHarvest;
        private IResource gatherOrderTarget;
        private IUnit attackOrderTarget;

        public UnitType Unittype;
        public int HarvestCarryAmount => harvestCarryAmount;
        
        private void Awake()
        {
            this.provider = this.GetComponent<GoapActionProvider>();
            agentTargetMove = this.GetComponent<AgentTargetMove>();
            inventory = GetComponent<UnitInventory>();
            currentHp = maxHp;
            
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

        private void OnEnable()
        {
            Services.ResolveWhenValid<GameDataStore>(RegisterUnit);
        }

        private void OnDisable()
        {
            if (Services.TryResolve<GameDataStore>(out var gameDataStore))
            {
                gameDataStore.UnregisterUnit(this);
            }
        }

        private void RegisterUnit()
        {
            if (Services.TryResolve<GameDataStore>(out var gameDataStore))
            {
                gameDataStore.RegisterUnit(this);
            }
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
                    provider.RequestGoal<AttackRivalUnitGoal>(true);
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
            ClearAttackOrder();
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
            ClearAttackOrder();
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

        public void AttackOrder(IUnit targetUnit)
        {
            if (!CanAttack(targetUnit))
            {
                return;
            }

            ClearGatherOrder();
            attackOrderTarget = targetUnit;
            IssueOrder();

            var receiver = provider.Receiver;
            var hadAction = receiver?.ActionState.Action != null;

            if (hadAction)
            {
                receiver.StopAction(false);
            }

            provider.RequestGoal<AttackOrderGoal>(true);

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
            var clickedUnit = hit.collider.GetComponentInParent<IUnit>();

            if (CanAttack(clickedUnit))
            {
                AttackOrder(clickedUnit);
                return;
            }

            var actionTarget = hit.collider.GetComponentInParent<IUnitActionTarget>();

            if (actionTarget != null && actionTarget.TryIssueOrder(this, hit))
            {
                return;
            }

            MoveOrder(hit.point);
        }

        public bool CanAttack(IUnit targetUnit)
        {
            if (targetUnit == null || ReferenceEquals(targetUnit, this) || !IsAlive || !targetUnit.IsAlive)
            {
                return false;
            }

            if (this is ITeam attackerTeam &&
                targetUnit is ITeam targetTeam &&
                string.Equals(attackerTeam.TeamId, targetTeam.TeamId, StringComparison.Ordinal))
            {
                return false;
            }

            return true;
        }

        public void TakeDamage(int amount)
        {
            if (amount <= 0 || !IsAlive)
            {
                return;
            }

            currentHp = Mathf.Max(0, currentHp - amount);

            if (currentHp <= 0)
            {
                Die();
            }
        }

        public void Heal(int amount)
        {
            if (amount <= 0 || !IsAlive)
            {
                return;
            }

            currentHp = Mathf.Min(maxHp, currentHp + amount);
        }

        private void Die()
        {
            HasOrder = false;
            ClearGatherOrder();
            ClearAttackOrder();
            provider?.Receiver?.StopAction(false);

            if (TryGetComponent<UnityEngine.AI.NavMeshAgent>(out var navMeshAgent))
            {
                navMeshAgent.ResetPath();
                navMeshAgent.isStopped = true;
            }

            if (IsSelected)
            {
                Deselect();
            }

            onDeath?.Invoke();

            if (disableOnDeath)
            {
                gameObject.SetActive(false);
            }
            if (deleteOnDeath)
            {
                Destroy(gameObject);
            }
            
            
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

        public bool TryGetAttackOrderTarget(out IUnit targetUnit)
        {
            targetUnit = null;

            if (attackOrderTarget == null)
            {
                return false;
            }

            if (!attackOrderTarget.IsAlive || attackOrderTarget.Transform == null)
            {
                CompleteAttackOrder();
                return false;
            }

            targetUnit = attackOrderTarget;
            return true;
        }

        public bool IsAttackOrderTarget(IUnit targetUnit)
        {
            return targetUnit != null && attackOrderTarget == targetUnit;
        }

        public void CompleteAttackOrder()
        {
            ClearAttackOrder();
            HasOrder = false;
            CompleteAutoHarvestCycle();
        }

        public void ClearAttackOrder()
        {
            attackOrderTarget = null;
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
