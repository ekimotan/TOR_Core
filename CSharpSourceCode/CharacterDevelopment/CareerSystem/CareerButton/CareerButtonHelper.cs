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

namespace TOR_Core.CharacterDevelopment.CareerSystem.CareerButton;

public static class CareerButtonHelper
{
    public static void ExchangeUnitForNewUnit(CharacterObject targetCharacterObject, CharacterObject newUnit, bool updateScreen)
    {
        var partyScreenLogic = PartyScreenManager.PartyScreenLogic;
        
        
        PartyScreenLogic.PresentationUpdate update = partyScreenLogic.UpdateDelegate;
        PartyScreenLogic.PartyCommand command = new PartyScreenLogic.PartyCommand();
        
        //Hero.MainHero.PartyBelongedTo.MemberRoster.AddToCountsAtIndex(index, -1,0,0,false);
        Hero.MainHero.PartyBelongedTo.MemberRoster.AddToCounts(targetCharacterObject,-1);
        int indexToInsertTroop = partyScreenLogic.GetIndexToInsertTroop(PartyScreenLogic.PartyRosterSide.Right, PartyScreenLogic.TroopType.Member,new TroopRosterElement(targetCharacterObject));
        
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