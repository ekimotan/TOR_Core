using System.Collections.Generic;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Party;
using TaleWorlds.CampaignSystem.Roster;
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
        if (updateScreen)
        {
            command.FillForRecruitTroop(PartyScreenLogic.PartyRosterSide.Right,PartyScreenLogic.TroopType.Member,targetCharacterObject,-1,indexToInsertTroop);
            update(command);  
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