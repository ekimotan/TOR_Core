using System.Collections.Generic;
using System.Linq;
using TaleWorlds.CampaignSystem;
using TaleWorlds.Core;
using TaleWorlds.Engine;
using TaleWorlds.Library;
using TaleWorlds.LinQuick;
using TaleWorlds.MountAndBlade;
using TOR_Core.AbilitySystem;
using TOR_Core.BattleMechanics.DamageSystem;
using TOR_Core.BattleMechanics.StatusEffect;
using TOR_Core.CharacterDevelopment.CareerSystem;
using TOR_Core.CharacterDevelopment.CareerSystem.Choices;
using TOR_Core.Extensions;
using TOR_Core.Extensions.ExtendedInfoSystem;
using TOR_Core.Items;
using TOR_Core.Utilities;

namespace TOR_Core.BattleMechanics.TriggeredEffect.Scripts
{
    
    public class ApplySwiftShiverTrait : ITriggeredScript
    {
        public void OnTrigger(Vec3 position, Agent triggeredByAgent, IEnumerable<Agent> triggeredAgents, float duration)
        {
            if(triggeredAgents.Count() > 0)
            {
  
                var additionalDamage = new DamageProportionTuple();

                additionalDamage.DamageType = DamageType.Magical;
                additionalDamage.Percent = 0.15f;
                var trait = new ItemTrait
                {
                    ItemTraitName = "Swiftshiver shards Trait",
                    ItemTraitDescription = "The damage is increased by 20% extra magical damage",
                    WeaponParticlePreset = new WeaponParticlePreset { ParticlePrefab = "magic_trait" },
                    AdditionalDamageTuple = additionalDamage,
                    OnHitScriptName = "none"
                };

                foreach (Agent agent in triggeredAgents)
                {
                    var comp = agent.GetComponent<ItemTraitAgentComponent>();
                    if(comp != null)
                    {
                        TraitHelper.ApplyEffectToRangedWeapons(comp, trait, agent, duration);
                    }
                }
            }
        }
    }
    
    public class ApplyHagbaneTrait : ITriggeredScript
    {
        public void OnTrigger(Vec3 position, Agent triggeredByAgent, IEnumerable<Agent> triggeredAgents, float duration)
        {
            if(triggeredAgents.Count() > 0)
            {
                var trait = new ItemTrait
                {
                    ItemTraitName = "Hagbane Trait",
                    ItemTraitDescription = "The weapon has been poisoned. Slows down enemies",
                    ImbuedStatusEffectId = "hagbane_debuff",
                    WeaponParticlePreset = new WeaponParticlePreset { ParticlePrefab = "hagbane_trait" },
                    OnHitScriptName = "none"
                };

                foreach (Agent agent in triggeredAgents)
                {
                    var comp = agent.GetComponent<ItemTraitAgentComponent>();
                    if(comp != null)
                    {
                        TraitHelper.ApplyEffectToRangedWeapons(comp, trait, agent, duration);
                    }
                }
            }
        }
    }
    
    public class ApplyStarFireTrait : ITriggeredScript
    {
        public void OnTrigger(Vec3 position, Agent triggeredByAgent, IEnumerable<Agent> triggeredAgents, float duration)
        {
            if(triggeredAgents.Count() > 0)
            {
                var trait = new ItemTrait
                {
                    ItemTraitName = "Starfire shards Trait",
                    ItemTraitDescription = "Adds Armor penetration effect, fire damage and dot",
                    ImbuedStatusEffectId = "starfire_debuff",
                    WeaponParticlePreset = new WeaponParticlePreset { ParticlePrefab = "psys_flaming_weapon" },
                    AdditionalDamageTuple = new DamageProportionTuple
                    {
                        DamageType = DamageType.Fire, Percent = 0.20f
                    },
                    OnHitScriptName = "none"
                };
                
                foreach (Agent agent in triggeredAgents)
                {
                    var comp = agent.GetComponent<ItemTraitAgentComponent>();
                    if(comp != null)
                    {
                        TraitHelper.ApplyEffectToRangedWeapons(comp, trait, agent, duration);
                    }
                }
            }
        }
    }
    
