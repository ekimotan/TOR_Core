﻿using TaleWorlds.MountAndBlade;
using TOR_Core.AbilitySystem;
using TOR_Core.BattleMechanics.AI.CastingAI.AgentTacticalBehavior;
using TOR_Core.BattleMechanics.AI.CastingAI.Components;
using TOR_Core.BattleMechanics.AI.CommonAIFunctions;

namespace TOR_Core.BattleMechanics.AI.CastingAI.AgentCastingBehavior
{
    public class AoEAdjacentCastingBehavior : AbstractAgentCastingBehavior
    {
        public AoEAdjacentCastingBehavior(Agent agent, AbilityTemplate template, int abilityIndex) : base(agent, template, abilityIndex)
        {
            TacticalBehavior = new AdjacentAoETacticalBehavior(agent, agent.GetComponent<WizardAIComponent>(), this);
        }

        public override void Execute()
        {
            var castingPosition = ((AdjacentAoETacticalBehavior) TacticalBehavior)?.CastingPosition;
            if (castingPosition.HasValue && Agent.Position.AsVec2.Distance(castingPosition.Value.AsVec2) > 5) return;

            base.Execute();
        }

        protected override float CalculateUtility(Target target)
        {
            if (!CommonAIStateFunctions.CanAgentMoveFreely(Agent))
                return 0.0f;
            
            return base.CalculateUtility(target);
        }
    }
}