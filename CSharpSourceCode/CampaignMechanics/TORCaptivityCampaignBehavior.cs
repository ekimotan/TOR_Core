using Helpers;
using System;
using System.Linq;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Actions;
using TaleWorlds.CampaignSystem.Party;
using TaleWorlds.CampaignSystem.Party.PartyComponents;

namespace TOR_Core.CampaignMechanics
{
    public class TORCaptivityCampaignBehavior : CampaignBehaviorBase
    {
        private readonly float _maximumDaysInCaptivity = 10;

        public override void RegisterEvents()
        {
            CampaignEvents.DailyTickHeroEvent.AddNonSerializedListener(this, OnDailyTick);
        }

        private void OnDailyTick(Hero hero)
        {
            if (hero.IsPrisoner && hero.PartyBelongedToAsPrisoner != null &&
                hero != Hero.MainHero &&
                hero.PartyBelongedToAsPrisoner.Owner != null &&
                hero.PartyBelongedToAsPrisoner.Owner.Clan != Clan.PlayerClan &&
                hero.CaptivityStartTime != null &&
                hero.IsLord)
            {
                var time = CampaignTime.Now;
                var duration = time - hero.CaptivityStartTime;
                if (duration.ToDays > _maximumDaysInCaptivity) EndCaptivityAction.ApplyByEscape(hero, null);
            }

            if(!hero.IsPrisoner && 
                hero.IsLord &&
                hero.GovernorOf == null && 
                hero.PartyBelongedTo == null && 
                hero.PartyBelongedToAsPrisoner == null && 
                hero.CanLeadParty() && 
                hero.IsAlive &&
                hero.LastKnownClosestSettlement != null)
            {
                //bugged? wtf, how can this even happen?
                MobilePartyHelper.SpawnLordParty(hero, hero.LastKnownClosestSettlement);
            }
        }

        public override void SyncData(IDataStore dataStore) { }
    }
}
