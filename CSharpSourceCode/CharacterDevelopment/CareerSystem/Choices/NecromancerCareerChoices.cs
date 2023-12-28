﻿using System.Collections.Generic;
using TaleWorlds.Core;
using TOR_Core.AbilitySystem;
using TOR_Core.BattleMechanics.StatusEffect;
using TOR_Core.BattleMechanics.TriggeredEffect;
using TOR_Core.CampaignMechanics.Choices;

namespace TOR_Core.CharacterDevelopment.CareerSystem.Choices
{
    public class NecromancerCareerChoices : TORCareerChoicesBase
    {
        public NecromancerCareerChoices(CareerObject id) : base(id) {}
        private CareerChoiceObject _necromancerRoot;
        
        private CareerChoiceObject _liberNecrisKeystone; // 1
        private CareerChoiceObject _liberNecrisPassive1;
        private CareerChoiceObject _liberNecrisPassive2;
        private CareerChoiceObject _liberNecrisPassive3;
        private CareerChoiceObject _liberNecrisPassive4;
        
        private CareerChoiceObject _deArcanisKadonKeystone; // 2
        private CareerChoiceObject _codexMortificaKeystone;     //3
        
        private CareerChoiceObject _liberMortisKeystone; //4
        private CareerChoiceObject _bookofWsoranKeystone;        //5
        private CareerChoiceObject _grimoireNecrisKeystone; //6
        
        
        private CareerChoiceObject _booksOfNagashKeystone; // 7



        protected override void RegisterAll()
        {
            _necromancerRoot = Game.Current.ObjectManager.RegisterPresumedObject(new CareerChoiceObject("NecromancerRoot"));
            
            _liberNecrisKeystone = Game.Current.ObjectManager.RegisterPresumedObject(new CareerChoiceObject("LiberNecrisKeystone"));
            _liberNecrisPassive1 = Game.Current.ObjectManager.RegisterPresumedObject(new CareerChoiceObject("LiberNecrisPassive1"));
            _liberNecrisPassive2 = Game.Current.ObjectManager.RegisterPresumedObject(new CareerChoiceObject("LiberNecrisPassive2"));
            _liberNecrisPassive3 = Game.Current.ObjectManager.RegisterPresumedObject(new CareerChoiceObject("LiberNecrisPassive3"));
            _liberNecrisPassive4 = Game.Current.ObjectManager.RegisterPresumedObject(new CareerChoiceObject("LiberNecrisPassive4"));
            
            _deArcanisKadonKeystone = Game.Current.ObjectManager.RegisterPresumedObject(new CareerChoiceObject("DeArcanisKadonKeystone"));
            _codexMortificaKeystone = Game.Current.ObjectManager.RegisterPresumedObject(new CareerChoiceObject("CodexMortificaKeystone"));
            
            _liberMortisKeystone = Game.Current.ObjectManager.RegisterPresumedObject(new CareerChoiceObject("LiberMortisKeystone"));
            _bookofWsoranKeystone = Game.Current.ObjectManager.RegisterPresumedObject(new CareerChoiceObject("BookofWsoranKeystone"));
            _grimoireNecrisKeystone = Game.Current.ObjectManager.RegisterPresumedObject(new CareerChoiceObject("GrimoireNecrisKeystone"));
            
            
            _booksOfNagashKeystone = Game.Current.ObjectManager.RegisterPresumedObject(new CareerChoiceObject("BooksOfNagashKeystone"));
            
            
        }

