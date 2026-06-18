using AI.Goap.UnitAI.Behaviors;
using CrashKonijn.Agent.Core;
using CrashKonijn.Goap.Core;
using CrashKonijn.Goap.Runtime;

namespace AI.Goap.UnitAI.Sensors
{
    public class HasOrderSensor : LocalWorldSensorBase
    {
        public override void Created()
        {
            
        }

        public override void Update()
        {
            
        }

        public override SenseValue Sense(IActionReceiver agent, IComponentReference references)
        {
            return agent.Transform.GetComponent<UnitAIBrain>().HasOrder ? 1 : 0;
        }
    }
}