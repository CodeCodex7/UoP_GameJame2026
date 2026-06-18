using AI.Goap.UnitAI.Behaviors;
using CrashKonijn.Agent.Core;
using CrashKonijn.Goap.Runtime;
using UnityEngine;
using UnityEngine.AI;

namespace AI.Goap.UnitAI.Actions
{
    public class AgentMoveToLocation : GoapActionBase<AgentMoveToLocation.Data>
    {
        public class Data : IActionData
        {
            public ITarget Target { get; set; }
            public float InRange { get; set; }
        };


        public override void Created()
        {
            
        }


        public override IActionRunState Perform(IMonoAgent agent, Data data, IActionContext context)
        {
            var navMeshAgent = agent.transform.GetComponent<NavMeshAgent>();
            var targetPos = agent.transform.GetComponent<UnitAIBrain>().OrderMoveTarget;

            navMeshAgent.isStopped = false;
            navMeshAgent.SetDestination(targetPos);

            if (!navMeshAgent.pathPending && navMeshAgent.remainingDistance <= 2f)
            {
                return ActionRunState.Completed;
            }
            return ActionRunState.Continue;
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
