using AI.Goap.UnitAI.Capabilitys;
using CrashKonijn.Goap.Core;
using CrashKonijn.Goap.Runtime;

namespace AI.Goap.UnitAI.Factories
{
    public class UnitAgentTypeConfigFactory : AgentTypeFactoryBase
    {
        public override IAgentTypeConfig Create()
        {
            var builder = this.CreateBuilder("IdleAgent");
            
            builder.AddCapability<IdleCapability>();
            builder.AddCapability<WanderCapability>();
            builder.AddCapability<MoveOrderCapability>();
            
            return builder.Build();

        }
    }
}