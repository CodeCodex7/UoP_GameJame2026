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
    public class MoveOrderCapability : CapabilityFactoryBase
    {
        public override ICapabilityConfig Create()
        {
            var builder = new CapabilityBuilder("MoveOrder");

            builder.AddGoal<MoveOrderGoal>()
                .SetBaseCost(5)
                .AddCondition<HasOrders>(Comparison.SmallerThanOrEqual, 0);

            builder.AddAction<AgentMoveToLocation>()
                .SetMoveMode(ActionMoveMode.PerformWhileMoving)
                .SetTarget<MoveOrderTarget>()
                .AddCondition<HasOrders>(Comparison.GreaterThan, 0)
                .AddEffect<HasOrders>(EffectType.Decrease);

            builder.AddTargetSensor<MoveOrderSensor>()
                .SetTarget<MoveOrderTarget>();

            builder.AddWorldSensor<HasOrderSensor>()
                .SetKey<HasOrders>();

            return builder.Build();

        }
    }
}