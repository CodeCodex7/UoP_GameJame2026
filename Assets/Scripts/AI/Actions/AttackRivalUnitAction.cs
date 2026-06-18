using System;
using AI.Goap.UnitAI.Behaviors;
using CrashKonijn.Agent.Core;
using CrashKonijn.Goap.Runtime;
using UnityEngine;

namespace AI.Goap.UnitAI.Actions
{
    public class AttackRivalUnitAction : GoapActionBase<AttackRivalUnitAction.Data, AttackRivalUnitAction.Props>
    {
        [Serializable]
        public class Props : IActionProperties
        {
            public float attackDuration = 1f;
        }

        public class Data : IActionData
        {
            public ITarget Target { get; set; }
            public IActionRunState AttackTimer { get; set; }
            public bool HasStartedAttack { get; set; }
            public IUnit TargetUnit { get; set; }
        }

        public override void Created()
        {
        }

        public override void Start(IMonoAgent agent, Data data)
        {
            data.TargetUnit = GetTargetUnit(data.Target);
            data.AttackTimer = null;
            data.HasStartedAttack = false;
        }

        public override bool IsValid(IActionReceiver agent, Data data)
        {
            var attacker = agent.Transform.GetComponent<UnitAIBrain>();
            var targetUnit = GetTargetUnit(data.Target);

            return attacker != null &&
                   attacker.IsAlive &&
                   targetUnit != null &&
                   targetUnit.IsAlive &&
                   IsRival(attacker, targetUnit);
        }

        public override IActionRunState Perform(IMonoAgent agent, Data data, IActionContext context)
        {
            if (!agent.transform.TryGetComponent<UnitAIBrain>(out var attacker) || !attacker.IsAlive)
            {
                return ActionRunState.Stop;
            }

            data.TargetUnit ??= GetTargetUnit(data.Target);

            if (data.TargetUnit == null || !data.TargetUnit.IsAlive || !IsRival(attacker, data.TargetUnit))
            {
                return ActionRunState.Completed;
            }

            if (!context.IsInRange)
            {
                data.HasStartedAttack = false;
                data.AttackTimer = null;
                return ActionRunState.Continue;
            }

            if (!data.HasStartedAttack)
            {
                data.HasStartedAttack = true;
                data.AttackTimer = ActionRunState.Wait(Properties.attackDuration);
            }

            if (data.AttackTimer.IsRunning())
            {
                return data.AttackTimer;
            }

            data.TargetUnit.TakeDamage(attacker.AttackDamage);

            if (!data.TargetUnit.IsAlive)
            {
                Debug.Log($"{attacker.name} defeated {data.TargetUnit.Transform.name}");
                return ActionRunState.Completed;
            }

            data.HasStartedAttack = false;
            data.AttackTimer = null;
            return ActionRunState.Continue;
        }

        private static IUnit GetTargetUnit(ITarget target)
        {
            if (target is not TransformTarget transformTarget || transformTarget.Transform == null)
            {
                return null;
            }

            return transformTarget.Transform.GetComponentInParent<IUnit>();
        }

        private static bool IsRival(IUnit attacker, IUnit target)
        {
            if (attacker == null || target == null || attacker == target)
            {
                return false;
            }

            if (attacker is ITeam attackerTeam &&
                target is ITeam targetTeam &&
                string.Equals(attackerTeam.TeamId, targetTeam.TeamId, StringComparison.Ordinal))
            {
                return false;
            }

            return true;
        }
    }
}
