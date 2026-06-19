using System;
using AI.Goap.UnitAI.Behaviors;
using CrashKonijn.Agent.Core;
using CrashKonijn.Goap.Runtime;
using UnityEngine;

namespace AI.Goap.UnitAI.Actions
{
    public class DepositResourceAction : GoapActionBase<DepositResourceAction.Data, DepositResourceAction.Props>
    {
        [Serializable]
        public class Props : IActionProperties
        {
            public float depositDuration = 0.5f;
        }

        public class Data : IActionData
        {
            public ITarget Target { get; set; }
            public IActionRunState DepositTimer { get; set; }
            public bool HasStartedDeposit { get; set; }
            public IStorageBuilding Storage { get; set; }
        }

        public override void Created()
        {
        }

        public override void Start(IMonoAgent agent, Data data)
        {
            data.DepositTimer = null;
            data.HasStartedDeposit = false;
            data.Storage = GetStorage(data.Target, agent.transform);
        }

        public override bool IsValid(IActionReceiver agent, Data data)
        {
            var brain = agent.Transform.GetComponent<UnitAIBrain>();
            return brain != null && brain.HasCarriedResource && GetStorage(data.Target, agent.Transform) != null;
        }

        public override IActionRunState Perform(IMonoAgent agent, Data data, IActionContext context)
        {
            if (data.Target == null || !data.Target.IsValid())
            {
                return ActionRunState.Stop;
            }

            if (!agent.transform.TryGetComponent<UnitAIBrain>(out var brain) || !brain.HasCarriedResource)
            {
                return ActionRunState.Stop;
            }

            data.Storage ??= GetStorage(data.Target, agent.transform);

            if (data.Storage == null)
            {
                return ActionRunState.Stop;
            }

            if (!context.IsInRange)
            {
                data.HasStartedDeposit = false;
                data.DepositTimer = null;
                return ActionRunState.Continue;
            }

            if (!data.HasStartedDeposit)
            {
                data.HasStartedDeposit = true;
                data.DepositTimer = ActionRunState.Wait(Properties.depositDuration);
                Debug.Log("Started depositing carried resource");
            }

            if (data.DepositTimer.IsRunning())
            {
                return data.DepositTimer;
            }

            if (!brain.Inventory.TryRemoveFirst(out var carriedResource))
            {
                return ActionRunState.Stop;
            }

            if (!data.Storage.Deposit(carriedResource))
            {
                brain.Inventory.TryAdd(carriedResource);
                return ActionRunState.Stop;
            }

            brain.ContinueGatherOrderOrComplete();
            Debug.Log("Finished depositing carried resource");
            return ActionRunState.Completed;
        }

        private static IStorageBuilding GetStorage(ITarget target, Transform agentTransform)
        {
            if (target is TransformTarget transformTarget && transformTarget.Transform != null)
            {
                return transformTarget.Transform.GetComponentInParent<IStorageBuilding>();
            }

            if (agentTransform == null ||
                !agentTransform.TryGetComponent<UnitAIBrain>(out var brain) ||
                brain.Inventory == null ||
                !brain.Inventory.TryPeekFirst(out var carriedResource) ||
                !Services.TryResolve<GameDataStore>(out var gameDataStore) ||
                !gameDataStore.TryGetClosestStorageForResource(agentTransform.position, carriedResource, out var closestStorage))
            {
                return null;
            }

            return closestStorage;
        }
    }
}