    public class ApplyFlamingItemTraitScript : ITriggeredScript
    {
        public void OnTrigger(Vec3 position, Agent triggeredByAgent, IEnumerable<Agent> triggeredAgents, float duration)
        {
            if(triggeredAgents.Count() > 0)
            {
                var trait = new ItemTrait();
                var additionalDamage = new DamageProportionTuple();

                additionalDamage.DamageType = DamageType.Fire;
                additionalDamage.Percent = 0.25f;
                
                trait.ItemTraitName = "Flaming Sword";
                trait.ItemTraitDescription = "This sword is on fire. It deals fire damage and applies the burning damage over time effect.";
                trait.ImbuedStatusEffectId = "fireball_dot";
                trait.WeaponParticlePreset = new WeaponParticlePreset { ParticlePrefab = "psys_flaming_weapon" };
                trait.AdditionalDamageTuple = additionalDamage;
                trait.OnHitScriptName = "none";

                foreach (Agent agent in triggeredAgents)
                {
                    var comp = agent.GetComponent<ItemTraitAgentComponent>();
                    if(comp != null)
                    {
                        comp.AddTraitToWieldedWeapon(trait, duration);
                    }
                }
            }
        }
    }

    public class ApplyKnightlyStrikeTraitScript : ITriggeredScript
    {
        public void OnTrigger(Vec3 position, Agent triggeredByAgent, IEnumerable<Agent> triggeredAgents, float duration)
        {
            var additionalDamage = new DamageProportionTuple();
            additionalDamage.DamageType = DamageType.Physical;
            additionalDamage.Percent = 0.2f;
            
            var ca = triggeredByAgent.GetComponent<AbilityComponent>().CareerAbility;

            var bonusdamage = 0f;
            if (ca != null)
            { 
                bonusdamage = ca.Template.ScaleVariable1;
            }
            additionalDamage.Percent += bonusdamage;

            var additionalLoads = Hero.MainHero.GetAllCareerChoices().WhereQ(x=> x.Contains("Keystone")).Count();

            if (Hero.MainHero.HasCareerChoice("SecularOrdersKeystone"))
            {
                additionalLoads += 2;
            }
            
            if (Hero.MainHero.HasCareerChoice("TemplarOrdersKeystone"))
            {
                additionalLoads += 2;
            }
            
            var traitList = new List<ItemTrait>();
            
            if (Hero.MainHero.HasCareerChoice("PathOfGloryKeystone"))
            {
                var holyTrait = CareerHelper.GetTraitForReligion(Hero.MainHero, Hero.MainHero.GetDominantReligion());
                traitList.Add(holyTrait);
            }
            
            var defaultTrait = new ItemTrait();
            defaultTrait.ItemTraitName = "KnightlyStrike";
            defaultTrait.ItemTraitDescription = " Charge your weapon with knightly power";
            defaultTrait.WeaponParticlePreset = new WeaponParticlePreset { ParticlePrefab = "psys_flaming_weapon" };
            defaultTrait.OnHitScriptName = "TOR_Core.BattleMechanics.TriggeredEffect.Scripts.KnightlyStrikeOnHitScript";
            defaultTrait.AdditionalDamageTuple = additionalDamage;
            traitList.Add(defaultTrait);

            triggeredByAgent.ApplyStatusEffect("knightly_strike",triggeredByAgent,30,false,false,true);
            for (int i = 0; i < additionalLoads; i++)
            {
                triggeredByAgent.ApplyStatusEffect("knightly_strike",triggeredByAgent,30,false,false,true);
            }
            
            foreach (Agent agent in triggeredAgents)
            {
                var comp = agent.GetComponent<ItemTraitAgentComponent>();
                if(comp != null)
                {
                    foreach (var trait in traitList)
                    {
                        comp.AddTraitToWieldedWeapon(trait, duration);
                    }
                }
            }
        }
    }
    
    public class ApplyLesserFlamingItemTraitScript : ITriggeredScript
    {
        public void OnTrigger(Vec3 position, Agent triggeredByAgent, IEnumerable<Agent> triggeredAgents, float duration)
        {
            if(triggeredAgents.Count() > 0)
            {
                var trait = new ItemTrait();
                var additionalDamage = new DamageProportionTuple();

                additionalDamage.DamageType = DamageType.Fire;
                additionalDamage.Percent = 0.15f;
                
                trait.ItemTraitName = "Lesser Flaming Sword";
                trait.ItemTraitDescription = "This sword is on fire. It deals additional fire damage.";
                trait.WeaponParticlePreset = new WeaponParticlePreset { ParticlePrefab = "psys_flaming_weapon" };
                trait.AdditionalDamageTuple = additionalDamage;
                trait.OnHitScriptName = "none";

                foreach (Agent agent in triggeredAgents)
                {
                    var comp = agent.GetComponent<ItemTraitAgentComponent>();
                    if(comp != null)
                    {
                        comp.AddTraitToWieldedWeapon(trait, duration);
                    }
                }
            }
        }
    }
    
