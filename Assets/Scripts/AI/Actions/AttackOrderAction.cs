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
            public IDamage DamageTarget { get; set; }
        }

        public override void Created()
        {
        }

        public override void Start(IMonoAgent agent, Data data)
        {
            data.DamageTarget = GetDamageTarget(agent.transform, data.Target);
            data.AttackTimer = null;
            data.HasStartedAttack = false;
        }

        public override bool IsValid(IActionReceiver agent, Data data)
        {
            var attacker = agent.Transform.GetComponent<UnitAIBrain>();
            var damageTarget = GetDamageTarget(agent.Transform, data.Target);

            return attacker != null &&
                   attacker.IsAlive &&
                   damageTarget != null &&
                   damageTarget.IsAlive &&
                   attacker.IsAttackOrderTarget(damageTarget) &&
                   attacker.CanAttack(damageTarget);
        }

        public override IActionRunState Perform(IMonoAgent agent, Data data, IActionContext context)
        {
            if (!agent.transform.TryGetComponent<UnitAIBrain>(out var attacker) || !attacker.IsAlive)
            {
                return ActionRunState.Stop;
            }

            data.DamageTarget ??= GetDamageTarget(agent.transform, data.Target);

            if (data.DamageTarget == null ||
                !data.DamageTarget.IsAlive ||
                !attacker.IsAttackOrderTarget(data.DamageTarget) ||
                !attacker.CanAttack(data.DamageTarget))
            {
                attacker.CompleteAttackOrder();
                return ActionRunState.Completed;
            }

            if (!context.IsInRange || !attacker.IsTargetInAttackRange(data.DamageTarget))
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

            if (!attacker.IsTargetInAttackRange(data.DamageTarget))
            {
                data.HasStartedAttack = false;
                data.AttackTimer = null;
                return ActionRunState.Continue;
            }

            data.DamageTarget.TakeDamage(attacker.AttackDamage);

            if (!data.DamageTarget.IsAlive)
            {
                var targetName = UnitAIBrain.TryGetDamageTransform(data.DamageTarget, out var targetTransform)
                    ? targetTransform.name
                    : data.DamageTarget.ToString();

                Debug.Log($"{attacker.name} defeated {targetName}");
                attacker.CompleteAttackOrder();
                return ActionRunState.Completed;
            }

            data.HasStartedAttack = false;
            data.AttackTimer = null;
            return ActionRunState.Continue;
        }

        private static IDamage GetDamageTarget(Transform attackerTransform, ITarget target)
        {
            if (target is TransformTarget transformTarget && transformTarget.Transform != null)
            {
                return transformTarget.Transform.GetComponentInParent<IDamage>();
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
