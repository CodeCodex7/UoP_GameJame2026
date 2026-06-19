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
    public class HarvestOrderCapability : CapabilityFactoryBase
    {
        public override ICapabilityConfig Create()
        {
            var builder = new CapabilityBuilder("HarvestOrder");

            builder.AddGoal<HarvestOrderGoal>()
                .SetBaseCost(1)
                .AddCondition<HasGatherOrder>(Comparison.SmallerThanOrEqual, 0);

            builder.AddAction<HarvestResourceAction>()
                .SetMoveMode(ActionMoveMode.PerformWhileMoving)
                .SetStoppingDistance(3.5f)
                .SetTarget<HarvestOrderTarget>()
                .AddCondition<HasGatherOrder>(Comparison.GreaterThan, 0)
                .AddEffect<HasGatherOrder>(EffectType.Decrease)
                .SetProperties(new HarvestResourceAction.Props
                {
                    harvestDuration = 3f,
                    harvestAmount = 1
                });

            builder.AddTargetSensor<HarvestOrderSensor>()
                .SetTarget<HarvestOrderTarget>();

            builder.AddWorldSensor<HasGatherOrderSensor>()
                .SetKey<HasGatherOrder>();

            return builder.Build();
        }
    }
}
