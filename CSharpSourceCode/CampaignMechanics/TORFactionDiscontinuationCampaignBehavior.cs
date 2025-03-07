﻿using System.Collections.Generic;
using System.Linq;
using TaleWorlds.CampaignSystem.Actions;
using TaleWorlds.CampaignSystem.Party.PartyComponents;
using TaleWorlds.CampaignSystem.Settlements;
using TaleWorlds.CampaignSystem;
using TaleWorlds.Core;
using TaleWorlds.LinQuick;

namespace TOR_Core.CampaignMechanics
{
    public class TORFactionDiscontinuationCampaignBehavior : CampaignBehaviorBase
    {
        private const float SurvivalDurationForIndependentClanInWeeks = 4f;
        private Dictionary<string, double> _independentClans = [];

        public override void RegisterEvents()
        {
            CampaignEvents.OnSettlementOwnerChangedEvent.AddNonSerializedListener(this, OnSettlementOwnerChanged);
            CampaignEvents.OnClanChangedKingdomEvent.AddNonSerializedListener(this, OnClanChangedKingdom);
            CampaignEvents.DailyTickClanEvent.AddNonSerializedListener(this, DailyTickClan);
        }

        public void OnSettlementOwnerChanged(Settlement settlement, bool openToClaim, Hero newOwner, Hero oldOwner, Hero capturerHero, ChangeOwnerOfSettlementAction.ChangeOwnerOfSettlementDetail detail)
        {
            if (_independentClans.ContainsKey(newOwner.Clan.StringId))
            {
                _independentClans.Remove(newOwner.Clan.StringId);
            }
            if (CanClanBeDiscontinued(oldOwner.Clan))
            {
                AddIndependentClan(oldOwner.Clan);
            }
            Kingdom kingdom = oldOwner.Clan.Kingdom;
            if (kingdom != null && CanKingdomBeDiscontinued(kingdom))
            {
                DiscontinueKingdom(kingdom);
            }
        }

        public void OnClanChangedKingdom(Clan clan, Kingdom oldKingdom, Kingdom newKingdom, ChangeKingdomAction.ChangeKingdomActionDetail detail, bool showNotification = true)
        {
            if (newKingdom == null)
            {
                if (CanClanBeDiscontinued(clan))
                {
                    AddIndependentClan(clan);
                }
            }
            else if (_independentClans.ContainsKey(clan.StringId))
            {
                _independentClans.Remove(clan.StringId);
            }
            if (clan == Clan.PlayerClan && oldKingdom != null && CanKingdomBeDiscontinued(oldKingdom))
            {
                DiscontinueKingdom(oldKingdom);
            }
        }

        private void DailyTickClan(Clan clan)
        {
            if (_independentClans.ContainsKey(clan.StringId))
            {
                if (MBRandom.RandomFloat > 0.7f)
                {
                    var candidateKingdoms = Kingdom.All.WhereQ(x => !x.IsEliminated && x.Culture == clan.Culture);
                    if (candidateKingdoms != null && candidateKingdoms.Count() > 0)
                    {
                        var targetKingdom = candidateKingdoms.MinBy(x => x.TotalStrength);
                        if (targetKingdom != null)
                        {
                            ChangeKingdomAction.ApplyByJoinToKingdom(clan, targetKingdom);
                            return;
                        }
                    }
                }
                if (_independentClans[clan.StringId] < CampaignTime.Now.ToWeeks) DiscontinueClan(clan);
            }
        }

        private bool CanKingdomBeDiscontinued(Kingdom kingdom)
        {
            bool flag = !kingdom.IsEliminated && kingdom != Clan.PlayerClan.Kingdom && kingdom.Settlements.IsEmpty();
            if (flag)
            {
                CampaignEventDispatcher.Instance.CanKingdomBeDiscontinued(kingdom, ref flag);
            }
            return flag;
        }

        private void DiscontinueKingdom(Kingdom kingdom)
        {
            foreach (Clan clan in new List<Clan>(kingdom.Clans))
            {
                FinalizeMapEvents(clan);
                ChangeKingdomAction.ApplyByLeaveByKingdomDestruction(clan, true);
            }
            kingdom.RulingClan = null;
            DestroyKingdomAction.Apply(kingdom);
        }

        private void FinalizeMapEvents(Clan clan)
        {
            foreach (WarPartyComponent warPartyComponent in clan.WarPartyComponents.ToList())
            {
                if (warPartyComponent?.MobileParty?.MapEvent != null)
                {
                    warPartyComponent.MobileParty.MapEvent.FinalizeEvent();
                }
                if (warPartyComponent?.MobileParty?.SiegeEvent != null)
                {
                    warPartyComponent.MobileParty.SiegeEvent.FinalizeSiegeEvent();
                }
            }
            foreach (Settlement settlement in clan.Settlements.ToList())
            {
                if (settlement?.Party?.MapEvent != null)
                {
                    settlement.Party.MapEvent.FinalizeEvent();
                }
                if (settlement?.Party?.SiegeEvent != null)
                {
                    settlement.Party.SiegeEvent.FinalizeSiegeEvent();
                }
            }
        }

        private bool CanClanBeDiscontinued(Clan clan)
        {
            return clan.Kingdom == null && !clan.IsRebelClan && !clan.IsBanditFaction && !clan.IsMinorFaction && clan != Clan.PlayerClan && clan.Settlements.IsEmpty();
        }

        private void DiscontinueClan(Clan clan)
        {
            DestroyClanAction.Apply(clan);
            _independentClans.Remove(clan.StringId);
        }

        private void AddIndependentClan(Clan clan)
        {
            if (!_independentClans.ContainsKey(clan.StringId))
            {
                _independentClans.Add(clan.StringId, CampaignTime.WeeksFromNow(SurvivalDurationForIndependentClanInWeeks).ToWeeks);
            }
        }

        public override void SyncData(IDataStore dataStore)
        {
            dataStore.SyncData("_independentClans", ref _independentClans);
        }
    }
}
