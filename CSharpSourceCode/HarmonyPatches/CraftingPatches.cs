using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TaleWorlds.CampaignSystem.CampaignBehaviors;
using TaleWorlds.CampaignSystem.ViewModelCollection.WeaponCrafting.WeaponDesign;
using TaleWorlds.Core;

namespace TOR_Core.HarmonyPatches
{
    [HarmonyPatch]
    public static class CraftingPatches
    {
        [HarmonyPrefix]
        [HarmonyPatch(typeof(WeaponClassSelectionPopupVM), MethodType.Constructor, typeof(ICraftingCampaignBehavior), typeof(List<CraftingTemplate>), typeof(Action<int>), typeof(Func<CraftingTemplate, int>))]
        public static void FilterCategories(ICraftingCampaignBehavior craftingBehavior, List<CraftingTemplate> templatesList, Action<int> onSelect, Func<CraftingTemplate, int> getUnlockedPiecesCount)
        {
            var backup = templatesList.ToList();
            templatesList.Clear();
            templatesList.AddRange(backup.Where(x => !HiddenCraftingTemplateIds.Contains(x.StringId)));
        }

        public static List<string> HiddenCraftingTemplateIds => ["tor_large_monster_weapon_template", "tor_dual_wield_mainhand"];
    }
}
