﻿using TaleWorlds.Core;
using TaleWorlds.Engine;
using TaleWorlds.MountAndBlade;
using TOR_Core.BattleMechanics.AI.CommonAIFunctions;
using TOR_Core.Utilities;

namespace TOR_Core.BattleMechanics.AI.CastingAI.AgentTacticalBehavior
{
    public class KeepSafeAgentTacticalBehavior : AbstractAgentTacticalBehavior
    {
        public KeepSafeAgentTacticalBehavior(Agent agent, HumanAIComponent aiComponent) : base(agent, aiComponent)
        {
        }

        public override void Tick()
        {
            var behavior = Agent.Formation?.AI?.ActiveBehavior;
            if (Agent.Team.GeneralAgent == Agent && Agent.Team.HasTeamAi && behavior != null && behavior.GetType() == typeof(BehaviorCharge))
            {
                Agent.Formation.AI.SetBehaviorWeight<BehaviorCharge>(0);
            }
        }

        public override void Terminate()
        {
        }

        public override void ApplyBehaviorParams()
        {
            var currentOrderType = GetMovementOrderType();
            if (currentOrderType != null && (currentOrderType == OrderType.Charge || currentOrderType == OrderType.ChargeWithTarget))
            {
                // AIComponent.SetDefaultBehaviorParams();
                // AIComponent.SetBehaviorParams(HumanAIComponent.AISimpleBehaviorKind.GoToPos, 3f, 8f, 5f, 20f, 6f);
                // if (ShouldAgentSkirmish())
                // {
                //     AIComponent.SetBehaviorParams(HumanAIComponent.AISimpleBehaviorKind.RangedHorseback, 5f, 7f, 3f, 20f, 5.5f);
                // }
                // else
                // {
                //     AIComponent.SetBehaviorParams(HumanAIComponent.AISimpleBehaviorKind.RangedHorseback, 0.0f, 15f, 0.0f, 30f, 0.0f);
                // }
            }
            else
            {
                AIComponent.SetBehaviorParams(HumanAIComponent.AISimpleBehaviorKind.Melee, 4f, 3f, 1f, 20f, 1f);
                AIComponent.SetBehaviorParams(HumanAIComponent.AISimpleBehaviorKind.ChargeHorseback, 0, 7, 0, 30, 0);
                AIComponent.SetBehaviorParams(HumanAIComponent.AISimpleBehaviorKind.RangedHorseback, 0f, 2.5f, 0f, 10f, 0.0f);
            }
        }

        public override void SetCurrentTarget(Target target)
        {
        }


        protected bool ShouldAgentSkirmish()
        {
            var querySystem = Agent?.Formation?.QuerySystem;
            var allyPower = querySystem?.LocalAllyPower;
            return allyPower < 20 || allyPower < querySystem?.LocalEnemyPower / 2;
        }
    }
}