        protected override void InitializeKeyStones()
        {
            _necromancerRoot.Initialize(CareerID, "Summon a champion that the necromancer take control of. The Champion loses every 2 seconds 5 health points. For every 3 points in spell casting skill the champion gains 1 health point. Charging: applying spell damage or healing.", null, true,
                ChoiceType.Keystone, new List<CareerChoiceObject.MutationObject>()
                {
                    new CareerChoiceObject.MutationObject()
                    {
                        MutationTargetType = typeof(AbilityTemplate),
                        MutationTargetOriginalId = "GreaterHarbinger",
                        PropertyName = "ScaleVariable1",
                        PropertyValue = (choice, originalValue, agent) => 0.1f+ CareerHelper.AddSkillEffectToValue(choice, agent, new List<SkillObject>(){ TORSkills.SpellCraft}, 0.33f),
                        MutationType = OperationType.Add
                    }
                });
            
            _liberNecrisKeystone.Initialize(CareerID, "Your Harbinger gains a two handed weapon. Ability scales with Roguery", "LiberNecris", false,
                ChoiceType.Keystone, new List<CareerChoiceObject.MutationObject>()
                {
                    new CareerChoiceObject.MutationObject()
                    {
                        MutationTargetType = typeof(TriggeredEffectTemplate),
                        MutationTargetOriginalId = "summon_champion",
                        PropertyName = "TroopIdToSummon",
                        PropertyValue = (choice, originalValue, agent) => originalValue+"_two_handed",
                        MutationType = OperationType.Replace
                    },
                    new CareerChoiceObject.MutationObject()
                    {
                        MutationTargetType = typeof(AbilityTemplate),
                        MutationTargetOriginalId = "GreaterHarbinger",
                        PropertyName = "ScaleVariable1",
                        PropertyValue = (choice, originalValue, agent) => 0.1f+ CareerHelper.AddSkillEffectToValue(choice, agent, new List<SkillObject>(){ DefaultSkills.Roguery}, 0.33f),
                        MutationType = OperationType.Add
                    }
                },new CareerChoiceObject.PassiveEffect()); 
            
            _deArcanisKadonKeystone.Initialize(CareerID, "Pressing Ability key allows to switch between characters; Harbinger acts indenpendant.", "DeArcanisKadon", false,
                ChoiceType.Keystone, new List<CareerChoiceObject.MutationObject>()
                {
                },new CareerChoiceObject.PassiveEffect());  // switch controls
                
            _codexMortificaKeystone.Initialize(CareerID, "During Champion control gain 90% damage resistance for caster. Ability scales with Medicine.", "CodexMortifica", false,
                ChoiceType.Keystone, new List<CareerChoiceObject.MutationObject>()
                {
                    new CareerChoiceObject.MutationObject()
                    {
                        MutationTargetType = typeof(AbilityTemplate),
                        MutationTargetOriginalId = "GreaterHarbinger",
                        PropertyName = "ScaleVariable1",
                        PropertyValue = (choice, originalValue, agent) => 0.1f+ CareerHelper.AddSkillEffectToValue(choice, agent, new List<SkillObject>(){ DefaultSkills.Medicine}, 0.33f),
                        MutationType = OperationType.Add
                    }
                },new CareerChoiceObject.PassiveEffect()); // add 90% resistence for necromancer while controlled
                
            
            _liberMortisKeystone.Initialize(CareerID, "Your champion gains 25% extra melee damage. Ability starts charged", "LiberMortis", false,
                ChoiceType.Keystone, new List<CareerChoiceObject.MutationObject>()
                {
                },new CareerChoiceObject.PassiveEffect(25,PassiveEffectType.Special,true)); // add 25% extra damage to champion
            
            _bookofWsoranKeystone.Initialize(CareerID, "Your Champion gains 25% Wardsave and can't be staggered.", "BookOfWsoran", false,
                ChoiceType.Keystone, new List<CareerChoiceObject.MutationObject>()
                {
                },new CareerChoiceObject.PassiveEffect(25,PassiveEffectType.Special,true)); // add 25% Wardsave to champion
            
            _grimoireNecrisKeystone.Initialize(CareerID, "Harbinger kills gain 2HP for caster and the Champion.", "GrimoireNecris", false,
                ChoiceType.Keystone, new List<CareerChoiceObject.MutationObject>()
                {
                    new CareerChoiceObject.MutationObject()
                    {
                        MutationTargetType = typeof(TriggeredEffectTemplate),
                        MutationTargetOriginalId = "summon_champion",
                        PropertyName = "TroopIdToSummon",
                        PropertyValue = (choice, originalValue, agent) => originalValue+"_plate",
                        MutationType = OperationType.Replace
                    },
                },new CareerChoiceObject.PassiveEffect()); // For every kill as Harbinger you and the Harbinger regain 2 HP
            
            _booksOfNagashKeystone.Initialize(CareerID, "Regenerate per kill of your champion 1 winds of magic. Adds 20% magical damage to champion", "BooksOfNagash", false,
                ChoiceType.Keystone, new List<CareerChoiceObject.MutationObject>()
                {
                },new CareerChoiceObject.PassiveEffect(25,PassiveEffectType.Special,true));// For every kill as Harbinger the necromancer gains 1 Winds of Magic
        }

        protected override void InitializePassives()
        {
            _liberNecrisPassive1.Initialize(CareerID, "Increases Party size by 10.", "LiberNecris", false, ChoiceType.Passive, null, new CareerChoiceObject.PassiveEffect(100, PassiveEffectType.PartySize));
            //_liberNecrisPassive.Initialize(CareerID, "Increases Party size by 10.", "LiberNecris", false, ChoiceType.Passive, null, new CareerChoiceObject.PassiveEffect(100, PassiveEffectType.PartySize));

        }
    }
}