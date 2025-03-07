﻿using Helpers;
using System;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Actions;
using TaleWorlds.CampaignSystem.Encounters;
using TaleWorlds.CampaignSystem.GameMenus;
using TaleWorlds.CampaignSystem.MapEvents;
using TaleWorlds.CampaignSystem.Overlay;
using TaleWorlds.CampaignSystem.Party;
using TaleWorlds.CampaignSystem.Settlements;
using TaleWorlds.Core;
using TaleWorlds.Localization;
using TaleWorlds.ObjectSystem;
using TOR_Core.Extensions;
using TOR_Core.Missions;
using TOR_Core.Utilities;

namespace TOR_Core.CampaignMechanics.RaiseDead
{
    public class RaiseDeadInTownBehavior : CampaignBehaviorBase
    {
        private CharacterObject _skeleton;
        private CampaignTime _startWaitTime = CampaignTime.Now;
        private MobileParty _currentWatchParty;
        private bool _isMissionStarted;
        private Settlement _currentSettlement;
        public override void RegisterEvents()
        {
            CampaignEvents.OnSessionLaunchedEvent.AddNonSerializedListener(this, Initialize);
            CampaignEvents.HourlyTickPartyEvent.AddNonSerializedListener(this, HourlyPartyTick);
            CampaignEvents.OnPlayerBattleEndEvent.AddNonSerializedListener(this, OnBattleEnded);
        }

        private void OnBattleEnded(MapEvent mapevent)
        {
            if (_isMissionStarted && mapevent.WinningSide == mapevent.PlayerSide)
            {
                _isMissionStarted = false;
                _currentWatchParty = null;
                _currentSettlement = null;
            }
        }

        private void HourlyPartyTick(MobileParty party)
        {
            if (party == _currentWatchParty && _isMissionStarted)
            {
                _isMissionStarted = false;
                if (_currentSettlement != null)
                {
                    if (Hero.MainHero.IsPrisoner && _currentWatchParty.PrisonRoster.Contains(CharacterObject.PlayerCharacter))
                    {
                        TransferPrisonerAction.Apply(CharacterObject.PlayerCharacter, _currentWatchParty.Party, _currentSettlement.Party);
                    }
                    DestroyPartyAction.ApplyForDisbanding(_currentWatchParty, _currentSettlement);
                }
                _currentWatchParty = null;
                _currentSettlement = null;
            }
        }

        private void SwitchToMenuIfThereIsAnInterrupt(string currentMenuId)
        {
            string genericStateMenu = Campaign.Current.Models.EncounterGameMenuModel.GetGenericStateMenu();
            if (genericStateMenu != currentMenuId)
            {
                if (!string.IsNullOrEmpty(genericStateMenu))
                {
                    GameMenu.SwitchToMenu(genericStateMenu);
                    return;
                }
                GameMenu.ExitToLast();
            }
        }

        private void MakeNecromancers()
        {
            foreach (var hero in Hero.AllAliveHeroes)
            {
                if (hero.Culture.StringId == TORConstants.Cultures.SYLVANIA && !hero.IsNecromancer() && (hero.IsLord || hero.IsWanderer) && hero != Hero.MainHero)
                {
                    hero.AddAttribute("Necromancer");
                }
            }
        }

