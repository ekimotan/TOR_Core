using System.Collections.Generic;
using System.Linq;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Party;
using TaleWorlds.CampaignSystem.Roster;
using TaleWorlds.CampaignSystem.ViewModelCollection.Party;
using TaleWorlds.Core;
using TaleWorlds.Core.ViewModelCollection;
using TaleWorlds.Library;
using TOR_Core.CampaignMechanics.CustomResources;
using TOR_Core.Extensions;

namespace TOR_Core.CharacterDevelopment.CareerSystem.CareerButton;

public static class CareerButtonHelper
{

    public static int GetMaximumExchangeTroops(CharacterObject originalTroop, bool isPrisoner,  int upperBoundNumberTroops, int goldCost, int customResourceCost)
    {
        var buyable = 1;//maximum affordable

        var count = 1;
        
        count = isPrisoner ? Hero.MainHero.PartyBelongedTo.PrisonRoster.GetElementNumber(originalTroop) : Hero.MainHero.PartyBelongedTo.MemberRoster.GetElementNumber(originalTroop);
        
        var goldBuyable = count;
        
        if (customResourceCost > 0)
        {
            var pending = CustomResourceManager.GetPendingResources().Values.ToList().Sum();
            var rest = Hero.MainHero.GetCultureSpecificCustomResourceValue() - pending;
            buyable = (int)(( rest / customResourceCost >= upperBoundNumberTroops) ? upperBoundNumberTroops : rest / customResourceCost);
        }
        //gold
        if (goldCost > 0)
        {
            var goldPending = PartyScreenManager.PartyScreenLogic.CurrentData.PartyGoldChangeAmount;
            var rest = Hero.MainHero.Gold - goldPending; 
            goldBuyable = (int)(( rest / goldCost >= upperBoundNumberTroops) ? upperBoundNumberTroops : rest / goldCost);
            
        }
        buyable = MathF.Min(buyable, goldBuyable);
        
        buyable = MathF.Min(buyable, count);
        
        return buyable;
    }
    
    public static void ExchangeUnitForNewUnit(CharacterObject targetCharacterObject, CharacterObject newUnit, bool updateScreen, bool isPrisoner = false)
    {
        var partyScreenLogic = PartyScreenManager.PartyScreenLogic;
        
        
        PartyScreenLogic.PresentationUpdate update = partyScreenLogic.UpdateDelegate;
        PartyScreenLogic.PartyCommand command = new PartyScreenLogic.PartyCommand();
        
        //Hero.MainHero.PartyBelongedTo.MemberRoster.AddToCountsAtIndex(index, -1,0,0,false);
        Hero.MainHero.PartyBelongedTo.MemberRoster.AddToCounts(targetCharacterObject,-1);

        var rosterType = isPrisoner ? PartyScreenLogic.TroopType.Prisoner : PartyScreenLogic.TroopType.Member;
        
        int indexToInsertTroop = partyScreenLogic.GetIndexToInsertTroop(PartyScreenLogic.PartyRosterSide.Right, rosterType,new TroopRosterElement(targetCharacterObject));
        
        var count = Hero.MainHero.PartyBelongedTo.MemberRoster.GetElementNumber(targetCharacterObject);

        if (updateScreen)
        {
            if (count > 0)
            {
                command.FillForRecruitTroop(PartyScreenLogic.PartyRosterSide.Right,PartyScreenLogic.TroopType.Member,targetCharacterObject,-1,indexToInsertTroop);
                update(command);  
            }
            else
            {
                MBInformationManager.HideInformations();
                var characterViewModel = PartyVMExtension.ViewModelInstance.MainPartyTroops.FirstOrDefault(x => x.Character == targetCharacterObject);
                PartyVMExtension.ViewModelInstance.MainPartyTroops.Remove(characterViewModel);
                PartyVMExtension.ViewModelInstance.ExecuteRemoveZeroCounts();
            }
        }
        
        Hero.MainHero.PartyBelongedTo.MemberRoster.AddToCounts(newUnit, 1);
        indexToInsertTroop = partyScreenLogic.GetIndexToInsertTroop(PartyScreenLogic.PartyRosterSide.Right, PartyScreenLogic.TroopType.Member,new TroopRosterElement(newUnit));
        if (updateScreen)
        {
            command.FillForRecruitTroop(PartyScreenLogic.PartyRosterSide.Right,PartyScreenLogic.TroopType.Member,newUnit,1,indexToInsertTroop);
            update(command);
        }

    }
}