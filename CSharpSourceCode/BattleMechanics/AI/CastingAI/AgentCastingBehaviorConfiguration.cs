﻿using System;
using System.Collections.Generic;
using System.Linq;
using TaleWorlds.MountAndBlade;
using TOR_Core.AbilitySystem;
using TOR_Core.Extensions;
using TOR_Core.BattleMechanics.AI.CastingAI.AgentCastingBehavior;
using TOR_Core.BattleMechanics.AI.CommonAIFunctions;

namespace TOR_Core.BattleMechanics.AI.CastingAI
{
    public static class AgentCastingBehaviorConfiguration
    {
        public static readonly Dictionary<AbilityEffectType, Func<Agent, int, AbilityTemplate, AbstractAgentCastingBehavior>> BehaviorByType =
            new Dictionary<AbilityEffectType, Func<Agent, int, AbilityTemplate, AbstractAgentCastingBehavior>>
            {
                {AbilityEffectType.Blast, (agent, abilityIndex, abilityTemplate) => new AoETargetedCastingBehavior(agent, abilityTemplate, abilityIndex)},
                {AbilityEffectType.Bombardment, (agent, abilityIndex, abilityTemplate) => new AoETargetedCastingBehavior(agent, abilityTemplate, abilityIndex)},
                {AbilityEffectType.Vortex, (agent, abilityIndex, abilityTemplate) => new AoETargetedCastingBehavior(agent, abilityTemplate, abilityIndex)},

                {
                    AbilityEffectType.Heal, (agent, abilityIndex, abilityTemplate) =>
                    {
                        if (abilityTemplate.AbilityTargetType == AbilityTargetType.AlliesInAOE)
                            return new SelectMultiTargetCastingBehavior(agent, abilityTemplate, abilityIndex);
                        if (abilityTemplate.AbilityTargetType == AbilityTargetType.Self ||
                            abilityTemplate.AbilityTargetType == AbilityTargetType.SingleAlly)
                            return new SelectSingleTargetCastingBehavior(agent, abilityTemplate, abilityIndex);
                        return new SelectMultiTargetCastingBehavior(agent, abilityTemplate, abilityIndex);
                    }
                },
                {
                    AbilityEffectType.Hex, (agent, abilityIndex, abilityTemplate) =>
                    {
                        if (abilityTemplate.AbilityTargetType == AbilityTargetType.EnemiesInAOE)
                            return new SelectMultiTargetCastingBehavior(agent, abilityTemplate, abilityIndex);
                        if (abilityTemplate.AbilityTargetType == AbilityTargetType.SingleEnemy)
                            return new SelectSingleTargetCastingBehavior(agent, abilityTemplate, abilityIndex);
                        return new AoETargetedCastingBehavior(agent, abilityTemplate, abilityIndex);
                    }
                },
                {
                    AbilityEffectType.Augment, (agent, abilityIndex, abilityTemplate) =>
                    {
                        if (abilityTemplate.AbilityTargetType == AbilityTargetType.AlliesInAOE)
                            return new SelectMultiTargetCastingBehavior(agent, abilityTemplate, abilityIndex);
                        if (abilityTemplate.AbilityTargetType == AbilityTargetType.Self ||
                            abilityTemplate.AbilityTargetType == AbilityTargetType.SingleAlly)
                            return new SelectSingleTargetCastingBehavior(agent, abilityTemplate, abilityIndex);
                        return new SelectMultiTargetCastingBehavior(agent, abilityTemplate, abilityIndex);
                    }
                },

                {AbilityEffectType.Missile, (agent, abilityIndex, abilityTemplate) => new MissileCastingBehavior(agent, abilityTemplate, abilityIndex)},
                {AbilityEffectType.SeekerMissile, (agent, abilityIndex, abilityTemplate) => new MissileCastingBehavior(agent, abilityTemplate, abilityIndex)},

                {AbilityEffectType.Summoning, (agent, abilityIndex, abilityTemplate) => new SummoningCastingBehavior(agent, abilityTemplate, abilityIndex)},

                {AbilityEffectType.Wind, (agent, abilityIndex, abilityTemplate) => new AoEDirectionalCastingBehavior(agent, abilityTemplate, abilityIndex)},

                //  {AbilityEffectType.AgentMoving, (agent, abilityIndex, abilityTemplate) => new MovementCastingBehavior(agent, abilityTemplate, abilityIndex)},
                {AbilityEffectType.ArtilleryPlacement, (agent, abilityIndex, abilityTemplate) => new ArtilleryPlacementCastingBehavior(agent, abilityTemplate, abilityIndex)},
            };