        private void Initialize(CampaignGameStarter obj)
        {
            obj.AddGameMenuOption("town", "graveyard", "Go to the graveyard",
                 graveyardaccesscondition,
                delegate (MenuCallbackArgs args)
                {
                    GameMenu.SwitchToMenu("graveyard");
                }
                , false, 4, false);

            obj.AddGameMenu("graveyard", "{GRAVEYARD_INTRODUCTION}",
                delegate (MenuCallbackArgs args)
                {
                    args.MenuTitle = new TextObject("Graveyard");
                    var intro = new TextObject("{=tor_settlement_graveyard_introduction}You have arrived at {SETTLEMENT_NAME}'s Graveyard. Graves, tombstones and family crypts litter the peaceful hillside.");
                    MBTextManager.SetTextVariable("SETTLEMENT_NAME", Settlement.CurrentSettlement.Name);
                    MBTextManager.SetTextVariable("GRAVEYARD_INTRODUCTION", intro);
                },
                GameOverlays.MenuOverlayType.SettlementWithBoth, GameMenu.MenuFlags.None, null);

            obj.AddGameMenuOption("graveyard", "raise_dead_attempt", new TextObject ("{=tor_settlement_graveyard_raise_dead_action}Raise dead from the corpses in the ground (wait 8 hours).").ToString(),
                raisedeadattemptcondition,
                delegate (MenuCallbackArgs args)
                {
                    GameMenu.SwitchToMenu("raising_dead");
                }
                , false, -1, false);

            obj.AddGameMenuOption("graveyard", "graveyard_leave", "Leave",
                delegate (MenuCallbackArgs args)
                {
                    args.optionLeaveType = GameMenuOption.LeaveType.Leave;
                    return true;
                },
                delegate (MenuCallbackArgs args)
                {
                    GameMenu.SwitchToMenu("town");
                }, true, -1, false);

            obj.AddWaitGameMenu("raising_dead", "{=tor_settlement_graveyard_raise_dead_begin_text}The common folk's graves are ripe for the taking! You spend time to raise corpses from the ground. Morr is going to be furious tonight!",
                delegate (MenuCallbackArgs args)
                {
                    _startWaitTime = CampaignTime.Now;
                    PlayerEncounter.Current.IsPlayerWaiting = true;
                    args.MenuContext.GameMenu.StartWait();
                },
                null,
                raisingdeadconsequence,
                raisingdeadtick,
                GameMenu.MenuAndOptionType.WaitMenuShowProgressAndHoursOption, GameOverlays.MenuOverlayType.SettlementWithBoth, 8f, GameMenu.MenuFlags.None, null);

            obj.AddGameMenuOption("raising_dead", "raising_dead_leave", "Leave",
                delegate (MenuCallbackArgs args)
                {
                    args.optionLeaveType = GameMenuOption.LeaveType.Leave;
                    return true;
                },
                delegate (MenuCallbackArgs args)
                {
                    PlayerEncounter.Current.IsPlayerWaiting = false;
                    SwitchToMenuIfThereIsAnInterrupt(args.MenuContext.GameMenu.StringId);
                }, true, -1, false);

            obj.AddGameMenu("graveyard_interrupt", "{GRAVEYARD_INTERRUPT}",
                delegate (MenuCallbackArgs args)
                {
                    args.MenuTitle = new TextObject("Caught in the act");
                    var text = new TextObject("{=tor_settlement_graveyard_raise_dead_interrupt}The local nightwatch is onto you. Face the consequences of your vile actions.");
                    MBTextManager.SetTextVariable("GRAVEYARD_INTERRUPT", text);
                    CalculateAndApplyCrimeRatingChange();
                },
                GameOverlays.MenuOverlayType.SettlementWithBoth, GameMenu.MenuFlags.None, null);

            obj.AddGameMenuOption("graveyard_interrupt", "interrupt_battle", "{=tor_settlement_graveyard_raise_dead_interrupt_action}Defend yourself",
                delegate (MenuCallbackArgs args)
                {
                    if (!Hero.MainHero.IsWounded)
                    {
                        args.optionLeaveType = GameMenuOption.LeaveType.DefendAction;
                        return true;
                    }
                    else return false;
                },
                (MenuCallbackArgs args) => SetupBattle(), false, -1, false);

            obj.AddGameMenuOption("graveyard_interrupt", "interrupt_surrender", "Surrender",
                delegate (MenuCallbackArgs args)
                {
                    args.optionLeaveType = GameMenuOption.LeaveType.LeaveTroopsAndFlee;
                    return true;
                },
                delegate (MenuCallbackArgs args)
                {
                    PlayerEncounter.Current.IsPlayerWaiting = false;
                    PlayerEncounter.Finish(false);
                    TakePrisonerAction.Apply(Settlement.CurrentSettlement.Party, Hero.MainHero);

                }, true, -1, false);

            _skeleton = MBObjectManager.Instance.GetObject<CharacterObject>("tor_vc_skeleton");
            MakeNecromancers();
        }

        private bool graveyardaccesscondition(MenuCallbackArgs args)
        {
            bool shouldBeDisabled;
            TextObject disabledText;
            bool canPlayerDo = Campaign.Current.Models.SettlementAccessModel.CanMainHeroAccessLocation(Settlement.CurrentSettlement, "center", out shouldBeDisabled, out disabledText);
            disabledText = new TextObject("{=tor_settlement_graveyard_closed}The Graveyard's massive iron gates are closed shut.");
            args.optionLeaveType = GameMenuOption.LeaveType.Submenu;
            if (canPlayerDo)
            {
                canPlayerDo = Hero.MainHero.CanRaiseDead() && !Settlement.CurrentSettlement.IsUnderSiege;
            }
            return MenuHelper.SetOptionProperties(args, canPlayerDo, shouldBeDisabled, disabledText);
        }

