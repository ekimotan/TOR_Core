using System.Collections.Generic;
using TaleWorlds.Core;
using TOR_Core.AbilitySystem;
using TOR_Core.CampaignMechanics.Choices;
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
        
        _pathOfViliganceKeystone = Game.Current.ObjectManager.RegisterPresumedObject(new CareerChoiceObject(nameof(_pathOfConquestKeystone).UnderscoreFirstCharToUpper()));
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
            });
        
        _secularOrdersKeystone.Initialize(CareerID, "Your Harbinger gains a two handed weapon. Ability scales with Roguery", "SecularOrders", false,
            ChoiceType.Keystone, new List<CareerChoiceObject.MutationObject>()
            {
              
            },new CareerChoiceObject.PassiveEffect()); 
        
        _pathOfConquestKeystone.Initialize(CareerID, "Your Harbinger gains a two handed weapon. Ability scales with Roguery", "PathOfConquest", false,
            ChoiceType.Keystone, new List<CareerChoiceObject.MutationObject>()
            {
              
            },new CareerChoiceObject.PassiveEffect()); 
        
        _squiresKeystone.Initialize(CareerID, "Your Harbinger gains a two handed weapon. Ability scales with Roguery", "Squires", false,
            ChoiceType.Keystone, new List<CareerChoiceObject.MutationObject>()
            {
              
            },new CareerChoiceObject.PassiveEffect()); 
        
        _templarOrdersKeystone.Initialize(CareerID, "Your Harbinger gains a two handed weapon. Ability scales with Roguery", "TemplarOrders", false,
            ChoiceType.Keystone, new List<CareerChoiceObject.MutationObject>()
            {
              
            },new CareerChoiceObject.PassiveEffect()); 
        
        _pathOfViliganceKeystone.Initialize(CareerID, "Your Harbinger gains a two handed weapon. Ability scales with Roguery", "PathOfViligance", false,
            ChoiceType.Keystone, new List<CareerChoiceObject.MutationObject>()
            {
              
            },new CareerChoiceObject.PassiveEffect()); 
        
        _wrathAgainstChaosKeystone.Initialize(CareerID, "Your Harbinger gains a two handed weapon. Ability scales with Roguery", "WrathAgainstChaos", false,
            ChoiceType.Keystone, new List<CareerChoiceObject.MutationObject>()
            {
              
            },new CareerChoiceObject.PassiveEffect()); 
        
        _pathOfGloryKeystone.Initialize(CareerID, "Your Harbinger gains a two handed weapon. Ability scales with Roguery", "PathOfGlory", false,
            ChoiceType.Keystone, new List<CareerChoiceObject.MutationObject>()
            {
              
            },new CareerChoiceObject.PassiveEffect()); 
        
    }

    protected override void InitializePassives()
    {
        _secularOrdersPassive1.Initialize(CareerID, "Increases Party size by 10.", "SecularOrders", false, ChoiceType.Passive, null, new CareerChoiceObject.PassiveEffect(10, PassiveEffectType.PartySize));
        _secularOrdersPassive2.Initialize(CareerID, "Increases Party size by 10.", "SecularOrders", false, ChoiceType.Passive, null, new CareerChoiceObject.PassiveEffect(10, PassiveEffectType.PartySize));
        _secularOrdersPassive3.Initialize(CareerID, "Increases Party size by 10.", "SecularOrders", false, ChoiceType.Passive, null, new CareerChoiceObject.PassiveEffect(10, PassiveEffectType.PartySize));
        _secularOrdersPassive4.Initialize(CareerID, "Increases Party size by 10.", "SecularOrders", false, ChoiceType.Passive, null, new CareerChoiceObject.PassiveEffect(10, PassiveEffectType.PartySize));
        
        _pathOfConquestPassive1.Initialize(CareerID, "Increases Party size by 10.", "PathOfConquest", false, ChoiceType.Passive, null, new CareerChoiceObject.PassiveEffect(10, PassiveEffectType.PartySize));
        _pathOfConquestPassive2.Initialize(CareerID, "Increases Party size by 10.", "PathOfConquest", false, ChoiceType.Passive, null, new CareerChoiceObject.PassiveEffect(10, PassiveEffectType.PartySize));
        _pathOfConquestPassive3.Initialize(CareerID, "Increases Party size by 10.", "PathOfConquest", false, ChoiceType.Passive, null, new CareerChoiceObject.PassiveEffect(10, PassiveEffectType.PartySize));
        _pathOfConquestPassive4.Initialize(CareerID, "Increases Party size by 10.", "PathOfConquest", false, ChoiceType.Passive, null, new CareerChoiceObject.PassiveEffect(10, PassiveEffectType.PartySize));
        
        _squiresPassive1.Initialize(CareerID, "Increases Party size by 10.", "Squires", false, ChoiceType.Passive, null, new CareerChoiceObject.PassiveEffect(10, PassiveEffectType.PartySize));
        _squiresPassive2.Initialize(CareerID, "Increases Party size by 10.", "Squires", false, ChoiceType.Passive, null, new CareerChoiceObject.PassiveEffect(10, PassiveEffectType.PartySize));
        _squiresPassive3.Initialize(CareerID, "Increases Party size by 10.", "Squires", false, ChoiceType.Passive, null, new CareerChoiceObject.PassiveEffect(10, PassiveEffectType.PartySize));
        _squiresPassive4.Initialize(CareerID, "Increases Party size by 10.", "Squires", false, ChoiceType.Passive, null, new CareerChoiceObject.PassiveEffect(10, PassiveEffectType.PartySize));
        
        _templarOrdersPassive1.Initialize(CareerID, "Increases Party size by 10.", "TemplarOrders", false, ChoiceType.Passive, null, new CareerChoiceObject.PassiveEffect(10, PassiveEffectType.PartySize)); 
        _templarOrdersPassive2.Initialize(CareerID, "Increases Party size by 10.", "TemplarOrders", false, ChoiceType.Passive, null, new CareerChoiceObject.PassiveEffect(10, PassiveEffectType.PartySize)); 
        _templarOrdersPassive3.Initialize(CareerID, "Increases Party size by 10.", "TemplarOrders", false, ChoiceType.Passive, null, new CareerChoiceObject.PassiveEffect(10, PassiveEffectType.PartySize)); 
        _templarOrdersPassive4.Initialize(CareerID, "Increases Party size by 10.", "TemplarOrders", false, ChoiceType.Passive, null, new CareerChoiceObject.PassiveEffect(10, PassiveEffectType.PartySize)); 
        
        _pathOfViligancePassive1.Initialize(CareerID, "Increases Party size by 10.", "PathOfViligance", false, ChoiceType.Passive, null, new CareerChoiceObject.PassiveEffect(10, PassiveEffectType.PartySize)); 
        _pathOfViligancePassive2.Initialize(CareerID, "Increases Party size by 10.", "PathOfViligance", false, ChoiceType.Passive, null, new CareerChoiceObject.PassiveEffect(10, PassiveEffectType.PartySize)); 
        _pathOfViligancePassive3.Initialize(CareerID, "Increases Party size by 10.", "PathOfViligance", false, ChoiceType.Passive, null, new CareerChoiceObject.PassiveEffect(10, PassiveEffectType.PartySize)); 
        _pathOfViligancePassive4.Initialize(CareerID, "Increases Party size by 10.", "PathOfViligance", false, ChoiceType.Passive, null, new CareerChoiceObject.PassiveEffect(10, PassiveEffectType.PartySize)); 
        
        _wrathAgainstChaosPassive1.Initialize(CareerID, "Increases Party size by 10.", "WrathAgainstChaos", false, ChoiceType.Passive, null, new CareerChoiceObject.PassiveEffect(10, PassiveEffectType.PartySize)); 
        _wrathAgainstChaosPassive2.Initialize(CareerID, "Increases Party size by 10.", "WrathAgainstChaos", false, ChoiceType.Passive, null, new CareerChoiceObject.PassiveEffect(10, PassiveEffectType.PartySize)); 
        _wrathAgainstChaosPassive3.Initialize(CareerID, "Increases Party size by 10.", "WrathAgainstChaos", false, ChoiceType.Passive, null, new CareerChoiceObject.PassiveEffect(10, PassiveEffectType.PartySize)); 
        _wrathAgainstChaosPassive4.Initialize(CareerID, "Increases Party size by 10.", "WrathAgainstChaos", false, ChoiceType.Passive, null, new CareerChoiceObject.PassiveEffect(10, PassiveEffectType.PartySize)); 
        
        _pathOfGloryPassive1.Initialize(CareerID, "Increases Party size by 10.", "PathOfGlory", false, ChoiceType.Passive, null, new CareerChoiceObject.PassiveEffect(10, PassiveEffectType.PartySize)); 
        _pathOfGloryPassive2.Initialize(CareerID, "Increases Party size by 10.", "PathOfGlory", false, ChoiceType.Passive, null, new CareerChoiceObject.PassiveEffect(10, PassiveEffectType.PartySize)); 
        _pathOfGloryPassive3.Initialize(CareerID, "Increases Party size by 10.", "PathOfGlory", false, ChoiceType.Passive, null, new CareerChoiceObject.PassiveEffect(10, PassiveEffectType.PartySize)); 
        _pathOfGloryPassive4.Initialize(CareerID, "Increases Party size by 10.", "PathOfGlory", false, ChoiceType.Passive, null, new CareerChoiceObject.PassiveEffect(10, PassiveEffectType.PartySize)); 

    }
}