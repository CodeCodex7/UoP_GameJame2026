using AI.Goap.UnitAI.Actions;
using AI.Goap.UnitAI.Goal;
using AI.Goap.UnitAI.Sensors;
using AI.Goap.UnitAI.TargetKeys;
using AI.Goap.UnitAI.WorldsKeys;
using CrashKonijn.Agent.Core;
using CrashKonijn.Goap.Core;
using CrashKonijn.Goap.Runtime;

namespace AI.Goap.UnitAI.Capabilitys
{
    public class FleeCapability : CapabilityFactoryBase
    {
        public override ICapabilityConfig Create()
        {
            var builder = new CapabilityBuilder("FleeCapability");

            builder.AddGoal<FleeGoal>()
                .SetBaseCost(0)
                .AddCondition<HasNearbyThreat>(Comparison.SmallerThanOrEqual, 0);

            builder.AddAction<FleeAction>()
                .SetMoveMode(ActionMoveMode.PerformWhileMoving)
                .SetStoppingDistance(1.5f)
                .SetTarget<FleeTarget>()
                .AddCondition<HasNearbyThreat>(Comparison.GreaterThan, 0)
                .AddEffect<HasNearbyThreat>(EffectType.Decrease);

            builder.AddTargetSensor<FleeTargetSensor>()
                .SetTarget<FleeTarget>();

            builder.AddWorldSensor<HasNearbyThreatSensor>()
                .SetKey<HasNearbyThreat>();

            return builder.Build();
        }
    }
}
