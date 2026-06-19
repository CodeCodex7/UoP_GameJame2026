using AI.Goap.UnitAI.Capabilitys;
using CrashKonijn.Goap.Core;
using CrashKonijn.Goap.Runtime;

namespace AI.Goap.UnitAI.Factories
{
    public class MayorAgentTypeConfigFactory : AgentTypeFactoryBase
    {
        public override IAgentTypeConfig Create()
        {
            var builder = this.CreateBuilder("Mayor");

            builder.AddCapability<IdleCapability>();
            builder.AddCapability<MoveOrderCapability>();
            builder.AddCapability<AttackOrderCapability>();
            builder.AddCapability<HarvestOrderCapability>();
            builder.AddCapability<DepositResourceCapability>();

            return builder.Build();
        }
    }
}
