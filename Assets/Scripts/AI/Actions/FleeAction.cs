using CrashKonijn.Agent.Core;
using CrashKonijn.Goap.Runtime;
using UnityEngine;
using UnityEngine.AI;

namespace AI.Goap.UnitAI.Actions
{
    public class FleeAction : GoapActionBase<FleeAction.Data>
    {
        private const float CompleteDistance = 1.5f;

        public class Data : IActionData
        {
            public ITarget Target { get; set; }
            public Vector3 Destination { get; set; }
            public bool HasDestination { get; set; }
        }

        public override void Start(IMonoAgent agent, Data data)
        {
            data.HasDestination = false;
        }

        public override IActionRunState Perform(IMonoAgent agent, Data data, IActionContext context)
        {
            if (!agent.transform.TryGetComponent<NavMeshAgent>(out var navMeshAgent))
            {
                return ActionRunState.Stop;
            }

            var destination = data.Target?.Position ?? agent.transform.position;
            navMeshAgent.isStopped = false;

            if (!data.HasDestination || (data.Destination - destination).sqrMagnitude > 1f)
            {
                data.Destination = destination;
                data.HasDestination = true;
                navMeshAgent.SetDestination(destination);
            }

            return !navMeshAgent.pathPending && navMeshAgent.remainingDistance <= CompleteDistance
                ? ActionRunState.Completed
                : ActionRunState.Continue;
        }

        public override void Stop(IMonoAgent agent, Data data)
        {
            if (agent.transform.TryGetComponent<NavMeshAgent>(out var navMeshAgent))
            {
                navMeshAgent.ResetPath();
            }
        }

        public override void Complete(IMonoAgent agent, Data data)
        {
            if (agent.transform.TryGetComponent<NavMeshAgent>(out var navMeshAgent))
            {
                navMeshAgent.ResetPath();
            }
        }
    }
}