    public class ApplyLesserLightItemTraitScript : ITriggeredScript
    {
        public void OnTrigger(Vec3 position, Agent triggeredByAgent, IEnumerable<Agent> triggeredAgents, float duration)
        {
            if(triggeredAgents.Count() > 0)
            {
                var trait = new ItemTrait();
                var additionalDamage = new DamageProportionTuple();

                additionalDamage.DamageType = DamageType.Magical;
                additionalDamage.Percent = 0.25f;
                
                trait.ItemTraitName = "Hysh infused Sword";
                trait.ItemTraitDescription = "This sword is guided by Hysh. It deals magical damage.";
                trait.ImbuedStatusEffectId = "powerstone_light_mov_debuff";
                trait.WeaponParticlePreset = new WeaponParticlePreset { ParticlePrefab = "psys_light_weapon" };
                trait.AdditionalDamageTuple = additionalDamage;
                trait.OnHitScriptName = "none";

                foreach (Agent agent in triggeredAgents)
                {
                    var comp = agent.GetComponent<ItemTraitAgentComponent>();
                    if(comp != null)
                    {
                        comp.AddTraitToWieldedWeapon(trait, duration);
                    }
                }
            }
        }
    }
    
    public class ApplyLightItemTraitScript : ITriggeredScript
    {
        public void OnTrigger(Vec3 position, Agent triggeredByAgent, IEnumerable<Agent> triggeredAgents, float duration)
        {
            if(triggeredAgents.Count() > 0)
            {
                var trait = new ItemTrait();
                var additionalDamage = new DamageProportionTuple();

                additionalDamage.DamageType = DamageType.Magical;
                additionalDamage.Percent = 0.4f;
                
                trait.ItemTraitName = "Hysh infused Sword";
                trait.ItemTraitDescription = "This sword is guided by Hysh. It deals magical damage.";
                trait.ImbuedStatusEffectId = "powerstone_light_mov_debuff";
                trait.WeaponParticlePreset = new WeaponParticlePreset { ParticlePrefab = "psys_light_weapon" };
                trait.AdditionalDamageTuple = additionalDamage;
                trait.OnHitScriptName = "none";

                foreach (Agent agent in triggeredAgents)
                {
                    var comp = agent.GetComponent<ItemTraitAgentComponent>();
                    if(comp != null)
                    {
                        comp.AddTraitToWieldedWeapon(trait, duration);
                    }
                }
            }
        }
    }
    
    public class ApplyLesserHeavensItemTraitScript : ITriggeredScript
    {
        public void OnTrigger(Vec3 position, Agent triggeredByAgent, IEnumerable<Agent> triggeredAgents, float duration)
        {
            if(triggeredAgents.Count() > 0)
            {
                var trait = new ItemTrait();
                var additionalDamage = new DamageProportionTuple();

                additionalDamage.DamageType = DamageType.Lightning;
                additionalDamage.Percent = 0.2f;
                
                trait.ItemTraitName = "Azyr infused weapon";
                trait.ItemTraitDescription = "This weapon is guided by Azyr. It deals lightning damage.";
                trait.ImbuedStatusEffectId = "none";
                trait.WeaponParticlePreset = new WeaponParticlePreset { ParticlePrefab = "electric_weapon" };
                
                trait.AdditionalDamageTuple = additionalDamage;
                trait.OnHitScriptName = "none";

                foreach (Agent agent in triggeredAgents)
                {
                    var comp = agent.GetComponent<ItemTraitAgentComponent>();
                    if(comp != null)
                    {
                        comp.AddTraitToWieldedWeapon(trait, duration);
                    }
                }
            }
        }
    }
    
    public class ApplyHeavensItemTraitScript : ITriggeredScript
    {
        public void OnTrigger(Vec3 position, Agent triggeredByAgent, IEnumerable<Agent> triggeredAgents, float duration)
        {
            if(triggeredAgents.Count() > 0)
            {
                var trait = new ItemTrait();
                var additionalDamage = new DamageProportionTuple();

                additionalDamage.DamageType = DamageType.Lightning;
                additionalDamage.Percent = 0.4f;
                
                trait.ItemTraitName = "Azyr infused weapon";
                trait.ItemTraitDescription = "This sword is guided by Azyr. It deals electrical damage.";
                trait.ImbuedStatusEffectId = "none";
                trait.WeaponParticlePreset = new WeaponParticlePreset { ParticlePrefab = "electric_weapon" };
                trait.AdditionalDamageTuple = additionalDamage;
                trait.OnHitScriptName = "none";

                foreach (Agent agent in triggeredAgents)
                {
                    var comp = agent.GetComponent<ItemTraitAgentComponent>();
                    if(comp != null)
                    {
                        comp.AddTraitToWieldedWeapon(trait, duration);
                    }
                }
            }
        }
    }
    
