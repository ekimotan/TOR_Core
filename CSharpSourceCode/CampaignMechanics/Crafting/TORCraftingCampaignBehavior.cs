using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.CampaignBehaviors;
using TaleWorlds.Core;
using TaleWorlds.Localization;
using TaleWorlds.ObjectSystem;

namespace TOR_Core.CampaignMechanics.Crafting
{
    public class TORCraftingCampaignBehavior : CraftingCampaignBehavior
    {
        public override void RegisterEvents()
        {
            base.RegisterEvents();
            CampaignEvents.OnSessionLaunchedEvent.AddNonSerializedListener(this, OnSessionStart);
        }

        private void OnSessionStart(CampaignGameStarter starter)
        {
            AccessTools.Property(typeof(ItemObject), "Name").SetValue(DefaultItems.IronIngot6, new TextObject("{=ironingot6_name}Star Metal{@Plural}loads of star metal{\\@}"));
        }
    }
}
