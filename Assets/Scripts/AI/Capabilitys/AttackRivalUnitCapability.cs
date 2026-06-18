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
    public class AttackRivalUnitCapability : CapabilityFactoryBase
    {
        public override ICapabilityConfig Create()
        {
            var builder = new CapabilityBuilder("AttackRivalUnit");

            builder.AddGoal<AttackRivalUnitGoal>()
                .SetBaseCost(1)
                .AddCondition<HasRivalUnit>(Comparison.SmallerThanOrEqual, 0);

            builder.AddAction<AttackRivalUnitAction>()
                .SetMoveMode(ActionMoveMode.PerformWhileMoving)
                .SetStoppingDistance(2f)
                .SetTarget<ClosestRivalUnitTarget>()
                .AddCondition<HasRivalUnit>(Comparison.GreaterThan, 0)
                .AddEffect<HasRivalUnit>(EffectType.Decrease)
                .SetProperties(new AttackRivalUnitAction.Props
                {
                    attackDuration = 1f
                });

            builder.AddTargetSensor<RivalUnitSensor>()
                .SetTarget<ClosestRivalUnitTarget>();

            builder.AddWorldSensor<HasRivalUnitSensor>()
                .SetKey<HasRivalUnit>();

            return builder.Build();
        }
    }
}
