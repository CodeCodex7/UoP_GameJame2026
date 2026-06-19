using AI.Goap.UnitAI.Behaviors;
using CrashKonijn.Agent.Core;
using CrashKonijn.Goap.Runtime;
using UnityEngine;
using UnityEngine.AI;

namespace AI.Goap.UnitAI.Actions
{
    public class AgentMoveToLocation : GoapActionBase<AgentMoveToLocation.Data>
    {
        private const float CompleteDistance = 2.5f;
        private const float DestinationRadius = 3f;
        private const float StuckTime = 1.5f;
        private const float MinimumProgress = 0.25f;
        private const float NavMeshSampleRadius = 3f;

        public class Data : IActionData
        {
            public ITarget Target { get; set; }
            public float InRange { get; set; }
            public Vector3 OriginalTarget { get; set; }
            public Vector3 CurrentDestination { get; set; }
            public float LastRemainingDistance { get; set; }
            public float LastProgressTime { get; set; }
            public bool HasDestination { get; set; }
        };


        public override void Created()
        {
            
        }

        public override void Start(IMonoAgent agent, Data data)
        {
            data.HasDestination = false;
            data.LastRemainingDistance = float.MaxValue;
            data.LastProgressTime = Time.time;
        }

        public override IActionRunState Perform(IMonoAgent agent, Data data, IActionContext context)
        {
            var navMeshAgent = agent.transform.GetComponent<NavMeshAgent>();
            var brain = agent.transform.GetComponent<UnitAIBrain>();

            if (navMeshAgent == null || brain == null)
            {
                return ActionRunState.Stop;
            }

            var targetPos = brain.OrderMoveTarget;
            navMeshAgent.isStopped = false;

            if (!data.HasDestination || (data.OriginalTarget - targetPos).sqrMagnitude > 0.01f)
            {
                data.OriginalTarget = targetPos;
                SetNewDestination(navMeshAgent, data);
            }

            if (!navMeshAgent.pathPending && navMeshAgent.remainingDistance <= CompleteDistance)
            {
                return ActionRunState.Completed;
            }

            UpdateProgressOrRepath(navMeshAgent, data);
            return ActionRunState.Continue;
        }

        private static void UpdateProgressOrRepath(NavMeshAgent navMeshAgent, Data data)
        {
            if (navMeshAgent.pathPending)
            {
                return;
            }

            if (navMeshAgent.pathStatus == NavMeshPathStatus.PathInvalid ||
                navMeshAgent.pathStatus == NavMeshPathStatus.PathPartial)
            {
                SetNewDestination(navMeshAgent, data);
                return;
            }

            var remainingDistance = navMeshAgent.remainingDistance;

            if (data.LastRemainingDistance - remainingDistance >= MinimumProgress)
            {
                data.LastRemainingDistance = remainingDistance;
                data.LastProgressTime = Time.time;
                return;
            }

            if (Time.time - data.LastProgressTime < StuckTime)
            {
                return;
            }

            SetNewDestination(navMeshAgent, data);
        }

        private static void SetNewDestination(NavMeshAgent navMeshAgent, Data data)
        {
            data.CurrentDestination = PickDestinationAround(data.OriginalTarget);
            data.HasDestination = true;
            data.LastRemainingDistance = float.MaxValue;
            data.LastProgressTime = Time.time;
            navMeshAgent.SetDestination(data.CurrentDestination);
        }

        private static Vector3 PickDestinationAround(Vector3 target)
        {
            for (var i = 0; i < 8; i++)
            {
                var offset = Random.insideUnitCircle * DestinationRadius;
                var candidate = target + new Vector3(offset.x, 0f, offset.y);

                if (NavMesh.SamplePosition(candidate, out var hit, NavMeshSampleRadius, NavMesh.AllAreas))
                {
                    return hit.position;
                }
            }

            return NavMesh.SamplePosition(target, out var targetHit, NavMeshSampleRadius, NavMesh.AllAreas)
                ? targetHit.position
                : target;
        }

        public override void Stop(IMonoAgent agent, Data data)
        {
            agent.transform.GetComponent<NavMeshAgent>().ResetPath();
        }

        public override void Complete(IMonoAgent agent, Data data)
        {
            Debug.Log("MoveComplete");
            agent.transform.GetComponent<NavMeshAgent>().ResetPath();
            agent.transform.GetComponent<UnitAIBrain>().HasOrder = false;
        }
    }
}