        public static List<Target> FindTargets(Agent agent, AbilityTemplate abilityTemplate)
        {
            if (abilityTemplate.AbilityTargetType == AbilityTargetType.AlliesInAOE ||
                abilityTemplate.AbilityEffectType == AbilityEffectType.Heal ||
                abilityTemplate.AbilityTargetType == AbilityTargetType.SingleAlly ||
                abilityTemplate.AssociatedTriggeredEffectTemplates.Count > 0 && abilityTemplate.AssociatedTriggeredEffectTemplates[0].TargetType == TargetType.Friendly)
                return agent.Team.GetAllyTeams()
                    .SelectMany(team => team.GetFormations())
                    .Select(form => new Target {Formation = form})
                    .ToList();
            if (abilityTemplate.AbilityEffectType == AbilityEffectType.Summoning ||
                abilityTemplate.AbilityTargetType == AbilityTargetType.Self)
                return new List<Target>()
                {
                    new Target
                    {
                        Formation = agent.Formation,
                        Agent = agent
                    }
                };

            return agent.Team.GetEnemyTeams()
                .SelectMany(team => team.GetFormations())
                .Select(form => new Target {Formation = form})
                .ToList();
        }

        public static readonly Dictionary<Type, Func<AbstractAgentCastingBehavior, List<Axis>>> UtilityByType =
            new Dictionary<Type, Func<AbstractAgentCastingBehavior, List<Axis>>>
            {
                {typeof(PreserveWindsAgentCastingBehavior), CreatePreserveWindsAxis()},

                {typeof(MissileCastingBehavior), CreateAoETargetedOffensiveSpellAxis()},
                {typeof(AoETargetedCastingBehavior), CreateAoETargetedOffensiveSpellAxis()},
                {typeof(AoEAdjacentCastingBehavior), CreateAoEAdjacentSpellAxis()},
                {typeof(AoEDirectionalCastingBehavior), CreateAoEDirectionalSpellAxis()},

                {typeof(SelectMultiTargetCastingBehavior), CreateBuffSpellAxis()},
                {typeof(SelectSingleTargetCastingBehavior), CreateBuffSpellAxis()},

                {typeof(SummoningCastingBehavior), CreateSummoningAxis()},
                {typeof(ArtilleryPlacementCastingBehavior), CreateArtilleryPlacementAxis()},
            };

        public static List<AbstractAgentCastingBehavior> PrepareCastingBehaviors(Agent agent)
        {
            var castingBehaviors = new List<AbstractAgentCastingBehavior>();
            var index = 0;
            foreach (var knownAbilityTemplate in agent.GetComponent<AbilityComponent>().GetKnownAbilityTemplates())
            {
                castingBehaviors
                    .Add(BehaviorByType.GetValueOrDefault(knownAbilityTemplate.AbilityEffectType, BehaviorByType[AbilityEffectType.Missile])
                        .Invoke(agent, index, knownAbilityTemplate));
                index++;
            }

            castingBehaviors.Add(new PreserveWindsAgentCastingBehavior(agent, new AbilityTemplate {AbilityTargetType = AbilityTargetType.Self}, index));
            return castingBehaviors;
        }

        private static Func<AbstractAgentCastingBehavior, List<Axis>> CreateSummoningAxis()
        {
            return behavior =>
            {
                var axes = new List<Axis>();

                axes.Add(new Axis(0, 1f, x => 1 - x, CommonAIDecisionFunctions.BalanceOfPower(behavior.Agent)));

                return axes;
            };
        }

        private static Func<AbstractAgentCastingBehavior, List<Axis>> CreateArtilleryPlacementAxis()
        {
            return behavior =>
            {
                var axes = new List<Axis>();

                axes.Add(new Axis(0, 100f, x => 1 - x, CommonAIDecisionFunctions.DistanceToTarget(() => behavior.Agent.Team.QuerySystem.MedianPosition.GetGroundVec3MT())));
                axes.Add(new Axis(0, 70f, x => x, CommonAIDecisionFunctions.TargetDistanceToHostiles(behavior.Agent.Team)));
                axes.Add(new Axis(0, 1, x => x, CommonAIDecisionFunctions.AssessPositionForArtillery()));

                return axes;
            };
        }

