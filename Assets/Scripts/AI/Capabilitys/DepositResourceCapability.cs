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
    public class DepositResourceCapability : CapabilityFactoryBase
    {
        public override ICapabilityConfig Create()
        {
            var builder = new CapabilityBuilder("DepositResource");

            builder.AddGoal<DepositResourceGoal>()
                .SetBaseCost(1)
                .AddCondition<HasCarriedResource>(Comparison.SmallerThanOrEqual, 0);

            builder.AddAction<DepositResourceAction>()
                .SetMoveMode(ActionMoveMode.PerformWhileMoving)
                .SetStoppingDistance(2f)
                .SetTarget<ClosestStorageTarget>()
                .AddCondition<HasCarriedResource>(Comparison.GreaterThan, 0)
                .AddEffect<HasCarriedResource>(EffectType.Decrease)
                .SetProperties(new DepositResourceAction.Props
                {
                    depositDuration = 0.5f
                });

            builder.AddTargetSensor<StorageSensor>()
                .SetTarget<ClosestStorageTarget>();

            builder.AddWorldSensor<HasCarriedResourceSensor>()
                .SetKey<HasCarriedResource>();

            return builder.Build();
        }
    }
}
