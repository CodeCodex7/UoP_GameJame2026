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
    public class AttackOrderCapability : CapabilityFactoryBase
    {
        public override ICapabilityConfig Create()
        {
            var builder = new CapabilityBuilder("AttackOrder");

            builder.AddGoal<AttackOrderGoal>()
                .SetBaseCost(1)
                .AddCondition<HasAttackOrder>(Comparison.SmallerThanOrEqual, 0);

            builder.AddAction<AttackOrderAction>()
                .SetMoveMode(ActionMoveMode.PerformWhileMoving)
                .SetStoppingDistance(3.5f)
                .SetTarget<AttackOrderTarget>()
                .AddCondition<HasAttackOrder>(Comparison.GreaterThan, 0)
                .AddEffect<HasAttackOrder>(EffectType.Decrease)
                .SetProperties(new AttackOrderAction.Props
                {
                    attackDuration = 1f
                });

            builder.AddTargetSensor<AttackOrderSensor>()
                .SetTarget<AttackOrderTarget>();

            builder.AddWorldSensor<HasAttackOrderSensor>()
                .SetKey<HasAttackOrder>();

            return builder.Build();
        }
    }
}
