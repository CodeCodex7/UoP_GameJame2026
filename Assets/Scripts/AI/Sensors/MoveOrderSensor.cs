using AI.Goap.UnitAI.Behaviors;
using AI.Goap.UnitAI.WorldsKeys;
using CrashKonijn.Agent.Core;
using CrashKonijn.Goap.Runtime;
using UnityEngine;

namespace AI.Goap.UnitAI.Sensors
{
    public class MoveOrderSensor : LocalTargetSensorBase
    {
        public override void Created()
        {
            
        }

        public override void Update()
        {
            
        }
        
        public override ITarget Sense(IActionReceiver agent, IComponentReference references, ITarget existingTarget)
        {
            var Target =agent.Transform.gameObject.GetComponent<UnitAIBrain>().OrderMoveTarget;
            return new PositionTarget(Target);
        }
    }
}