using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using TaleWorlds.CampaignSystem;

namespace TOR_Core.Extensions
{
    public static class FactionExtensions
    {
        public static int GetNumActiveKingdomWars(this IFaction faction)
        {
            var count = 0;
            if (faction == null) return count;
            if(faction?.Stances is IEnumerable<StanceLink> stances)
            {
                foreach (var stance in stances)
                {
                    if (stance.Faction1 is Kingdom && stance.Faction2 is Kingdom)
                    {
                        if (stance.IsAtWar || stance.IsAtConstantWar) count++;
                    }
                }
            }
            return count;
        }

        public static float GetSumEnemyKingdomPower(this IFaction faction)
        {
            float sum = 0;
            if (faction == null) return sum;
            if (faction?.Stances is IEnumerable<StanceLink> stances)
            {
                foreach (var stance in stances)
                {
                    if (stance.Faction1 is Kingdom && stance.Faction2 is Kingdom)
                    {
                        if (stance.IsAtWar || stance.IsAtConstantWar) sum += stance.Faction1 == faction ? stance.Faction2.GetAllianceTotalStrength() : stance.Faction1.GetAllianceTotalStrength();
                    }
                }
            }
            return sum;
        }

        public static bool IsAlliedWith(this IFaction faction1, IFaction faction2)
        {
            if (faction1 == faction2)
            {
                return false;
            }
            var stanceLink = faction1.GetStanceWith(faction2);
            return stanceLink.IsAllied;
        }

        public static IEnumerable<IFaction> GetAlliedFactions(this IFaction faction) =>  faction.Stances.Where(m => m.IsAllied).Select(m => m.Faction1.Id == faction.Id ? m.Faction2 : m.Faction2);

        public static float GetAllianceTotalStrength(this IFaction faction) => faction.GetAlliedFactions().Select(curKingdom => curKingdom.TotalStrength).Sum() + faction.TotalStrength;

        public static void SetAlliance(this IFaction factionA, IFaction factionB)
        {
            // Get the internal type: TaleWorlds.CampaignSystem.StanceType
            var stanceType = AccessTools.TypeByName("TaleWorlds.CampaignSystem.StanceType");
            var stanceTypeValue = Enum.Parse(stanceType, "Alliance"); // Get the "Alliance" enum value

            // Get the internal class: TaleWorlds.CampaignSystem.StanceLink
            var stanceLinkType = AccessTools.TypeByName("TaleWorlds.CampaignSystem.StanceLink");
            // Get the private/internal constructor: StanceLink(StanceType, IFaction, IFaction, bool)
            var stanceLinkConstructor = AccessTools.Constructor(stanceLinkType, [stanceType, typeof(IFaction), typeof(IFaction), typeof(bool)]);
            // Create an instance of StanceLink
            var stanceLink = stanceLinkConstructor.Invoke([stanceTypeValue, factionA, factionB, false]);

            // Get the private method: FactionManager.AddStance(IFaction, IFaction, StanceLink)
            var addStanceMethod = AccessTools.Method(typeof(FactionManager), "AddStance");
            // Call AddStance via Harmony's access tools
            addStanceMethod?.Invoke(FactionManager.Instance, [factionA, factionB, stanceLink]);
        }
    }
}
