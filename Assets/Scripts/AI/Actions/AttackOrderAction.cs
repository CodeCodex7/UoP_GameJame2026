using System;
using AI.Goap.UnitAI.Behaviors;
using CrashKonijn.Agent.Core;
using CrashKonijn.Goap.Runtime;
using UnityEngine;

namespace AI.Goap.UnitAI.Actions
{
    public class AttackOrderAction : GoapActionBase<AttackOrderAction.Data, AttackOrderAction.Props>
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
            data.TargetUnit = GetTargetUnit(agent.transform, data.Target);
            data.AttackTimer = null;
            data.HasStartedAttack = false;
        }

        public override bool IsValid(IActionReceiver agent, Data data)
        {
            var attacker = agent.Transform.GetComponent<UnitAIBrain>();
            var targetUnit = GetTargetUnit(agent.Transform, data.Target);

            return attacker != null &&
                   attacker.IsAlive &&
                   targetUnit != null &&
                   targetUnit.IsAlive &&
                   attacker.IsAttackOrderTarget(targetUnit) &&
                   attacker.CanAttack(targetUnit);
        }

        public override IActionRunState Perform(IMonoAgent agent, Data data, IActionContext context)
        {
            if (!agent.transform.TryGetComponent<UnitAIBrain>(out var attacker) || !attacker.IsAlive)
            {
                return ActionRunState.Stop;
            }

            data.TargetUnit ??= GetTargetUnit(agent.transform, data.Target);

            if (data.TargetUnit == null ||
                !data.TargetUnit.IsAlive ||
                !attacker.IsAttackOrderTarget(data.TargetUnit) ||
                !attacker.CanAttack(data.TargetUnit))
            {
                attacker.CompleteAttackOrder();
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
                attacker.CompleteAttackOrder();
                return ActionRunState.Completed;
            }

            data.HasStartedAttack = false;
            data.AttackTimer = null;
            return ActionRunState.Continue;
        }

        private static IUnit GetTargetUnit(Transform attackerTransform, ITarget target)
        {
            if (target is TransformTarget transformTarget && transformTarget.Transform != null)
            {
                return transformTarget.Transform.GetComponentInParent<IUnit>();
            }

            if (attackerTransform != null &&
                attackerTransform.TryGetComponent<UnitAIBrain>(out var attacker) &&
                attacker.TryGetAttackOrderTarget(out var orderedTarget))
            {
                return orderedTarget;
            }

            return null;
        }
    }
}
