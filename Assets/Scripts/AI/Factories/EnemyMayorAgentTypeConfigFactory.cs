using AI.Goap.UnitAI.Capabilitys;
using CrashKonijn.Goap.Core;
using CrashKonijn.Goap.Runtime;

namespace AI.Goap.UnitAI.Factories
{
    public class EnemyMayorAgentTypeConfigFactory : AgentTypeFactoryBase
    {
        public override IAgentTypeConfig Create()
        {
            var builder = this.CreateBuilder("EnemyMayor");

            builder.AddCapability<IdleCapability>();
            builder.AddCapability<FleeCapability>();
            builder.AddCapability<MoveOrderCapability>();

            return builder.Build();
        }
    }
}
