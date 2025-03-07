﻿using System;
using System.Linq;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Party;
using TaleWorlds.Core;
using TaleWorlds.LinQuick;
using TaleWorlds.Localization;
using TOR_Core.CampaignMechanics.CustomResources;
using TOR_Core.CampaignMechanics.Religion;
using TOR_Core.CampaignMechanics.ServeAsAHireling;
using TOR_Core.CampaignMechanics.TORCustomSettlement;
using TOR_Core.CampaignMechanics.TORCustomSettlement.CustomSettlementMenus;
using TOR_Core.CharacterDevelopment;
using TOR_Core.CharacterDevelopment.CareerSystem;
using TOR_Core.Extensions;
using TOR_Core.Utilities;

namespace TOR_Core.Models;

public class TORCustomResourceModel : GameModel
{
            public ExplainedNumber GetCultureSpecificCustomResourceChange(Hero hero)
        {
            if (hero.PartyBelongedTo == null) return new ExplainedNumber();

            var number = new ExplainedNumber(0,true);
            if (hero.GetCultureSpecificCustomResource() != null)
            {
                var upkeep =  GetCalculatedCustomResourceUpkeep(hero, hero.GetCultureSpecificCustomResource().StringId);
                
                if (upkeep.ResultNumber < 0)
                {
                    foreach (var line in upkeep.GetLines())
                    {
                        number.Add((int)line.number, new TextObject(line.name));
                    }
                }

                if (hero == Hero.MainHero)
                {
                    CareerHelper.ApplyBasicCareerPassives(Hero.MainHero, ref number,PassiveEffectType.CustomResourceGain, false);
                    
                    if (hero.IsEnlisted())
                    {
                        ServeAsAHirelingHelpers.AddHirelingCustomResourceBenefits(hero,ref number);
                    }

                    if (!hero.IsEnlisted() && hero.PartyBelongedTo.Army != null && hero.PartyBelongedTo.Army.LeaderParty != hero.PartyBelongedTo)
                    {
                        number.Add(5,new TextObject("Part of Army"));
                    }

                    if (hero.HasCareer(TORCareers.Spellsinger) && hero.PartyBelongedTo != null && hero.PartyBelongedTo.HasBlessing("cult_of_isha"))
                    {
                        var choice = TORCareerChoices.GetChoice("ArielsBlessingPassive3");

                        if (choice != null)
                        {
                            number.Add(choice.GetPassiveValue(),choice.BelongsToGroup.Name);
                        }
                    }
                    if (hero.HasCareer(TORCareers.Necromancer) && hero.PartyBelongedTo != null)
                    {
                        
                        if (hero.HasCareerChoice("BookofWsoranPassive4"))
                        {
                            var choice = TORCareerChoices.GetChoice("BookofWsoranPassive4");
                            var elements = hero.PartyBelongedTo.MemberRoster.GetTroopRoster().WhereQ(x => x.Character.StringId.Contains("grave_guard")).ToList();
                            var bonus = 0f;
                            foreach (var element in elements)
                            {
                                bonus += element.Number * choice.GetPassiveValue();
                            }
                            
                            number.Add((int)bonus, choice.BelongsToGroup.Name);
                        }
                    }

                    if (hero.Clan.Kingdom!=null && (hero.Culture.StringId == TORConstants.Cultures.SYLVANIA || hero.Culture.StringId ==  TORConstants.Cultures.MOUSILLON))
                    {
                        var kingdom = hero.Clan.Kingdom;

                        var kingdomSettlements = kingdom.Settlements;
                        var bonus = 0;
                        foreach (var settlement  in kingdomSettlements)
                        {
                            
                            if (settlement.IsCastle)
                            {
                                bonus += 2;
                            }
                            else if (settlement.IsTown)
                            {
                                bonus += 3;
                            }
                            else
                            {
                                continue;
                            }

                            if (settlement.OwnerClan == Clan.PlayerClan)
                            {
                                bonus *= 2;
                            }
                        }
                        
                        number.Add(bonus, new TextObject("Dark Tribute"));
                    }

                    if (hero.Culture.StringId == TORConstants.Cultures.ASRAI)
                    {
                        var weSettlements = Campaign.Current.Settlements.WhereQ(x => x.StringId.Contains("_AL")).ToList();
                        var text = new TextObject("Athel Loren Harmony");
                        foreach (var settlement in weSettlements)
                        {
                            if (settlement.IsRaided && settlement.Owner.Culture.StringId == TORConstants.Cultures.ASRAI)
                            {
                                
                                number.Add(-15,new TextObject(settlement.Name+" is raided"));
                                continue;
                            }
                            if (settlement.Owner.Culture.StringId != TORConstants.Cultures.ASRAI)
                            {
                                number.Add(-5,new TextObject(settlement.Name+" is captured"));
                                continue;
                            }
                            if (settlement.IsVillage)
                            {
                                number.Add(0.5f,text);
                            }

                            if (settlement.IsCastle || settlement.IsTown)
                            {
                                number.Add(1.5f,text);
                            }
                        }

                        if (number.ResultNumber > 0)
                        {
                            var settlementBehavior = Campaign.Current.GetCampaignBehavior<TORCustomSettlementCampaignBehavior>();
                            var list = settlementBehavior.GetUnlockedOakUpgradeCategory("WEGainUpgrade");
                            var harmonyFactor = 0f; 
                            harmonyFactor += 0.2f * list.Count;

                            harmonyFactor= ForestHarmonyHelper.CalculateForestGain(harmonyFactor);
                            number.AddFactor(harmonyFactor, new TextObject("Thriving Leaves"));
                        }
                    }
                }
                
                if (hero.HasCareer(TORCareers.BlackGrailKnight)&& hero.HasCareerChoice("BlackGrailVowPassive4"))
                {
                    var choice = TORCareerChoices.GetChoice("BlackGrailVowPassive4");
                    if (hero.PartyBelongedTo != null)
                    {
                        var heroes = hero.PartyBelongedTo.GetMemberHeroes();
                        heroes.Remove(Hero.MainHero);

                        foreach (var companion in heroes)
                        {
                            if (companion.IsVampire() || companion.IsNecromancer())
                            {
                                number.Add(choice.GetPassiveValue(),choice.BelongsToGroup.Name);
                            }
                        }
                    }
                }

                if (hero.HasCareer(TORCareers.GrailKnight)&&hero.HasCareerChoice("QuestingVowPassive4"))
                {
                    var choice = TORCareerChoices.GetChoice("QuestingVowPassive4");
                    var heroes = hero.PartyBelongedTo.GetMemberHeroes();
                    heroes.Remove(Hero.MainHero);
                    foreach (var companion in heroes)
                    {
                        if (companion.IsBretonnianKnight())
                        {
                            number.Add(choice.GetPassiveValue(),choice.BelongsToGroup.Name);
                        }
                    }
                }

                if (hero.HasCareer(TORCareers.Necrarch) && hero.HasCareerChoice("EverlingsSecretPassive3"))
                {
                    var choice = TORCareerChoices.GetChoice("EverlingsSecretPassive3");
                    if (choice!=null)
                    {
                        if (hero.GetExtendedInfo().MaxWindsOfMagic <= hero.GetCustomResourceValue("WindsOfMagic"))
                        {
                            number.Add(hero.GetExtendedInfo().WindsOfMagicRechargeRate * CampaignTime.HoursInDay, choice.BelongsToGroup.Name);
                        }
                    }
                }

                if (hero.Culture.StringId == TORConstants.Cultures.BRETONNIA)
                {
                    if (hero.PartyBelongedTo != null)
                    {
                        if (hero.PartyBelongedTo.HasBlessing("cult_of_lady"))
                        {
                            var obj = ReligionObject.All.FirstOrDefault( x=>x.StringId=="cult_of_lady");
                            if (obj != null)
                            {
                                
                                number.Add(15,new TextObject("Blessing of the Lady"));
                            }
                        }
                    }

                    if (hero.IsClanLeader)
                    {
                        foreach (var clanmember in hero.Clan.Heroes)
                        {
                            if(clanmember==hero) continue;

                            if (clanmember.IsAlive&&clanmember.IsPartyLeader)
                            {
                                number.Add(2,new TextObject("Clan members with Party"));
                            }
                        }
                    }

                    if (hero.GetChivalryLevel() == ChivalryLevel.Honourable)
                    {
                        number.Add(5,new TextObject(ChivalryLevel.Honourable.ToString()));
                    }
                    
                    if (hero.GetChivalryLevel() == ChivalryLevel.Chivalrous)
                    {
                        number.Add(15,new TextObject(ChivalryLevel.Honourable.ToString()));
                    }
                }
            } 
            return number;
        }

