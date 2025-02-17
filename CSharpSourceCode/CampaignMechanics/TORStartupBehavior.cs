using TaleWorlds.CampaignSystem;
using TOR_Core.CampaignMechanics.Diplomacy;
using TOR_Core.Extensions;

namespace TOR_Core.CampaignMechanics
{
    public class TORStartupBehavior : CampaignBehaviorBase
    {
        public override void RegisterEvents()
        {
            CampaignEvents.OnNewGameCreatedEvent.AddNonSerializedListener(this, OnNewGameCreated);
        }

        public override void SyncData(IDataStore dataStore)
        {
        }

        private void OnNewGameCreated(CampaignGameStarter starter)
        {
            var moot = Kingdom.All.Find(m => m.StringId == "moot");
            var stirland = Kingdom.All.Find(s => s.StringId == "stirland");
            moot.SetAlliance(stirland);

            TORKingdomDecisionsCampaignBehavior.UpdateWarPeaceForAlliance(stirland);
            stirland.SetAllyTriggered(false);
        }
    }
}