        private static Func<AbstractAgentCastingBehavior, List<Axis>> CreateAoEAdjacentSpellAxis()
        {
            return behavior =>
            {
                var axes = new List<Axis>
                {
                    new Axis(0, 1, x => x, (target) => 0.45f)
                };

                if (behavior.AbilityTemplate.AbilityTargetType != AbilityTargetType.Self)
                {
                    axes.Add(new Axis(0, 100, x => 1 - x, CommonAIDecisionFunctions.DistanceToTarget(() => behavior.Agent.Position)));
                    axes.Add(new Axis(0, 5, x => 1 - x, CommonAIDecisionFunctions.TargetDistanceToHostiles()));
                    axes.Add(new Axis(0, 3, x => 1 - x + 0.1f, CommonAIDecisionFunctions.TargetSpeed()));
                    axes.Add(new Axis(0, 0.5f, x => x + 0.01f, CommonAIDecisionFunctions.FormationUnderFire()));
                }

                return axes;
            };
        }

        private static Func<AbstractAgentCastingBehavior, List<Axis>> CreatePreserveWindsAxis()
        {
            return behavior =>
            {
                return new List<Axis>
                {
                    new Axis(0, 1, x => (float)Math.Min(0.4, 1 - x), CommonAIDecisionFunctions.WindsOfMagicRemainingRatio(behavior.Agent))
                };
            };
        }

        public static Func<AbstractAgentCastingBehavior, List<Axis>> CreateAoETargetedOffensiveSpellAxis()
        {
            return behavior =>
            {
                return new List<Axis>
                {
                    new Axis(0, 120, x => 1 - x, CommonAIDecisionFunctions.DistanceToTarget(() => behavior.Agent.Position)),
                    new Axis(0, CommonAIDecisionFunctions.CalculateEnemyTotalPower(behavior.Agent.Team) / 4, x => x, CommonAIDecisionFunctions.FormationPower()),
                    new Axis(0.0f, 1, x => x + 0.3f, CommonAIDecisionFunctions.RangedUnitRatio()),
                };
            };
        }

        public static Func<AbstractAgentCastingBehavior, List<Axis>> CreateBuffSpellAxis()
        {
            return behavior =>
            {
                return new List<Axis>
                {
                    new Axis(0, 50, x => ScoringFunctions.Logistic(0.4f, 1, 20).Invoke(1 - x), CommonAIDecisionFunctions.DistanceToTarget(() => behavior.Agent.Position)),
                    new Axis(0, 15, x => 1 - x, CommonAIDecisionFunctions.TargetDistanceToHostiles()),
                    new Axis(0, CommonAIDecisionFunctions.CalculateTeamTotalPower(behavior.Agent.Team), x => x, CommonAIDecisionFunctions.FormationPower()),
                    new Axis(1, 2.5f, x => 1 - x, CommonAIDecisionFunctions.Dispersedness()),
                };
            };
        }

        public static Func<AbstractAgentCastingBehavior, List<Axis>> CreateAoEDirectionalSpellAxis()
        {
            return behavior =>
            {
                return new List<Axis>
                {
                    new Axis(0, 50, x => ScoringFunctions.Logistic(0.4f, 1, 20).Invoke(1 - x), CommonAIDecisionFunctions.DistanceToTarget(() => behavior.Agent.Position)),
                    new Axis(0, 15, x => 1 - x, CommonAIDecisionFunctions.TargetDistanceToHostiles()),
                    new Axis(0, CommonAIDecisionFunctions.CalculateEnemyTotalPower(behavior.Agent.Team), x => x, CommonAIDecisionFunctions.FormationPower()),
                    new Axis(1, 2.5f, x => 1 - x, CommonAIDecisionFunctions.Dispersedness()),
                    new Axis(0, 1, x => 1 - x, CommonAIDecisionFunctions.CavalryUnitRatio()),
                };
            };
        }
    }
}
