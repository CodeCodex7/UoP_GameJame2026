using CrashKonijn.Agent.Core;
using CrashKonijn.Goap.Runtime;
using UnityEngine;

namespace AI.Goap.UnitAI.Sensors
{
    public class WanderSensor : LocalTargetSensorBase
    {
        public override void Created()
        {
            //throw new System.NotImplementedException();
        }

        public override void Update()
        {
            //throw new System.NotImplementedException();
        }

        public override ITarget Sense(IActionReceiver agent, IComponentReference references, ITarget existingTarget)
        {
            //throw new System.NotImplementedException();
            return new PositionTarget(GetRandomTarget(agent));

        }
        
        Vector3 GetRandomTarget(IActionReceiver agent)
        {
            var random =  Random.insideUnitCircle * 5f;
            var position = agent.Transform.position + new Vector3(random.x, 0f, random.y);
            return position;
        }
        
    }
}