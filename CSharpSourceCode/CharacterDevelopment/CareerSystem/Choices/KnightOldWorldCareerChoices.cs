﻿using System.Collections.Generic;
using System.Linq;
using TaleWorlds.CampaignSystem;
using TaleWorlds.Core;
using TaleWorlds.MountAndBlade;
using TOR_Core.AbilitySystem;
using TOR_Core.BattleMechanics.DamageSystem;
using TOR_Core.BattleMechanics.StatusEffect;
using TOR_Core.CampaignMechanics.Choices;
using TOR_Core.CampaignMechanics.Religion;
using TOR_Core.Extensions;
using TOR_Core.Extensions.ExtendedInfoSystem;
using TOR_Core.Items;
using TOR_Core.Utilities;

namespace TOR_Core.CharacterDevelopment.CareerSystem.Choices;

public class KnightOldWorldCareerChoices(CareerObject id) : TORCareerChoicesBase(id)
{
    private CareerChoiceObject _knightOldWorldRoot;
    
    private CareerChoiceObject _secularOrdersPassive1;
    private CareerChoiceObject _secularOrdersPassive2;
    private CareerChoiceObject _secularOrdersPassive3;
    private CareerChoiceObject _secularOrdersPassive4;
    private CareerChoiceObject _secularOrdersKeystone;

    private CareerChoiceObject _pathOfConquestPassive1;
    private CareerChoiceObject _pathOfConquestPassive2;
    private CareerChoiceObject _pathOfConquestPassive3;
    private CareerChoiceObject _pathOfConquestPassive4;
    private CareerChoiceObject _pathOfConquestKeystone;

    private CareerChoiceObject _squiresPassive1;
    private CareerChoiceObject _squiresPassive2;
    private CareerChoiceObject _squiresPassive3;
    private CareerChoiceObject _squiresPassive4;
    private CareerChoiceObject _squiresKeystone;


    private CareerChoiceObject _templarOrdersKeystone;
    private CareerChoiceObject _templarOrdersPassive1;
    private CareerChoiceObject _templarOrdersPassive2;
    private CareerChoiceObject _templarOrdersPassive3;
    private CareerChoiceObject _templarOrdersPassive4;
    
    private CareerChoiceObject _pathOfViliganceKeystone;
    private CareerChoiceObject _pathOfViligancePassive1;
    private CareerChoiceObject _pathOfViligancePassive2;
    private CareerChoiceObject _pathOfViligancePassive3;
    private CareerChoiceObject _pathOfViligancePassive4;

    private CareerChoiceObject _wrathAgainstChaosKeystone;
    private CareerChoiceObject _wrathAgainstChaosPassive1;
    private CareerChoiceObject _wrathAgainstChaosPassive2;
    private CareerChoiceObject _wrathAgainstChaosPassive3;
    private CareerChoiceObject _wrathAgainstChaosPassive4;

    private CareerChoiceObject _pathOfGloryKeystone;
    private CareerChoiceObject _pathOfGloryPassive1;
    private CareerChoiceObject _pathOfGloryPassive2;
    private CareerChoiceObject _pathOfGloryPassive3;
    private CareerChoiceObject _pathOfGloryPassive4;


