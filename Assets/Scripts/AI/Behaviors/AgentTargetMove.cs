using System;
using CrashKonijn.Agent.Core;
using CrashKonijn.Agent.Runtime;
using UnityEngine;
using UnityEngine.AI;

namespace AI.Goap.UnitAI.Behaviors
{
    
    [RequireComponent(typeof(AgentBehaviour))]
    [RequireComponent(typeof(NavMeshAgent))]
    public class AgentTargetMove : MonoBehaviour
    {
        
        private AgentBehaviour AiAgent;
        private NavMeshAgent NavMeshAgent;
        public ITarget CurrentTarget;
        
        private void Awake()
        {
            this.AiAgent = this.GetComponent<AgentBehaviour>();
            this.NavMeshAgent = this.GetComponent<NavMeshAgent>();
        }


        private void OnEnable()
        {
            AiAgent.Events.OnTargetChanged += OntargetChanged;
            AiAgent.Events.OnTargetLost += OnTargetLost;
        }

        private void OnDisable()
        {
            AiAgent.Events.OnTargetChanged -= OntargetChanged;
            AiAgent.Events.OnTargetLost -= OnTargetLost;
        }


        private void Update()
        {
            if (AiAgent.IsPaused)
            {
                return;
            }

            if (AiAgent.CurrentTarget == null)
            {
                return;
            }

            if (CurrentTarget == null)
            {
                return;
            }

            NavMeshAgent.isStopped = false;
            NavMeshAgent.SetDestination(CurrentTarget.Position);
            
        }


        void OntargetChanged(ITarget target, bool inRange)
        {
            CurrentTarget = target;
            NavMeshAgent.isStopped = false;
            NavMeshAgent.SetDestination(target.Position);
        }

        void OnTargetLost()
        {
            CurrentTarget = null;
            NavMeshAgent.ResetPath();
        }
        
    }
}