        public ExplainedNumber GetCalculatedCustomResourceUpkeep(Hero hero, string resourceID="")
        {
            if (resourceID == "")
            {
                resourceID = hero.GetCultureSpecificCustomResource().StringId;
            }
            var upkeep = new ExplainedNumber(0,true,new TextObject("Upkeep"));
            foreach (var element in hero.PartyBelongedTo.MemberRoster.GetTroopRoster())
            {
                if (element.Character.HasCustomResourceUpkeepRequirement())
                {
                    var resource = element.Character.GetCustomResourceRequiredForUpkeep();
                
                    if(resource.Item1.StringId !=resourceID) continue;
                    var unitUpkeet = new ExplainedNumber(resource.Item2*element.Number);
                    if (hero == Hero.MainHero)
                    {
                        CareerHelper.ApplyBasicCareerPassives(Hero.MainHero, ref unitUpkeet,PassiveEffectType.CustomResourceUpkeepModifier, true, element.Character);

                        if (hero.Culture.StringId == TORConstants.Cultures.ASRAI)
                        {
                            
                            if (Hero.MainHero.HasAttribute("WETreekinSymbol") && !element.Character.IsElf() && element.Character.Culture.StringId== TORConstants.Cultures.ASRAI)
                            {
                                unitUpkeet.AddFactor(-0.5f, ForestHarmonyHelper.TreeSymbolText("WETreekinSymbol"));
                            }
                            
                            if (Hero.MainHero.HasAttribute("WEOrionSymbol") && !element.Character.IsElf() && element.Character.Culture.StringId== TORConstants.Cultures.ASRAI)
                            {
                                unitUpkeet.AddFactor(1f, ForestHarmonyHelper.TreeSymbolText("WEOrionSymbol"));
                            }
                        }

                        if (hero.Culture.StringId ==  TORConstants.Cultures.SYLVANIA || hero.Culture.StringId ==  TORConstants.Cultures.MOUSILLON)
                        {
                            if (hero.PartyBelongedTo != null && hero.PartyBelongedTo.Army != null && hero.PartyBelongedTo.Army.LeaderParty != MobileParty.MainParty)
                            {
                                unitUpkeet.AddFactor(-0.5f, new TextObject("Part of Army"));
                            }else if (hero.PartyBelongedTo != null && hero.PartyBelongedTo.BesiegedSettlement != null)
                            {
                                unitUpkeet.AddFactor(-0.5f, new TextObject("Siege Camp Bonus"));
                            }
                        }
                    }
                    
                    upkeep.Add(-unitUpkeet.ResultNumber,new TextObject("Upkeep"));
                    
                }
            }

            foreach (var settlement in hero.Clan.Settlements)
            {
                if (!settlement.IsCastle && !settlement.IsTown)
                {
                    continue;
                }
                if(settlement.Town.GarrisonParty==null) continue;
                var garrison = settlement.Town.GarrisonParty.MemberRoster.GetTroopRoster();
                foreach (var elem in garrison)
                {
                    if (elem.Character.HasCustomResourceUpgradeRequirement())
                    {
                        var resource = elem.Character.GetCustomResourceRequiredForUpkeep();
                        
                        if(resource==null) continue;
                
                        if(resource.Item1.StringId !=resourceID) continue;
                        var garrisonFactor = 0.25f; //base reduction bonus 
                        var garrisonUnitUpkeep = new ExplainedNumber(resource.Item2*elem.Number*garrisonFactor);
                        if (hero == Hero.MainHero)
                        {
                            CareerHelper.ApplyBasicCareerPassives(Hero.MainHero, ref garrisonUnitUpkeep,PassiveEffectType.CustomResourceUpkeepModifier, true, elem.Character); 
                        }
                    
                        upkeep.Add(-garrisonUnitUpkeep.ResultNumber,new TextObject("Garrison Upkeep"));
                    }
                }
            }
            
            return upkeep;
        }
        
        
}