    public class ApplyDeathDamageItemTraitScript : ITriggeredScript
    {
        public void OnTrigger(Vec3 position, Agent triggeredByAgent, IEnumerable<Agent> triggeredAgents, float duration)
        {
            if(triggeredAgents.Count() > 0)
            {
                var trait = new ItemTrait();
                var additionalDamage = new DamageProportionTuple();

                additionalDamage.DamageType = DamageType.Magical;
                additionalDamage.Percent = 0.4f;
                
                trait.ItemTraitName = "Shyish infused weapon";
                trait.ItemTraitDescription = "This sword is guided by shyish. It deals electrical damage.";
                trait.ImbuedStatusEffectId = "none";
                trait.WeaponParticlePreset = new WeaponParticlePreset { ParticlePrefab = "psys_death_weapon_effect" };
                trait.AdditionalDamageTuple = additionalDamage;
                trait.OnHitScriptName = "none";

                foreach (Agent agent in triggeredAgents)
                {
                    var comp = agent.GetComponent<ItemTraitAgentComponent>();
                    if(comp != null)
                    {
                        comp.AddTraitToWieldedWeapon(trait, duration);
                    }
                }
            }
        }
    }
    
    public class ApplyGreaterHeavensItemTraitScript : ITriggeredScript
    {
        public void OnTrigger(Vec3 position, Agent triggeredByAgent, IEnumerable<Agent> triggeredAgents, float duration)
        {
            if(triggeredAgents.Count() > 0)
            {
                var trait = new ItemTrait();
                var additionalDamage = new DamageProportionTuple();

                additionalDamage.DamageType = DamageType.Lightning;
                additionalDamage.Percent = 0.3f;
                
                additionalDamage.DamageType = DamageType.Frost;
                additionalDamage.Percent = 0.3f;
                
                trait.ItemTraitName = "Azyr infused weapon";
                trait.ItemTraitDescription = "This sword is guided by Azyr. It deals electrical damage.";
                trait.ImbuedStatusEffectId = "powerstone_heavens_debuff";
                trait.WeaponParticlePreset = new WeaponParticlePreset { ParticlePrefab = "psys_heavens_weapon" };
                trait.AdditionalDamageTuple = additionalDamage;
                trait.OnHitScriptName = "none";

                foreach (Agent agent in triggeredAgents)
                {
                    var comp = agent.GetComponent<ItemTraitAgentComponent>();
                    if(comp != null)
                    {
                        comp.AddTraitToWieldedWeapon(trait, duration);
                    }
                }
            }
        }
    }
    
    public class ApplyMetalItemTraitScript : ITriggeredScript
    {
        public void OnTrigger(Vec3 position, Agent triggeredByAgent, IEnumerable<Agent> triggeredAgents, float duration)
        {
            if(triggeredAgents.Count() > 0)
            {
                var trait = new ItemTrait();
                var trait2 = new ItemTrait();
                var additionalDamage = new DamageProportionTuple();
                var additionalDamage2 = new DamageProportionTuple();

                additionalDamage.DamageType = DamageType.Fire;
                additionalDamage.Percent = 0.2f;
                
                additionalDamage2.DamageType = DamageType.Magical;
                additionalDamage2.Percent = 0.2f;
                
                trait.ItemTraitName = "Chamon infused weapon";
                trait.ItemTraitDescription = "This weapon is guided by chamon. It deals lightning damage.";
                trait.ImbuedStatusEffectId = "none";
                trait.WeaponParticlePreset = new WeaponParticlePreset { ParticlePrefab = "psys_flaming_weapon" };
                trait.AdditionalDamageTuple = additionalDamage;
                trait.OnHitScriptName = "none";
                
                trait2.AdditionalDamageTuple  = additionalDamage2;

                foreach (Agent agent in triggeredAgents)
                {
                    var comp = agent.GetComponent<ItemTraitAgentComponent>();
                    if(comp != null)
                    {
                        comp.AddTraitToWieldedWeapon(trait, duration);
                        comp.AddTraitToWieldedWeapon(trait2,duration);
                    }
                }
            }
        }
    }
    
