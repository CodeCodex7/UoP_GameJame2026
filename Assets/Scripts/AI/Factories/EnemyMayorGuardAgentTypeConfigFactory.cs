using AI.Goap.UnitAI.Capabilitys;
using CrashKonijn.Goap.Core;
using CrashKonijn.Goap.Runtime;

namespace AI.Goap.UnitAI.Factories
{
    public class EnemyMayorGuardAgentTypeConfigFactory : AgentTypeFactoryBase
    {
        public override IAgentTypeConfig Create()
        {
            var builder = this.CreateBuilder("EnemyMayorGuard");

            builder.AddCapability<IdleCapability>();
            builder.AddCapability<MoveOrderCapability>();
            builder.AddCapability<AttackOrderCapability>();

            return builder.Build();
        }
    }
}
