using AI.Goap.UnitAI.Goal;
using AI.Goap.UnitAI.Sensors;
using AI.Goap.UnitAI.TargetKeys;
using CrashKonijn.Goap.Runtime;
using CrashKonijn.Goap.Core;

namespace AI.Goap.UnitAI.Capabilitys
{
    public class DanceCapability : CapabilityFactoryBase
    {
        public override ICapabilityConfig Create()
        {
            var builder = new CapabilityBuilder("IdleCapability");

            builder.AddGoal<DanceGoal>()
                .SetBaseCost(10);

            builder.AddTargetSensor<DanceSensor>()
                .SetTarget<WanderTarget>();
            
            return builder.Build();
        }
    }
}