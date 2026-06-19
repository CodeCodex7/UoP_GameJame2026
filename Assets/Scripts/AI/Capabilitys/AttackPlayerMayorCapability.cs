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
    public class AttackPlayerMayorCapability : CapabilityFactoryBase
    {
        public override ICapabilityConfig Create()
        {
            var builder = new CapabilityBuilder("AttackPlayerMayor");

            builder.AddGoal<AttackPlayerMayorGoal>()
                .SetBaseCost(1)
                .AddCondition<HasPlayerMayorUnit>(Comparison.SmallerThanOrEqual, 0);

            builder.AddAction<AttackRivalUnitAction>()
                .SetMoveMode(ActionMoveMode.PerformWhileMoving)
                .SetStoppingDistance(3.5f)
                .SetTarget<PlayerMayorUnitTarget>()
                .AddCondition<HasPlayerMayorUnit>(Comparison.GreaterThan, 0)
                .AddEffect<HasPlayerMayorUnit>(EffectType.Decrease)
                .SetProperties(new AttackRivalUnitAction.Props
                {
                    attackDuration = 1f
                });

            builder.AddTargetSensor<PlayerMayorUnitSensor>()
                .SetTarget<PlayerMayorUnitTarget>();

            builder.AddWorldSensor<HasPlayerMayorUnitSensor>()
                .SetKey<HasPlayerMayorUnit>();

            return builder.Build();
        }
    }
}
