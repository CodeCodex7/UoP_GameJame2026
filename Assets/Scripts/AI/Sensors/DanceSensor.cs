using CrashKonijn.Agent.Core;
using CrashKonijn.Goap.Runtime;
using UnityEngine;

namespace AI.Goap.UnitAI.Sensors
{
    public class DanceSensor : LocalTargetSensorBase
    {
        public override void Created()
        {
            
        }

        public override void Update()
        {
            
        }

        public override ITarget Sense(IActionReceiver agent, IComponentReference references, ITarget existingTarget)
        {
            return new PositionTarget(new Vector3(0, 0, 0));
        }
    }
}