        private bool raisedeadattemptcondition(MenuCallbackArgs args)
        {
            if (Hero.MainHero.CanRaiseDead() && !Settlement.CurrentSettlement.IsUnderSiege)
            {
                args.optionLeaveType = GameMenuOption.LeaveType.Wait;
                return true;
            }
            else return false;
        }

        private void raisingdeadconsequence(MenuCallbackArgs args)
        {
            PlayerEncounter.Current.IsPlayerWaiting = false;
            args.MenuContext.GameMenu.EndWait();
            args.MenuContext.GameMenu.SetProgressOfWaitingInMenu(0f);
            GameMenu.SwitchToMenu("graveyard");
        }

        private void raisingdeadtick(MenuCallbackArgs args, CampaignTime dt)
        {
            if (Settlement.CurrentSettlement.IsUnderSiege)
            {
                InterruptWaitSiege(args);
                return;
            }
            float progress = args.MenuContext.GameMenu.Progress;
            int diff = (int)_startWaitTime.ElapsedHoursUntilNow;
            if (diff > 0)
            {
                args.MenuContext.GameMenu.SetProgressOfWaitingInMenu(diff * 0.125f);
                if (args.MenuContext.GameMenu.Progress != progress)
                {
                    if (_skeleton != null && MobileParty.MainParty.MemberRoster.TotalManCount <= MobileParty.MainParty.Party.PartySizeLimit)
                    {
                        int raisePower = Math.Max(1, (int)Hero.MainHero.GetExtendedInfo().SpellCastingLevel);
                        MobileParty.MainParty.MemberRoster.AddToCounts(_skeleton, 2 * raisePower);
                    }
                    var rng = MBRandom.RandomFloatRanged(0, 1);
                    if (rng > 0.95f && Settlement.CurrentSettlement != null && Settlement.CurrentSettlement.OwnerClan != Clan.PlayerClan)
                    {
                        InterruptWait(args);
                    }
                }
            }
        }
        private void SetupBattle()
        {
            PlayerEncounter.Current.IsPlayerWaiting = false;
            _currentWatchParty = GraveyardNightWatchPartyComponent.CreateParty(Settlement.CurrentSettlement);
            _currentSettlement = Settlement.CurrentSettlement;
            PlayerEncounter.RestartPlayerEncounter(PartyBase.MainParty, _currentWatchParty.Party, true);
            if (PlayerEncounter.Battle == null)
            {
                PlayerEncounter.StartBattle();
                PlayerEncounter.Update();
            }
            _isMissionStarted = true;
            TorMissionManager.OpenGraveyardMission();
        }

        private void CalculateAndApplyCrimeRatingChange()
        {
            //Have to make sure crime rating stays within the moderate range (30 - 65) if it isn't already more otherwise declaration of war occurs.
            float ratingChange = 0;
            float currentRating = Settlement.CurrentSettlement.MapFaction.MainHeroCrimeRating;
            if (currentRating < 30f)
            {
                ratingChange = (30f - currentRating) + 5;
            }
            else if (currentRating >= 30f && currentRating < 65f)
            {
                ratingChange = Math.Min(20f, (65f - currentRating - 5));
            }
            else if (currentRating >= 65f) ratingChange = 20f;
            if (ratingChange > 0) ChangeCrimeRatingAction.Apply(Settlement.CurrentSettlement.MapFaction, ratingChange, true);
        }

        private void InterruptWait(MenuCallbackArgs args)
        {
            PlayerEncounter.Current.IsPlayerWaiting = false;
            args.MenuContext.GameMenu.EndWait();
            args.MenuContext.GameMenu.SetProgressOfWaitingInMenu(0f);
            GameMenu.SwitchToMenu("graveyard_interrupt");
        }

        private void InterruptWaitSiege(MenuCallbackArgs args)
        {
            PlayerEncounter.Current.IsPlayerWaiting = false;
            args.MenuContext.GameMenu.EndWait();
            args.MenuContext.GameMenu.SetProgressOfWaitingInMenu(0f);
            GameMenu.SwitchToMenu("town");
        }

        public override void SyncData(IDataStore dataStore)
        {
            //throw new NotImplementedException();
        }
    }
}
