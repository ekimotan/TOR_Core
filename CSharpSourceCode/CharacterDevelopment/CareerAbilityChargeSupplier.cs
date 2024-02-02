﻿using System;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Party;
using TaleWorlds.MountAndBlade;
using TaleWorlds.TwoDimension;
using TOR_Core.AbilitySystem;
using TOR_Core.CharacterDevelopment.CareerSystem;
using TOR_Core.Extensions;
using TOR_Core.Extensions.ExtendedInfoSystem;

namespace TOR_Core.CharacterDevelopment
{
    public static class CareerAbilityChargeSupplier
    {
        public static float WitchHunterCareerCharge(Agent affectingAgent, Agent affectedAgent, ChargeType chargeType, int chargeValue, AttackTypeMask mask = AttackTypeMask.Melee, CareerHelper.ChargeCollisionFlag collisionFlag = CareerHelper.ChargeCollisionFlag.None)
        {
            
            if (chargeType != ChargeType.DamageDone) return 0;
            if (!affectingAgent.BelongsToMainParty()) return 0;

            if (affectingAgent.IsMainAgent || ( affectingAgent.IsHero && Hero.MainHero.HasCareerChoice("GuiltyByAssociationKeystone") ))
            {
                ExplainedNumber explainedNumber = new ExplainedNumber(0);
            
                if (mask == AttackTypeMask.Ranged || mask == AttackTypeMask.Melee && Hero.MainHero.HasCareerChoice("HuntTheWickedKeystone"))
                {
                    if (mask == AttackTypeMask.Ranged)
                    {
                        explainedNumber.AddFactor(-0.5f);
                    }

                    if (collisionFlag == CareerHelper.ChargeCollisionFlag.HeadShot)
                    {
                        explainedNumber.AddFactor(1f);
                    }

                    explainedNumber.Add(chargeValue);
                }

                return explainedNumber.ResultNumber;
                
            }
            
            return 0;
        }

        public static float NecromancerCareerCharge(Agent affectingAgent, Agent affectedAgent, ChargeType chargeType, int chargeValue, AttackTypeMask mask = AttackTypeMask.Melee, CareerHelper.ChargeCollisionFlag collisionFlag = CareerHelper.ChargeCollisionFlag.None)
        {
            if (!affectingAgent.BelongsToMainParty()) return 0;
            if (mask == AttackTypeMask.Ranged) return 0;

            if (mask == AttackTypeMask.Melee && !affectingAgent.IsUndead())
            {
                return 0;
            }

            if (( chargeType != ChargeType.DamageDone && chargeType != ChargeType.Healed )) return 0;

            ExplainedNumber explainedNumber = new ExplainedNumber();

            explainedNumber.Add(chargeValue);

            if (mask == AttackTypeMask.Spell)
            {
                explainedNumber.AddFactor(-0.9f);
            }

            explainedNumber.Add(chargeValue);
            return explainedNumber.ResultNumber;
        }
        
        public static float NecrarchCareerCharge(Agent affectingAgent, Agent affectedAgent, ChargeType chargeType, int chargeValue, AttackTypeMask mask = AttackTypeMask.Melee, CareerHelper.ChargeCollisionFlag collisionFlag = CareerHelper.ChargeCollisionFlag.None)
        {
            if (chargeType != ChargeType.DamageDone && chargeType != ChargeType.Healed) return 0;
            ExplainedNumber explainedNumber = new ExplainedNumber();
            
            if (!affectingAgent.IsHero && affectingAgent.IsUndead() && Hero.MainHero.HasCareerChoice("DiscipleOfAccursedKeystone"))
            {
                explainedNumber.Add(chargeValue);
                explainedNumber.AddFactor(-0.75f);
                return explainedNumber.ResultNumber;
            }

            if (!affectingAgent.IsHero || mask != AttackTypeMask.Spell) return explainedNumber.ResultNumber;
            explainedNumber.Add(chargeValue);
            if (!affectingAgent.IsMainAgent && !Hero.MainHero.HasCareerChoice("HungerForKnowledgeKeystone")) 
                return 0;
            

            return explainedNumber.ResultNumber;
        }
        
        public static float GrailDamselCareerCharge(Agent affectingAgent, Agent affectedAgent, ChargeType chargeType, int chargeValue, AttackTypeMask mask = AttackTypeMask.Melee, CareerHelper.ChargeCollisionFlag collisionFlag = CareerHelper.ChargeCollisionFlag.None)
        {
            if (chargeType != ChargeType.DamageDone || chargeType != ChargeType.Healed) return 0;

            if (mask != AttackTypeMask.Spell) return 0;

            if (!affectingAgent.IsHero) return 0;


            ExplainedNumber explainedNumber = new ExplainedNumber();

            if (affectingAgent.GetOriginMobileParty() != MobileParty.MainParty) return 0;


            if (!affectingAgent.IsMainAgent && !Hero.MainHero.HasCareerChoice("InspirationOfTheLadyKeystone")) return 0;

            explainedNumber.Add(chargeValue);

            explainedNumber.AddFactor(-0.9f);

            var choices = Hero.MainHero.GetAllCareerChoices();

            if (choices.Contains("VividVisionsKeystone"))
            {
                var choice = TORCareerChoices.GetChoice("VividVisionsKeystone");
                if (choice != null)
                {
                    var value = choice.GetPassiveValue();
                    explainedNumber.AddFactor(value);
                }
            }

            if (choices.Contains("InspirationOfTheLadyKeystone"))
            {
                var choice = TORCareerChoices.GetChoice("InspirationOfTheLadyKeystone");
                if (choice != null)
                {
                    explainedNumber.AddFactor(-0.05f); // Originally only 10% of charge is taken into account, now it would be 5% 
                }
            }

            return explainedNumber.ResultNumber;
        }

