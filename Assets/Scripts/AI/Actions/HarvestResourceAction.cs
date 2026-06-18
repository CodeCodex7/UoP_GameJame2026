using System;
using AI.Goap.UnitAI.Behaviors;
using CrashKonijn.Agent.Core;
using CrashKonijn.Goap.Runtime;
using UnityEngine;

namespace AI.Goap.UnitAI.Actions
{
    public class HarvestResourceAction : GoapActionBase<HarvestResourceAction.Data, HarvestResourceAction.Props>
    {
        [Serializable]
        public class Props : IActionProperties
        {
            public float harvestDuration = 3f;
            public int harvestAmount = 1;
        }

        public class Data : IActionData
        {
            public ITarget Target { get; set; }
            public IActionRunState HarvestTimer { get; set; }
            public bool HasStartedHarvesting { get; set; }
            public bool HasHarvested { get; set; }
        }

        public override void Created()
        {
        }

        public override void Start(IMonoAgent agent, Data data)
        {
            data.HarvestTimer = null;
            data.HasStartedHarvesting = false;
            data.HasHarvested = false;
        }

        public override IActionRunState Perform(IMonoAgent agent, Data data, IActionContext context)
        {
            if (data.Target == null || !data.Target.IsValid())
            {
                NotifyHarvestUnavailable(agent);
                return ActionRunState.Stop;
            }

            if (!context.IsInRange)
            {
                data.HasStartedHarvesting = false;
                data.HarvestTimer = null;
                return ActionRunState.Continue;
            }

            if (!data.HasStartedHarvesting)
            {
                data.HasStartedHarvesting = true;
                data.HarvestTimer = ActionRunState.Wait(Properties.harvestDuration);
                Debug.Log("Started harvesting nearest resource");
            }

            if (data.HarvestTimer.IsRunning())
            {
                return data.HarvestTimer;
            }

            if (!data.HasHarvested)
            {
                if (!TryHarvestResource(agent, data))
                {
                    NotifyHarvestUnavailable(agent);
                    return ActionRunState.Stop;
                }

                data.HasHarvested = true;
            }

            return ActionRunState.Completed;
        }

        public override void Complete(IMonoAgent agent, Data data)
        {
            if (agent.transform.TryGetComponent<UnitAIBrain>(out var brain))
            {
                brain.RequestDepositResource();
            }
        }

        private bool TryHarvestResource(IMonoAgent agent, Data data)
        {
            if (agent.transform.TryGetComponent<UnitAIBrain>(out var brain) && brain.HasCarriedResource)
            {
                return true;
            }

            if (data.Target is not TransformTarget transformTarget)
            {
                return false;
            }

            var resource = transformTarget.Transform.GetComponentInParent<IResource>();

            if (resource == null || resource.IsDepleted)
            {
                return false;
            }

            var requestedAmount = Properties.harvestAmount;

            if (agent.transform.TryGetComponent<UnitAIBrain>(out brain))
            {
                requestedAmount = Mathf.Max(1, brain.HarvestCarryAmount);
            }

            if (!resource.TryHarvest(requestedAmount, out var harvestedResource))
            {
                return false;
            }

            if (brain != null && !brain.AddCarriedResource(harvestedResource))
            {
                return false;
            }

            Debug.Log($"Finished harvesting {harvestedResource.amount} {harvestedResource.ResourceName}");
            return true;
        }

        private void NotifyHarvestUnavailable(IMonoAgent agent)
        {
            if (agent.transform.TryGetComponent<UnitAIBrain>(out var brain))
            {
                brain.HandleHarvestTargetUnavailable();
            }
        }
    }
}
