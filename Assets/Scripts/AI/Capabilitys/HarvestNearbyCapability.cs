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
    public class HarvestNearbyCapability : CapabilityFactoryBase
    {
        public override ICapabilityConfig Create()
        {
            var builder = new CapabilityBuilder("HarvestNearby");

            builder.AddGoal<HarvestNearestGoal>()
                .SetBaseCost(1)
                .AddCondition<IsWorking>(Comparison.GreaterThanOrEqual, 1);

            builder.AddAction<HarvestResourceAction>()
                .SetMoveMode(ActionMoveMode.PerformWhileMoving)
                .SetStoppingDistance(2f)
                .SetTarget<ClosestResourceNodeTarget>()
                .AddEffect<IsWorking>(EffectType.Increase)
                .SetProperties(new HarvestResourceAction.Props
                {
                    harvestDuration = 3f,
                    harvestAmount = 1
                });

            builder.AddTargetSensor<ResourceSensor>()
                .SetTarget<ClosestResourceNodeTarget>();

            return builder.Build();
        }
    }
}