        public static float MinorVampireCareerCharge(Agent affectingAgent, Agent affectedAgent, ChargeType chargeType, int chargeValue, AttackTypeMask mask = AttackTypeMask.Melee, CareerHelper.ChargeCollisionFlag collisionFlag = CareerHelper.ChargeCollisionFlag.None)
        {
            if (affectingAgent.GetOriginMobileParty() != MobileParty.MainParty) return 0;
            if (chargeType != ChargeType.DamageDone) return 0;

            ExplainedNumber explainedNumber = new ExplainedNumber();

            explainedNumber.Add(chargeValue);

            if (mask == AttackTypeMask.Spell && Hero.MainHero.GetAllCareerChoices().Contains("ArkayneKeystone"))
            {
                explainedNumber.AddFactor(-0.9f);
            }

            if (Hero.MainHero.GetAllCareerChoices().Contains("NewBloodKeystone"))
            {
                var choice = TORCareerChoices.GetChoice("NewBloodKeystone");
                if (choice != null)
                {
                    var value = choice.GetPassiveValue();
                    explainedNumber.AddFactor(value);
                }
            }

            if (Hero.MainHero.GetAllCareerChoices().Contains("MartialleKeystone"))
            {
                var choice = TORCareerChoices.GetChoice("MartialleKeystone");
                if (choice != null)
                {
                    var value = choice.GetPassiveValue();
                    explainedNumber.AddFactor(value);
                }
            }

            return explainedNumber.ResultNumber;
        }

        public static float BloodKnightCareerCharge(Agent affectingAgent, Agent affectedAgent, ChargeType chargeType, int chargeValue, AttackTypeMask mask = AttackTypeMask.Melee, CareerHelper.ChargeCollisionFlag collisionFlag = CareerHelper.ChargeCollisionFlag.None)
        {
            if (chargeType != ChargeType.NumberOfKills) return 0;

            if (!affectingAgent.IsHero) return 0;
            
            var explainedNumber = new ExplainedNumber();   //charge value is always 1, this is added with the calculated value  below
            
            var maxCharge = Hero.MainHero.GetCareer().MaxCharge;
            
            var malus = 0f;
            
            if (Hero.MainHero.HasCareerChoice("NightRiderKeystone"))
            {
                malus += 1;
            }
            
            if (Hero.MainHero.HasCareerChoice("BladeMasterKeystone"))
            {
                malus += 1;
            }
            
            if (Hero.MainHero.HasCareerChoice("DoomRiderKeystone"))
            {
                malus += 1;
            }
            
            if (Hero.MainHero.HasCareerChoice("AvatarOfDeathKeystone"))
            {
                malus += 1;
            }
            
            if (Hero.MainHero.HasCareerChoice("ControlledHungerKeystone"))
            {
                malus += 1;
            }
            
            if (Hero.MainHero.HasCareerChoice("DreadKnightKeystone"))
            {
                malus += 1;
            }
            
            if (Hero.MainHero.HasCareerChoice("PeerlessWarriorKeystone"))
            {
                malus += 1;
            }
            
            malus = Math.Min(5, malus);
            
            var change = maxCharge / ( 5 + malus );

           explainedNumber.Add(change);
            
            return explainedNumber.ResultNumber;
        }

        public static float WarriorPriestCareerCharge(Agent affectingAgent, Agent affectedAgent, ChargeType chargeType, int chargeValue, AttackTypeMask mask = AttackTypeMask.Melee, CareerHelper.ChargeCollisionFlag collisionFlag = CareerHelper.ChargeCollisionFlag.None)
        {
            if (mask != AttackTypeMask.Melee) return 0;
            if (chargeType == ChargeType.DamageTaken && ( !affectingAgent.IsMainAgent || !Hero.MainHero.HasCareerChoice("BookOfSigmarKeystone") )) return 0;

            ExplainedNumber explainedNumber = new ExplainedNumber();
            explainedNumber.Add(chargeValue);

            explainedNumber.Add((float)chargeValue / Hero.MainHero.MaxHitPoints); //proportion of lost health 

            if (collisionFlag == CareerHelper.ChargeCollisionFlag.HitShield)
            {
                explainedNumber.AddFactor(-0.85f);
            }

            return explainedNumber.ResultNumber;
        }
    }
}