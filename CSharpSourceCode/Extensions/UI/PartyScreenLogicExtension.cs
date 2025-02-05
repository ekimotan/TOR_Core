using System;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Party;

namespace TOR_Core.Extensions.UI;

public static class PartyScreenLogicExtension
{
    public static void AddPseudoUpgrade(this PartyScreenLogic partyScreenLogic, CharacterObject fromTroop, CharacterObject toTroop, int number)
    {
        
        Tuple<CharacterObject, CharacterObject, int> tuple = partyScreenLogic.CurrentData.UpgradedTroopsHistory.Find((Predicate<Tuple<CharacterObject, CharacterObject, int>>) (t => t.Item1 == fromTroop && t.Item2 == toTroop));
        if (tuple != null)
        {
            int num1 = tuple.Item3;
            partyScreenLogic.CurrentData.UpgradedTroopsHistory.Remove(tuple);
            partyScreenLogic.CurrentData.UpgradedTroopsHistory.Add(new Tuple<CharacterObject, CharacterObject, int>(fromTroop, toTroop, number + num1));
        }
        else
            partyScreenLogic.CurrentData.UpgradedTroopsHistory.Add(new Tuple<CharacterObject, CharacterObject, int>(fromTroop, toTroop, number));
    }
}