﻿using Helpers;
using SandBox.GameComponents;
using System;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Party;
using TaleWorlds.Core;
using TaleWorlds.Library;
using TaleWorlds.MountAndBlade;
using TOR_Core.AbilitySystem;
using TOR_Core.Battle.CrosshairMissionBehavior;
using TOR_Core.BattleMechanics.Crosshairs;
using TOR_Core.CharacterDevelopment;
using TOR_Core.Extensions;

namespace TOR_Core.Models
{
    public class TORAgentStatCalculateModel : SandboxAgentStatCalculateModel
    {
        private float vampireDaySpeedModificator = 1.1f;
        private float vampireNightSpeedModificator = 1.2f;
        private CustomCrosshairMissionBehavior _crosshairBehavior;


        public override void InitializeAgentStats(Agent agent, Equipment spawnEquipment, AgentDrivenProperties agentDrivenProperties, AgentBuildData agentBuildData)
        {
            base.InitializeAgentStats(agent, spawnEquipment, agentDrivenProperties, agentBuildData);
            UpdateAgentDrivenProperties(agent, agentDrivenProperties);
        }

        public override void UpdateAgentStats(Agent agent, AgentDrivenProperties agentDrivenProperties)
        {
            base.UpdateAgentStats(agent, agentDrivenProperties);
            UpdateAgentDrivenProperties(agent, agentDrivenProperties);
        }

        public override float GetWeaponInaccuracy(Agent agent, WeaponComponentData weapon, int weaponSkill)
        {
            var result = base.GetWeaponInaccuracy(agent, weapon, weaponSkill);
            ExplainedNumber accuracy = new ExplainedNumber(result, false, null);
            var character = agent.Character as CharacterObject;
            var captain = agent.GetPartyLeaderCharacter();
            if(character != null)
            {
                if (weapon.IsRangedWeapon && weapon.RelevantSkill == TORSkills.GunPowder)
                {
                    SkillHelper.AddSkillBonusForCharacter(TORSkills.GunPowder, TORSkillEffects.GunAccuracy, character, ref accuracy, weaponSkill, false, 0);
                    if (agent.HasMount)
                    {
                        PerkHelper.AddPerkBonusForCharacter(TORPerks.GunPowder.MountedHeritage, character, true, ref accuracy);
                    }
                    if(weapon.WeaponClass == WeaponClass.Musket)
                    {
                        PerkHelper.AddPerkBonusFromCaptain(TORPerks.GunPowder.DeadEye, captain, ref accuracy);
                    }

                }
            }
            return accuracy.ResultNumber;
        }

        public override void InitializeMissionEquipment(Agent agent)
        {
            if (agent.Origin is SummonedAgentOrigin) return;
            base.InitializeMissionEquipment(agent);
            if (agent.IsHuman)
            {
                var character = agent.Character as CharacterObject;
                var mobileParty = agent.GetOriginMobileParty();
                if (character != null && mobileParty != null)
                {
                    MissionEquipment equipment = agent.Equipment;
                    for (int i = 0; i < 5; i++)
                    {
                        EquipmentIndex equipmentIndex = (EquipmentIndex)i;
                        MissionWeapon missionWeapon = equipment[equipmentIndex];
                        if (!missionWeapon.IsEmpty)
                        {
                            WeaponComponentData currentUsageItem = missionWeapon.CurrentUsageItem;
                            if (currentUsageItem != null && currentUsageItem.IsAmmo && currentUsageItem.RelevantSkill != null)
                            {
                                ExplainedNumber ammoCount = new ExplainedNumber(missionWeapon.Amount);
                                if (currentUsageItem.RelevantSkill == TORSkills.GunPowder && currentUsageItem.WeaponClass == WeaponClass.Cartridge)
                                {
                                    PerkHelper.AddPerkBonusForParty(TORPerks.GunPowder.AmmoWagons, mobileParty, true, ref ammoCount);
                                }
                                var result = MathF.Round(ammoCount.ResultNumber);
                                if(result != missionWeapon.Amount)
                                {
                                    equipment.SetAmountOfSlot(equipmentIndex, (short)result, true);
                                }
                            }
                        }
                    }
                }
            }
        }

        public override int GetEffectiveSkill(BasicCharacterObject agentCharacter, IAgentOriginBase agentOrigin, Formation agentFormation, SkillObject skill)
        {
            if (agentOrigin is SummonedAgentOrigin) return agentCharacter.GetSkillValue(skill);
            var result = base.GetEffectiveSkill(agentCharacter, agentOrigin, agentFormation, skill);
            ExplainedNumber resultNumber = new ExplainedNumber(result, false, null);

            var partyBase = ((agentOrigin != null) ? agentOrigin.BattleCombatant : null) as PartyBase;
            if (partyBase != null && partyBase.IsMobile)
            {
                var mobileParty = partyBase.MobileParty;
                if (mobileParty != null)
                {
                    if (skill == TORSkills.GunPowder && agentCharacter.Equipment.HasWeaponOfClass(WeaponClass.Cartridge))
                    {
                        PerkHelper.AddPerkBonusForParty(TORPerks.GunPowder.RunAndGun, mobileParty, false, ref resultNumber);
                    }
                    if (skill == DefaultSkills.OneHanded && agentCharacter.Equipment.HasWeaponOfClass(WeaponClass.Cartridge))
                    {
                        PerkHelper.AddPerkBonusForParty(TORPerks.GunPowder.CloseQuarters, mobileParty, false, ref resultNumber);
                    }
                    if (skill == DefaultSkills.Riding && agentCharacter.IsMounted && agentCharacter.Equipment.HasWeaponOfClass(WeaponClass.Cartridge))
                    {
                        PerkHelper.AddPerkBonusForParty(TORPerks.GunPowder.MountedHeritage, mobileParty, false, ref resultNumber);
                    }
                }
            }

            return (int)resultNumber.ResultNumber;
        }

