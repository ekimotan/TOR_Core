﻿using Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.GameComponents;
using TaleWorlds.Core;
using TOR_Core.CharacterDevelopment;
using TOR_Core.CharacterDevelopment.CareerSystem;
using TOR_Core.Extensions;

namespace TOR_Core.Models
{
    public class TORStrikeMagnitudeModel : SandboxStrikeMagnitudeModel
    {
        public override float CalculateAdjustedArmorForBlow(float baseArmor, BasicCharacterObject attackerCharacter, BasicCharacterObject attackerCaptainCharacter, BasicCharacterObject victimCharacter, BasicCharacterObject victimCaptainCharacter, WeaponComponentData weaponComponent)
        {
            var result = base.CalculateAdjustedArmorForBlow(baseArmor, attackerCharacter, attackerCaptainCharacter, victimCharacter, victimCaptainCharacter, weaponComponent);
            ExplainedNumber resultArmor = new ExplainedNumber(baseArmor);
            var attacker = attackerCharacter as CharacterObject;
            if(attacker != null && attacker.GetPerkValue(TORPerks.GunPowder.PiercingShots) && weaponComponent.IsGunPowderWeapon())
            {
                PerkHelper.AddPerkBonusForCharacter(TORPerks.GunPowder.PiercingShots, attacker, true, ref resultArmor);
            }
            
            if(attacker != null && attacker.IsPlayerCharacter)
            {
                CareerHelper.ApplyBasicCareerPassives(attacker.HeroObject, ref resultArmor, PassiveEffectType.ArmorPenetration, true);
            }
            return resultArmor.ResultNumber;
        }
    }
}