    protected override void RegisterAll()
    {
        _knightOldWorldRoot = Game.Current.ObjectManager.RegisterPresumedObject(new CareerChoiceObject("KnightOldWorldRoot"));
        
        _secularOrdersKeystone = Game.Current.ObjectManager.RegisterPresumedObject(new CareerChoiceObject(nameof(_secularOrdersKeystone).UnderscoreFirstCharToUpper()));
        _secularOrdersPassive1 = Game.Current.ObjectManager.RegisterPresumedObject(new CareerChoiceObject(nameof(_secularOrdersPassive1).UnderscoreFirstCharToUpper()));
        _secularOrdersPassive2 = Game.Current.ObjectManager.RegisterPresumedObject(new CareerChoiceObject(nameof(_secularOrdersPassive2).UnderscoreFirstCharToUpper()));
        _secularOrdersPassive3 = Game.Current.ObjectManager.RegisterPresumedObject(new CareerChoiceObject(nameof(_secularOrdersPassive3).UnderscoreFirstCharToUpper()));
        _secularOrdersPassive4 = Game.Current.ObjectManager.RegisterPresumedObject(new CareerChoiceObject(nameof(_secularOrdersPassive4).UnderscoreFirstCharToUpper()));
        
        _pathOfConquestKeystone = Game.Current.ObjectManager.RegisterPresumedObject(new CareerChoiceObject(nameof(_pathOfConquestKeystone).UnderscoreFirstCharToUpper()));
        _pathOfConquestPassive1 = Game.Current.ObjectManager.RegisterPresumedObject(new CareerChoiceObject(nameof(_pathOfConquestPassive1).UnderscoreFirstCharToUpper()));
        _pathOfConquestPassive2 = Game.Current.ObjectManager.RegisterPresumedObject(new CareerChoiceObject(nameof(_pathOfConquestPassive2).UnderscoreFirstCharToUpper()));
        _pathOfConquestPassive3 = Game.Current.ObjectManager.RegisterPresumedObject(new CareerChoiceObject(nameof(_pathOfConquestPassive3).UnderscoreFirstCharToUpper()));
        _pathOfConquestPassive4 = Game.Current.ObjectManager.RegisterPresumedObject(new CareerChoiceObject(nameof(_pathOfConquestPassive4).UnderscoreFirstCharToUpper()));
        
        
        _squiresKeystone = Game.Current.ObjectManager.RegisterPresumedObject(new CareerChoiceObject(nameof(_squiresKeystone).UnderscoreFirstCharToUpper()));
        _squiresPassive1 = Game.Current.ObjectManager.RegisterPresumedObject(new CareerChoiceObject(nameof(_squiresPassive1).UnderscoreFirstCharToUpper()));
        _squiresPassive2 = Game.Current.ObjectManager.RegisterPresumedObject(new CareerChoiceObject(nameof(_squiresPassive2).UnderscoreFirstCharToUpper()));
        _squiresPassive3 = Game.Current.ObjectManager.RegisterPresumedObject(new CareerChoiceObject(nameof(_squiresPassive3).UnderscoreFirstCharToUpper()));
        _squiresPassive4 = Game.Current.ObjectManager.RegisterPresumedObject(new CareerChoiceObject(nameof(_squiresPassive4).UnderscoreFirstCharToUpper()));

        _templarOrdersKeystone = Game.Current.ObjectManager.RegisterPresumedObject(new CareerChoiceObject(nameof(_templarOrdersKeystone).UnderscoreFirstCharToUpper()));
        _templarOrdersPassive1 = Game.Current.ObjectManager.RegisterPresumedObject(new CareerChoiceObject(nameof(_templarOrdersPassive1).UnderscoreFirstCharToUpper()));
        _templarOrdersPassive2 = Game.Current.ObjectManager.RegisterPresumedObject(new CareerChoiceObject(nameof(_templarOrdersPassive2).UnderscoreFirstCharToUpper()));
        _templarOrdersPassive3 = Game.Current.ObjectManager.RegisterPresumedObject(new CareerChoiceObject(nameof(_templarOrdersPassive3).UnderscoreFirstCharToUpper()));
        _templarOrdersPassive4 = Game.Current.ObjectManager.RegisterPresumedObject(new CareerChoiceObject(nameof(_templarOrdersPassive4).UnderscoreFirstCharToUpper()));
        
        _pathOfViliganceKeystone = Game.Current.ObjectManager.RegisterPresumedObject(new CareerChoiceObject(nameof(_pathOfViliganceKeystone).UnderscoreFirstCharToUpper()));
        _pathOfViligancePassive1 = Game.Current.ObjectManager.RegisterPresumedObject(new CareerChoiceObject(nameof(_pathOfViligancePassive1).UnderscoreFirstCharToUpper()));
        _pathOfViligancePassive2 = Game.Current.ObjectManager.RegisterPresumedObject(new CareerChoiceObject(nameof(_pathOfViligancePassive2).UnderscoreFirstCharToUpper()));
        _pathOfViligancePassive3 = Game.Current.ObjectManager.RegisterPresumedObject(new CareerChoiceObject(nameof(_pathOfViligancePassive3).UnderscoreFirstCharToUpper()));
        _pathOfViligancePassive4 = Game.Current.ObjectManager.RegisterPresumedObject(new CareerChoiceObject(nameof(_pathOfViligancePassive4).UnderscoreFirstCharToUpper()));
        
        _wrathAgainstChaosKeystone = Game.Current.ObjectManager.RegisterPresumedObject(new CareerChoiceObject(nameof(_wrathAgainstChaosKeystone).UnderscoreFirstCharToUpper()));
        _wrathAgainstChaosPassive1 = Game.Current.ObjectManager.RegisterPresumedObject(new CareerChoiceObject(nameof(_wrathAgainstChaosPassive1).UnderscoreFirstCharToUpper()));
        _wrathAgainstChaosPassive2 = Game.Current.ObjectManager.RegisterPresumedObject(new CareerChoiceObject(nameof(_wrathAgainstChaosPassive2).UnderscoreFirstCharToUpper()));
        _wrathAgainstChaosPassive3 = Game.Current.ObjectManager.RegisterPresumedObject(new CareerChoiceObject(nameof(_wrathAgainstChaosPassive3).UnderscoreFirstCharToUpper()));
        _wrathAgainstChaosPassive4 = Game.Current.ObjectManager.RegisterPresumedObject(new CareerChoiceObject(nameof(_wrathAgainstChaosPassive4).UnderscoreFirstCharToUpper()));
        
        _pathOfGloryKeystone = Game.Current.ObjectManager.RegisterPresumedObject(new CareerChoiceObject(nameof(_pathOfGloryKeystone).UnderscoreFirstCharToUpper()));
        _pathOfGloryPassive1 = Game.Current.ObjectManager.RegisterPresumedObject(new CareerChoiceObject(nameof(_pathOfGloryPassive1).UnderscoreFirstCharToUpper()));
        _pathOfGloryPassive2 = Game.Current.ObjectManager.RegisterPresumedObject(new CareerChoiceObject(nameof(_pathOfGloryPassive2).UnderscoreFirstCharToUpper()));
        _pathOfGloryPassive3 = Game.Current.ObjectManager.RegisterPresumedObject(new CareerChoiceObject(nameof(_pathOfGloryPassive3).UnderscoreFirstCharToUpper()));
        _pathOfGloryPassive4 = Game.Current.ObjectManager.RegisterPresumedObject(new CareerChoiceObject(nameof(_pathOfGloryPassive4).UnderscoreFirstCharToUpper()));
    }