    public class ApplyQuickSilverWeaponItemTraitScript : ITriggeredScript
    {
        public void OnTrigger(Vec3 position, Agent triggeredByAgent, IEnumerable<Agent> triggeredAgents, float duration)
        {
            if(triggeredAgents.Count() > 0)
            {
                var trait = new ItemTrait();
                var additionalDamage = new DamageProportionTuple();
                additionalDamage.DamageType = DamageType.Physical;
                additionalDamage.Percent = 0.25f;
                trait.ItemTraitName = "Quick silver Weapon Enchantment";
                trait.ItemTraitDescription = "Quicksilver surrounds your weapons.";
                trait.WeaponParticlePreset = new WeaponParticlePreset { ParticlePrefab = "psys_quicksilver_swords" };
                trait.AdditionalDamageTuple = additionalDamage;
                trait.OnHitScriptName = "none";
                var trait2 = new ItemTrait();
                
                var additionalDamage2 = new DamageProportionTuple();
                additionalDamage2.DamageType = DamageType.Magical;
                additionalDamage2.Percent = 0.25f;
                
                trait.ItemTraitName = "Quick silver Weapon Enchantment";
                trait.ItemTraitDescription = "Quicksilver surrounds your weapons.";
                trait.WeaponParticlePreset = new WeaponParticlePreset { ParticlePrefab = "psys_quicksilver_swords" };
                trait.AdditionalDamageTuple = additionalDamage;
                trait.OnHitScriptName = "none";

                foreach (Agent agent in triggeredAgents)
                {
                    var comp = agent.GetComponent<ItemTraitAgentComponent>();
                    if(comp != null)
                    {
                        comp.AddTraitToWieldedWeapon(trait, duration);
                        comp.AddTraitToWieldedWeapon(trait2, duration);
                    }
                }
            }
        }
    }
    
    public class ApplyHolyItemTraitScript : ITriggeredScript
    {
        public void OnTrigger(Vec3 position, Agent triggeredByAgent, IEnumerable<Agent> triggeredAgents, float duration)
        {
            if(triggeredAgents.Count() > 0)
            {
                var trait = new ItemTrait();
                var additionalDamage = new DamageProportionTuple();

                additionalDamage.DamageType = DamageType.Holy;
                additionalDamage.Percent = 0.30f;
                
                trait.ItemTraitName = "Holy Weapon Enchantment";
                trait.ItemTraitDescription = "This sword is on fire. It deals fire damage and applies the burning damage over time effect.";
                trait.WeaponParticlePreset = new WeaponParticlePreset { ParticlePrefab = "psys_holy_weapon" };
                trait.AdditionalDamageTuple = additionalDamage;
                trait.OnHitScriptName = "none";

                foreach (Agent agent in triggeredAgents)
                {
                    var comp = agent.GetComponent<ItemTraitAgentComponent>();
                    if(comp != null)
                    {
                        comp.AddTraitToWieldedWeapon(trait, duration);
                    }
                }
            }
        }
    }

    public class EnchantWeaponScript : ITriggeredScript
    {
        public void OnTrigger(Vec3 position, Agent triggeredByAgent, IEnumerable<Agent> triggeredAgents, float duration)
        {
            if (triggeredAgents.Count() > 0)
            {
                var trait = new ItemTrait();
                var additionalDamage = new DamageProportionTuple();

                additionalDamage.DamageType = DamageType.Magical;
                additionalDamage.Percent = 0.10f;

                trait.ItemTraitName = "Enchanted Weapon";
                trait.ItemTraitDescription = "This weapon deals additional magic damage.";
                trait.ImbuedStatusEffectId = "none";
                trait.WeaponParticlePreset = new WeaponParticlePreset { ParticlePrefab = "psys_magic_weapon" };
                trait.AdditionalDamageTuple = additionalDamage;
                trait.OnHitScriptName = "none";

                foreach (Agent agent in triggeredAgents)
                {
                    var comp = agent.GetComponent<ItemTraitAgentComponent>();
                    if (comp != null)
                    {
                        comp.AddTraitToWieldedWeapon(trait, duration);
                    }
                }
            }
        }
    }
    
    public class SpiritLeech: ITriggeredScript
    {
        /*
         * Leech consists of a damage over time (dot) and heal over time (hot) effect. the dot is handled via the XMLs while the hot is applied here.
         * The ability takes only the strongest unit (or hero) into account, since it otherwise would stack and scale too much.
         * the duration of the dot is affecting the duration of the hot.a
         * P and Z
         */
        public void OnTrigger(Vec3 position, Agent triggeredByAgent, IEnumerable<Agent> triggeredAgents, float duration)
        {
            
            var targets = triggeredAgents.ToList();


            if (targets.Count <= 0) return;
            
            var target = targets[0];

            target = targets.FirstOrDefaultQ(x => x.IsHero) ?? targets.MaxBy(x => x.Character.Level);

            var tier = target.Character.GetBattleTier();
            
            triggeredByAgent.ApplyStatusEffect("spirit_leech_heal",triggeredByAgent,tier * duration);

        }
    }
}
