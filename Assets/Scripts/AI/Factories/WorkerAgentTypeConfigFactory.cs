using AI.Goap.UnitAI.Capabilitys;
using CrashKonijn.Goap.Core;
using CrashKonijn.Goap.Runtime;

namespace AI.Goap.UnitAI.Factories
{
    public class WorkerAgentTypeConfigFactory : AgentTypeFactoryBase
    {
        public override IAgentTypeConfig Create()
        {
            var builder = this.CreateBuilder("Worker");
            
            builder.AddCapability<IdleCapability>();
            builder.AddCapability<WanderCapability>();
            builder.AddCapability<MoveOrderCapability>();
            builder.AddCapability<HarvestNearbyCapability>();
            builder.AddCapability<DepositResourceCapability>();
            
            return builder.Build();

        }
    }
}
