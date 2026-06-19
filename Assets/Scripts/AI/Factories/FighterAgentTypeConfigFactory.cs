using AI.Goap.UnitAI.Capabilitys;
using CrashKonijn.Goap.Core;
using CrashKonijn.Goap.Runtime;

namespace AI.Goap.UnitAI.Factories
{
    public class FighterAgentTypeConfigFactory : AgentTypeFactoryBase
    {
        public override IAgentTypeConfig Create()
        {
            var builder = this.CreateBuilder("Fighter");

            builder.AddCapability<IdleCapability>();
            builder.AddCapability<MoveOrderCapability>();
            builder.AddCapability<AttackOrderCapability>();
            builder.AddCapability<AttackRivalUnitCapability>();

            return builder.Build();
        }
    }
}
