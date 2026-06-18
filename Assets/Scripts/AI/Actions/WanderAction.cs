using CrashKonijn.Agent.Core;
using CrashKonijn.Agent.Runtime;
using CrashKonijn.Goap.Runtime;
using UnityEngine;

namespace AI.Goap.UnitAI.Actions
{
    public class WanderAction : GoapActionBase<WanderAction.Data>
    {
        public class Data : IActionData
        {
            public ITarget Target { get; set; }
            public float Timer { get; set; }
        };


        public override IActionRunState Perform(IMonoAgent agent, Data data, IActionContext context)
        {
            return data.Timer < 0 ? ActionRunState.Continue : ActionRunState.Completed;
        }
    }
}