    protected override void InitializeKeyStones()
    {
        _knightOldWorldRoot.Initialize(CareerID, "Summon a champion that the necromancer take control of. The Champion loses every 2 seconds 5 health points. For every 3 points in spell casting skill the champion gains 1 health point. Charging: applying spell- damage or healing. Alternatively, Let undead units inflict damage.", null, true,
            ChoiceType.Keystone, new List<CareerChoiceObject.MutationObject>()
            {
                new CareerChoiceObject.MutationObject()
                {
                    MutationTargetType = typeof(AbilityTemplate),
                    MutationTargetOriginalId = "KnightlyStrike",
                    PropertyName = "ScaleVariable1",
                    PropertyValue = (choice, originalValue, agent) => 0.2f+ CareerHelper.AddSkillEffectToValue(choice, agent, new List<SkillObject>(){ DefaultSkills.OneHanded }, 0.004f),
                    MutationType = OperationType.Add
                }
            });
        
        _secularOrdersKeystone.Initialize(CareerID, "Knightly strike has 2 additional loads. starts charged", "SecularOrders", false,
            ChoiceType.Keystone, new List<CareerChoiceObject.MutationObject>()
            {
              
            },new CareerChoiceObject.PassiveEffect()); 
        
        _pathOfConquestKeystone.Initialize(CareerID, "Adds cleaving attacks for ability. Ability scales with Two handed weapon skill", "PathOfConquest", false,
            ChoiceType.Keystone, new List<CareerChoiceObject.MutationObject>()
            {
                new CareerChoiceObject.MutationObject()
                {
                    MutationTargetType = typeof(AbilityTemplate),
                    MutationTargetOriginalId = "KnightlyStrike",
                    PropertyName = "ScaleVariable1",
                    PropertyValue = (choice, originalValue, agent) => 0.2f+ CareerHelper.AddSkillEffectToValue(choice, agent, new List<SkillObject>(){ DefaultSkills.TwoHanded }, 0.004f),
                    MutationType = OperationType.Add
                },
                new CareerChoiceObject.MutationObject()
                {
                    MutationTargetType = typeof(StatusEffectTemplate),
                    MutationTargetOriginalId = "knightly_strike",
                    PropertyName = "TemporaryAttributes",
                    PropertyValue = (choice, originalValue, agent) =>
                    {
                        
                        var list = (List<string>)originalValue;
                        if (list.Contains("Slice"))
                        {
                            return list;
                        }
                        return list.Concat(new[] { "Slice" }).ToList();
                    },
                    MutationType = OperationType.Replace
                }
            },new CareerChoiceObject.PassiveEffect()); 
        
        _squiresKeystone.Initialize(CareerID, "Your Harbinger gains a two handed weapon. Ability scales with Roguery", "Squires", false,
            ChoiceType.Keystone, new List<CareerChoiceObject.MutationObject>()
            {
                new CareerChoiceObject.MutationObject()
                {
                    MutationTargetType = typeof(AbilityTemplate),
                    MutationTargetOriginalId = "KnightlyStrike",
                    PropertyName = "ScaleVariable1",
                    PropertyValue = (choice, originalValue, agent) => 0.2f+ CareerHelper.AddSkillEffectToValue(choice, agent, new List<SkillObject>(){ DefaultSkills.Riding }, 0.004f),
                    MutationType = OperationType.Add
                }
            },new CareerChoiceObject.PassiveEffect()); 
        
        _templarOrdersKeystone.Initialize(CareerID, "2 additional charges. Ability scales with Faith", "TemplarOrders", false,
            ChoiceType.Keystone, new List<CareerChoiceObject.MutationObject>()
            {
                new CareerChoiceObject.MutationObject()
                {
                    MutationTargetType = typeof(AbilityTemplate),
                    MutationTargetOriginalId = "KnightlyStrike",
                    PropertyName = "ScaleVariable1",
                    PropertyValue = (choice, originalValue, agent) => 0.2f+ CareerHelper.AddSkillEffectToValue(choice, agent, new List<SkillObject>(){ TORSkills.Faith }, 0.004f),
                    MutationType = OperationType.Add
                }
            },new CareerChoiceObject.PassiveEffect()); 
        
        _pathOfViliganceKeystone.Initialize(CareerID, "Couched Lance attacks are not removing loads. Ability scales with polearm", "PathOfViligance", false,
            ChoiceType.Keystone, new List<CareerChoiceObject.MutationObject>()
            {
                new CareerChoiceObject.MutationObject()
                {
                    MutationTargetType = typeof(AbilityTemplate),
                    MutationTargetOriginalId = "KnightlyStrike",
                    PropertyName = "ScaleVariable1",
                    PropertyValue = (choice, originalValue, agent) => (float)originalValue+ CareerHelper.AddSkillEffectToValue(choice, agent, new List<SkillObject>(){ DefaultSkills.Polearm }, 0.004f),
                    MutationType = OperationType.Add
                }
            },new CareerChoiceObject.PassiveEffect()); 
        
        _wrathAgainstChaosKeystone.Initialize(CareerID, "Every discharge hit adds a 25% Wardsave for 5 seconds. Ability charging is 20% more efficient", "WrathAgainstChaos", false,
            ChoiceType.Keystone, new List<CareerChoiceObject.MutationObject>()
            {
                new CareerChoiceObject.MutationObject()
                {
                    MutationTargetType = typeof(AbilityTemplate),
                    MutationTargetOriginalId = "KnightlyStrike",
                    PropertyName = "ScaleVariable1",
                    PropertyValue = (choice, originalValue, agent) => (float)originalValue+ CareerHelper.AddSkillEffectToValue(choice, agent, new List<SkillObject>(){ DefaultSkills.Polearm }, 0.004f),
                    MutationType = OperationType.Add
                }
            },new CareerChoiceObject.PassiveEffect()); 
        
        _pathOfGloryKeystone.Initialize(CareerID, "Your Harbinger gains a two handed weapon. Ability scales with Roguery", "PathOfGlory", false,
            ChoiceType.Keystone, new List<CareerChoiceObject.MutationObject>()
            {
              
            },new CareerChoiceObject.PassiveEffect()); 
        
    }

