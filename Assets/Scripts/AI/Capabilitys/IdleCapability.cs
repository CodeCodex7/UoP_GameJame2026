using AI.Goap.UnitAI.Goal;
using AI.Goap.UnitAI.Sensors;
using AI.Goap.UnitAI.TargetKeys;
using AI.Goap.UnitAI.WorldsKeys;
using CrashKonijn.Goap.Runtime;
using CrashKonijn.Goap.Core;

namespace AI.Goap.UnitAI.Capabilitys
{
    public class IdleCapability : CapabilityFactoryBase
    {
        public override ICapabilityConfig Create()
        {
            var builder = new CapabilityBuilder("IdleCapability");

            builder.AddGoal<IdleGoal>()
                .SetBaseCost(0)
                .AddCondition<IsIdle>(Comparison.SmallerThanOrEqual, 1);
            
            builder.AddTargetSensor<IdleSensor>()
                .SetTarget<IdleTarget>();
            
            return builder.Build();
        }
    }
}