        public override string GetMissionDebugInfoForAgent(Agent agent)
        {
            if (agent.Origin is SummonedAgentOrigin) return "Impossible to debug summoned units. Base implementation has invalid IAgentOriginBase to PartyBase type cast.";
            else return base.GetMissionDebugInfoForAgent(agent);
        }

        public override float GetEffectiveMaxHealth(Agent agent)
        {
            if (agent.Origin is SummonedAgentOrigin) return agent.BaseHealthLimit;
            else return base.GetEffectiveMaxHealth(agent);
        }

        private void UpdateAgentDrivenProperties(Agent agent, AgentDrivenProperties agentDrivenProperties)
        {
            if (agent.IsHuman)
            {
                AddSkillEffectsForAgent(agent, agentDrivenProperties);
                AddPerkEffectsForAgent(agent, agentDrivenProperties);
                var character = agent.Character as CharacterObject;
                if (character != null && character.IsVampire())
                {
                    float modificator = vampireDaySpeedModificator;
                    if (Campaign.Current != null && Campaign.Current.IsNight)
                    {
                        modificator = vampireNightSpeedModificator;
                    }
                    agentDrivenProperties.TopSpeedReachDuration *= modificator;
                    agentDrivenProperties.MaxSpeedMultiplier *= modificator;
                    agentDrivenProperties.CombatMaxSpeedMultiplier *= modificator;
                }
            }
        }

        private void AddSkillEffectsForAgent(Agent agent, AgentDrivenProperties agentDrivenProperties)
        {
            EquipmentIndex wieldedItemIndex = agent.GetWieldedItemIndex(Agent.HandIndex.MainHand);
            WeaponComponentData weapon = (wieldedItemIndex != EquipmentIndex.None) ? agent.Equipment[wieldedItemIndex].CurrentUsageItem : null;
            var character = agent.Character as CharacterObject;
            var captain = agent.GetCaptainCharacter();
            if (weapon != null && character != null)
            {
                int effectiveSkill = GetEffectiveSkill(character, agent.Origin, agent.Formation, weapon.RelevantSkill);
                ExplainedNumber reloadSpeed = new ExplainedNumber(agentDrivenProperties.ReloadSpeed);
                if(weapon.RelevantSkill == TORSkills.GunPowder)
                {
                    SkillHelper.AddSkillBonusForCharacter(TORSkills.GunPowder, TORSkillEffects.GunReloadSpeed, character, ref reloadSpeed, effectiveSkill, true, 0);
                }
                agentDrivenProperties.ReloadSpeed = reloadSpeed.ResultNumber;
            }
        }

        private void AddPerkEffectsForAgent(Agent agent, AgentDrivenProperties agentDrivenProperties)
        {
            EquipmentIndex wieldedItemIndex = agent.GetWieldedItemIndex(Agent.HandIndex.MainHand);
            WeaponComponentData weapon = (wieldedItemIndex != EquipmentIndex.None) ? agent.Equipment[wieldedItemIndex].CurrentUsageItem : null;
            var character = agent.Character as CharacterObject;
            var captain = agent.GetCaptainCharacter();
            ExplainedNumber movementAccuracyPenalty = new ExplainedNumber(agentDrivenProperties.WeaponMaxMovementAccuracyPenalty);
            if (weapon != null && character != null)
            {
                if (weapon.WeaponClass == WeaponClass.Pistol && !agent.HasMount)
                {
                    PerkHelper.AddPerkBonusForCharacter(TORPerks.GunPowder.RunAndGun, character, true, ref movementAccuracyPenalty);
                }
            }
            agentDrivenProperties.WeaponMaxMovementAccuracyPenalty = movementAccuracyPenalty.ResultNumber;
        }

        public override float GetMaxCameraZoom(Agent agent)
        {
            if (_crosshairBehavior == null)
            {
                _crosshairBehavior = Mission.Current.GetMissionBehavior<CustomCrosshairMissionBehavior>();
            }

            if (_crosshairBehavior != null && _crosshairBehavior.CurrentCrosshair is SniperScope && _crosshairBehavior.CurrentCrosshair.IsVisible)
            {
                return 3;
            }
            return base.GetMaxCameraZoom(agent);
        }
    }
}
