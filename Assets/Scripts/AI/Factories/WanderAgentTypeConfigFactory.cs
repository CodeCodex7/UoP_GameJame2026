using AI.Goap.UnitAI.Capabilitys;
using CrashKonijn.Goap.Core;
using CrashKonijn.Goap.Runtime;

namespace AI.Goap.UnitAI.Factories
{
    public class WanderAgentTypeConfigFactory : AgentTypeFactoryBase
    {
        public override IAgentTypeConfig Create()
        {
            var builder = this.CreateBuilder("WonderAI");
            
            builder.AddCapability<IdleCapability>();
            builder.AddCapability<WanderCapability>();
            
            return builder.Build();
            
        }
    }
}