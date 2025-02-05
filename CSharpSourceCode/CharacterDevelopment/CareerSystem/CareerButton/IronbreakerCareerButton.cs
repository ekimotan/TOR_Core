using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using SandBox.View.Map;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.GameMenus;
using TaleWorlds.CampaignSystem.Party;
using TaleWorlds.CampaignSystem.Roster;
using TaleWorlds.Core;
using TaleWorlds.Library;
using TaleWorlds.LinQuick;
using TaleWorlds.Localization;
using TaleWorlds.ObjectSystem;
using TaleWorlds.ScreenSystem;
using TaleWorlds.TwoDimension;
using TOR_Core.CampaignMechanics.CustomResources;
using TOR_Core.Extensions;
using TOR_Core.Extensions.UI;
using TOR_Core.Utilities;

namespace TOR_Core.CharacterDevelopment.CareerSystem.CareerButton;

public class IronbreakerCareerButtonBehavior(CareerObject careerObject) : CareerButtonBehaviorBase(careerObject)
{
    private const string IronbreakerId = "tor_m_knight_of_misfortune";
    private const int ExchangeCost = 15;

    private static readonly List<CharacterObject> ResetCharacterObjects = [];

    public override void ButtonClickedEvent(CharacterObject characterObject, bool isPrisoner = false)
    {
        CustomResourceManager.AddResourceChanges(Hero.MainHero.GetCultureSpecificCustomResource(),ExchangeCost);
        PartyVMExtension.ViewModelInstance.RefreshValues();
        
        ResetCharacterObjects.Add(characterObject);
        
        var ironbreakerUnit = MBObjectManager.Instance.GetObject<CharacterObject>(IronbreakerId);
        CareerButtonHelper.ExchangeUnitForNewUnit(characterObject, ironbreakerUnit, true);
        
        PartyScreenManager.PartyScreenLogic.PartyScreenClosedEvent += OnClose;
        PartyScreenManager.PartyScreenLogic.AfterReset += AfterReset;
    }

    private void AfterReset(PartyScreenLogic partyscreenlogic, bool fromcancel)
    {
        ResetTroops();
    }

    private void OnClose(PartyBase leftownerparty, TroopRoster leftmemberroster, TroopRoster leftprisonroster, PartyBase rightownerparty, TroopRoster rightmemberroster, TroopRoster rightprisonroster, bool fromcancel)
    {
        if (fromcancel)
        {
            ResetTroops();
        }
        ResetCharacterObjects.Clear();      //just to be sure
        
        PartyScreenManager.PartyScreenLogic.PartyScreenClosedEvent-=OnClose;
        PartyScreenManager.PartyScreenLogic.AfterReset -= AfterReset;
    }

    private void ResetTroops()
    {
        if (ResetCharacterObjects.IsEmpty())
        {
            return;
        }
        
        var ironbreakerUnit = MBObjectManager.Instance.GetObject<CharacterObject>(IronbreakerId);
        foreach (var character in ResetCharacterObjects)
        {
            CareerButtonHelper.ExchangeUnitForNewUnit(ironbreakerUnit, character, false);
        }
        ResetCharacterObjects.Clear();
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

        var t = CustomResourceManager.GetPendingResources().Values.ToList().Sum();

        if (Hero.MainHero.GetCultureSpecificCustomResourceValue() < t+ ExchangeCost)
        {
            displayText = new TextObject("Not enough resources");
            return false;
        }
        
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