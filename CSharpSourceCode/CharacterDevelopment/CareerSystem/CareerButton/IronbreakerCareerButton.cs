using HarmonyLib;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Party;
using TaleWorlds.CampaignSystem.Roster;
using TaleWorlds.Library;
using TaleWorlds.Localization;
using TaleWorlds.ObjectSystem;
using TaleWorlds.TwoDimension;
using TOR_Core.CampaignMechanics.CustomResources;
using TOR_Core.Extensions;
using TOR_Core.Extensions.UI;
using TOR_Core.Utilities;

namespace TOR_Core.CharacterDevelopment.CareerSystem.CareerButton;

public class IronbreakerCareerButtonBehavior(CareerObject careerObject) : CareerButtonBehaviorBase(careerObject)
{
    private TroopRoster _copiedTroopRoster;
    private const string _ironbreakerId = "tor_m_knight_of_misfortune";
    private CharacterObject _originalTroop;
    private CharacterObject _ironbreakerUnit;
    private int _exchangeCost = 15;
    private bool _isPrisoner;

    public override void ButtonClickedEvent(CharacterObject characterObject, bool isPrisoner = false)
    {
        var index = Hero.MainHero.PartyBelongedTo.MemberRoster.FindIndexOfTroop(characterObject);

        _ironbreakerUnit = MBObjectManager.Instance.GetObject<CharacterObject>(_ironbreakerId);
        
        TroopRosterElement unit = new TroopRosterElement(characterObject);
        
        int indexToInsertTroop = PartyVMExtension.ViewModelInstance.PartyScreenLogic.GetIndexToInsertTroop(PartyScreenLogic.PartyRosterSide.Right, PartyScreenLogic.TroopType.Member,unit);
        PartyScreenLogic.PresentationUpdate update = PartyVMExtension.ViewModelInstance.PartyScreenLogic.UpdateDelegate;
        PartyScreenLogic.PartyCommand command = new PartyScreenLogic.PartyCommand();
        
        
         Hero.MainHero.PartyBelongedTo.MemberRoster.AddToCountsAtIndex(index, -1,0,0,false);
         command.FillForRecruitTroop(PartyScreenLogic.PartyRosterSide.Right,PartyScreenLogic.TroopType.Member,characterObject,-1,indexToInsertTroop);
         update(command); 
         
         
         Hero.MainHero.PartyBelongedTo.MemberRoster.AddToCounts(_ironbreakerUnit, 1);
         command.FillForRecruitTroop(PartyScreenLogic.PartyRosterSide.Right,PartyScreenLogic.TroopType.Member,_ironbreakerUnit,1,indexToInsertTroop);
         update(command);
        
        
        
        

      //  PartyVMExtension.ViewModelInstance.PartyScreenLogic.DoneLogic(false);
        
         
        // command.FillForSortTroops(PartyScreenLogic.PartyRosterSide.Right,PartyScreenLogic.TroopSortType.Name,true);

         

         if (PartyVMExtension.ViewModelInstance != null)
         {
             PartyVMExtension.ViewModelInstance.RefreshValues();
         }
         
         
    }

    public override bool ShouldButtonBeVisible(CharacterObject characterObject, bool isPrisoner = false)
    {
        if (isPrisoner)
            return false;

        if (characterObject.IsHero)
            return false;
        
        if (characterObject.IsIronbreakerUnit())
        {
            return false;
        }
        return true;
        if (characterObject.Culture.StringId != TORConstants.Cultures.DAWI)
            return false;
        
        return true;
    }

    public override bool ShouldButtonBeActive(CharacterObject characterObject, out TextObject displayText, bool isPrisoner = false)
    {
        displayText = new TextObject();
        var index = Hero.MainHero.PartyBelongedTo.MemberRoster.FindIndexOfTroop(characterObject);

        var number = Hero.MainHero.PartyBelongedTo.MemberRoster.GetElementNumber(characterObject);

        if (number <= 0)
        {
            return false;
        }

        if (characterObject.Level < 16)
        {
            displayText = new TextObject(" not high enough level");
            return false;
        }

        return true;
        


        return false;

    }
}