    protected override void InitializePassives()
    {
        _secularOrdersPassive1.Initialize(CareerID, "custom resource upgrade costs for knights are reduced by 25%.", "SecularOrders", false, ChoiceType.Passive, null, new CareerChoiceObject.PassiveEffect(-25, PassiveEffectType.CustomResourceUpgradeCostModifier, true, 
            characterObject => characterObject.IsKnightUnit() || characterObject.HasAttribute("Knight")));
        _secularOrdersPassive2.Initialize(CareerID, "All Knight troops receive 30 bonus points in their Two handed skill.", "SecularOrders", false, ChoiceType.Passive, null, new CareerChoiceObject.PassiveEffect(30, PassiveEffectType.Special)); //
        _secularOrdersPassive3.Initialize(CareerID, "All Knight troops wages are reduced by 25%.", "SecularOrders", false, ChoiceType.Passive, null, new CareerChoiceObject.PassiveEffect(-25, PassiveEffectType.TroopUpgradeCost, true, 
            characterObject => characterObject.IsKnightUnit()|| characterObject.HasAttribute("Knightly")));
        _secularOrdersPassive4.Initialize(CareerID,"Secular Seals can be applied on any Knight unit","SecularOrders",false,ChoiceType.Passive); 

        _pathOfConquestPassive1.Initialize(CareerID, "Extra melee damage (10%).", "PathOfConquest", false, ChoiceType.Passive, null, new CareerChoiceObject.PassiveEffect(PassiveEffectType.Damage, new DamageProportionTuple(DamageType.Physical, 10), AttackTypeMask.Melee));
        _pathOfConquestPassive2.Initialize(CareerID, "Party movement speed is increased by 2.", "PathOfConquest", false, ChoiceType.Passive, null, new CareerChoiceObject.PassiveEffect(2, PassiveEffectType.PartyMovementSpeed));
        _pathOfConquestPassive3.Initialize(CareerID, "Horse charge damage is increased by 40%.", "PathOfConquest", false, ChoiceType.Passive, null, new CareerChoiceObject.PassiveEffect(40, PassiveEffectType.HorseChargeDamage, true));
        _pathOfConquestPassive4.Initialize(CareerID, "All Knight troops receive 30 bonus points in their Polearm-skill.", "PathOfConquest", false, ChoiceType.Passive, null, new CareerChoiceObject.PassiveEffect(30, PassiveEffectType.Special)); //
        
        _squiresPassive1.Initialize(CareerID, "Increases Hitpoints by 50.", "Squires", false, ChoiceType.Passive, null,new CareerChoiceObject.PassiveEffect(30, PassiveEffectType.Health));
        _squiresPassive2.Initialize(CareerID, "All Knight troops wages are reduced by 25%.", "Squires", false, ChoiceType.Passive, null, new CareerChoiceObject.PassiveEffect(-25, PassiveEffectType.TroopWages, true, 
            characterObject => characterObject.IsKnightUnit()));
        _squiresPassive3.Initialize(CareerID, "Wounded troops in your party heal faster.", "Squires", false, ChoiceType.Passive, null, new CareerChoiceObject.PassiveEffect(2, PassiveEffectType.TroopRegeneration));
        _squiresPassive4.Initialize(CareerID, "Battles against non-humans provide 100% prestige gain.", "Squires", false, ChoiceType.Passive, null, new CareerChoiceObject.PassiveEffect(30, PassiveEffectType.Special)); //
        
        _templarOrdersPassive1.Initialize(CareerID, "Increases Hitpoints by 30.", "TemplarOrders", false, ChoiceType.Passive, null,new CareerChoiceObject.PassiveEffect(30, PassiveEffectType.Health));
        _templarOrdersPassive2.Initialize(CareerID,"kills add faith skill.","TemplarOrders",false,ChoiceType.Passive);
        _templarOrdersPassive3.Initialize(CareerID, "Having matching knights of deity increase their damage by 20%", "TemplarOrders", false, ChoiceType.Passive, null,
            new CareerChoiceObject.PassiveEffect(PassiveEffectType.Damage, new DamageProportionTuple(DamageType.Physical, 20), AttackTypeMask.All,
                (attacker, victim, mask) => ( attacker.Character.IsKnightUnit() || attacker.Character.HasAttribute("Knightly") )&& attacker.BelongsToMainParty() && mask == AttackTypeMask.Melee && Hero.MainHero.GetDominantReligion().ReligiousTroops.Contains((CharacterObject)attacker.Character)));
        _templarOrdersPassive4.Initialize(CareerID, "Bonus damage against undead", "TemplarOrders", false, ChoiceType.Passive, null,
            new CareerChoiceObject.PassiveEffect(PassiveEffectType.Damage, new DamageProportionTuple(DamageType.Physical, 20), AttackTypeMask.All,
                (attacker, victim, mask) => victim.IsUndead() && attacker.IsMainAgent  && mask == AttackTypeMask.Melee  ));
        
        _pathOfViligancePassive1.Initialize(CareerID, "50% additional Hitpoints for the player's mount.", "PathOfViligance", false, ChoiceType.Passive, null, new CareerChoiceObject.PassiveEffect(50, PassiveEffectType.HorseHealth, true)); 
        _pathOfViligancePassive2.Initialize(CareerID, "Gain 15% physical resistance to melee and ranged attacks.", "PathOfViligance", false, ChoiceType.Passive, null, new CareerChoiceObject.PassiveEffect(PassiveEffectType.Resistance, new DamageProportionTuple(DamageType.Physical, 15), AttackTypeMask.Ranged | AttackTypeMask.Melee)); 
        _pathOfViligancePassive3.Initialize(CareerID,"Hits below 15 damage do not stagger the player.","PathOfViligance",false,ChoiceType.Passive);
        _pathOfViligancePassive4.Initialize(CareerID, "Wielding a shield increases wardsave.", "PathOfViligance", false, ChoiceType.Passive, null,
            new CareerChoiceObject.PassiveEffect(PassiveEffectType.Resistance, new DamageProportionTuple(DamageType.All, 35), AttackTypeMask.All,
                (attacker, victim, mask) => victim.IsMainAgent && victim.WieldedOffhandWeapon.IsShield() ));
        
        _wrathAgainstChaosPassive1.Initialize(CareerID, "All units deal more damage against chaos.", "WrathAgainstChaos", false, ChoiceType.Passive, null, new CareerChoiceObject.PassiveEffect(PassiveEffectType.TroopDamage, new DamageProportionTuple(DamageType.Holy, 15), AttackTypeMask.All,
            (attacker, victim, mask) => victim.Character.Race != 0));
        _wrathAgainstChaosPassive2.Initialize(CareerID, "Gain 35% Magic Resistance.", "WrathAgainstChaos", false, ChoiceType.Passive, null, new CareerChoiceObject.PassiveEffect(PassiveEffectType.Resistance, new DamageProportionTuple(DamageType.Magical, 35), AttackTypeMask.Spell)); 
        _wrathAgainstChaosPassive3.Initialize(CareerID, "Weapon swing speed increased by 15%.", "SwiftProcedure", false, ChoiceType.Passive, null, new CareerChoiceObject.PassiveEffect(15f, PassiveEffectType.SwingSpeed,true)); 
        _wrathAgainstChaosPassive4.Initialize(CareerID, "Extra 25% armor penetration of melee attacks.", "WrathAgainstChaos", false, ChoiceType.Passive, null, new CareerChoiceObject.PassiveEffect(-25, PassiveEffectType.ArmorPenetration, AttackTypeMask.Melee));
        
        _pathOfGloryPassive1.Initialize(CareerID, "Increases Hitpoints by 30.", "PathOfGlory", false, ChoiceType.Passive, null,new CareerChoiceObject.PassiveEffect(30, PassiveEffectType.Health)); 
        _pathOfGloryPassive2.Initialize(CareerID, "10% Ward save for all knight units.", "PathOfGlory", false, ChoiceType.Passive, null, new CareerChoiceObject.PassiveEffect(PassiveEffectType.TroopResistance, new DamageProportionTuple(DamageType.All, 10), AttackTypeMask.Spell, 
            (attacker, victim, mask) =>  !victim.BelongsToMainParty()&& !victim.IsHero && victim.Character.IsKnightUnit()));
        _pathOfGloryPassive3.Initialize(CareerID, "Gain 15% Ward save.", "PathOfGlory", false, ChoiceType.Passive, null, new CareerChoiceObject.PassiveEffect(PassiveEffectType.Resistance, new DamageProportionTuple(DamageType.All,15),AttackTypeMask.All));
        _pathOfGloryPassive4.Initialize(CareerID, "Gain the option to add an additional seal on a troop.", "PathOfGlory", false, ChoiceType.Passive, null, new CareerChoiceObject.PassiveEffect(30, PassiveEffectType.Special)); //


    }


}