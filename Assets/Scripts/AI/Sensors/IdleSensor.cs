using CrashKonijn.Agent.Core;
using CrashKonijn.Goap.Runtime;

namespace AI.Goap.UnitAI.Sensors
{
    /// <summary>
    /// If they have nothing to do we want to move or stay where they are
    /// </summary>
    public class IdleSensor : LocalTargetSensorBase
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
            if (existingTarget != null)
                return existingTarget;

            return new TransformTarget(agent.Transform);
        }
    }
}