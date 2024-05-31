using System.Collections.Generic;
using System.Linq;
using Helpers;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Actions;
using TaleWorlds.CampaignSystem.Encounters;
using TaleWorlds.CampaignSystem.GameMenus;
using TaleWorlds.CampaignSystem.GameState;
using TaleWorlds.CampaignSystem.MapEvents;
using TaleWorlds.CampaignSystem.Overlay;
using TaleWorlds.CampaignSystem.Party;
using TaleWorlds.CampaignSystem.Roster;
using TaleWorlds.CampaignSystem.Settlements;
using TaleWorlds.Core;
using TaleWorlds.Library;
using TaleWorlds.Localization;
using TOR_Core.CharacterDevelopment.CareerSystem;
using TOR_Core.Extensions;
using TOR_Core.Utilities;

namespace TOR_Core.CampaignMechanics.ServeAsAMerc
{
    public class ServeAsAHirelingCampaignBehavior : CampaignBehaviorBase
    {
        private int _manuallyFoughtBattles;
        private const float _minimumServeDays = 20;
        private float _durationInDays;
        private bool _hirelingEnlisted;
        private Hero _hirelingEnlistingLord;
        private bool _hirelingEnlistingLordIsAttacking;
        private bool _hirelingLordIsFightingWithoutPlayer;
        private float _ratioPartyAgainstEnemyStrength = 0;
        private readonly float _percentageOfBalanceRequiredToAvoidFight = 0.30f;
        private bool debugSkipBattles = false;
        private bool _pauseModeToggle;

        private bool _startBattle;
        private bool _siegeBattleMissionStarted;

        private bool _hirelingWaitMenuShown;

        private float _entryServiceTimeStamp;

        private SkillObject _currentTrainedSkill;


        public float DurationInDays
        {
            get => _durationInDays;
        }

        public int ManuallyFoughtBattles
        {
            get => _manuallyFoughtBattles;
        }


        public bool IsEnlisted()
        {
            return _hirelingEnlisted;
        }

        public override void RegisterEvents()
        {
            CampaignEvents.OnSessionLaunchedEvent.AddNonSerializedListener(this, ServeAsAMercDialog);
            CampaignEvents.TickEvent.AddNonSerializedListener(this, OnTick);
            CampaignEvents.SettlementEntered.AddNonSerializedListener(this, EnlistingLordPartyEntersSettlement);
            CampaignEvents.OnSettlementLeftEvent.AddNonSerializedListener(this, OnPartyLeavesSettlement);
            CampaignEvents.OnPlayerBattleEndEvent.AddNonSerializedListener(this, controlPlayerLoot);                //Those events are never executed when the player lose a battle!
            CampaignEvents.MapEventEnded.AddNonSerializedListener(this, mapEventEnded);
            CampaignEvents.GameMenuOpened.AddNonSerializedListener(this, BattleMenuOpened);
            CampaignEvents.GameMenuOptionSelectedEvent.AddNonSerializedListener(this, ContinueTimeAfterLeftSettlementWhileEnlisted);
            CampaignEvents.DailyTickEvent.AddNonSerializedListener(this,DailyRenownGain);
        }

        private void DailyRenownGain()
        {
            var gain = 3;
            
            var clanTier = Hero.MainHero.Clan.Tier;

            gain += clanTier;
            
            
            Hero.MainHero.Clan.AddRenown(gain);
        }


        private void ContinueTimeAfterLeftSettlementWhileEnlisted(GameMenuOption obj)
        {
            if (_hirelingEnlisted && obj.IdString =="town_leave")
            {
                GameMenu.ActivateGameMenu("hireling_menu");
                Campaign.Current.TimeControlMode = CampaignTimeControlMode.StoppableFastForward;
            }
        }

        private float GetEnlistingLordEventStrengthRatio(MapEvent mapEvent)
        {
            var t = mapEvent.GetMapEventSide(BattleSideEnum.Attacker);
            BattleSideEnum side;
            if(t.Parties.Where(x => x.Party == _hirelingEnlistingLord.PartyBelongedTo.Party).Any())
            {
                side = BattleSideEnum.Attacker;
            }
            else
            {
                side = BattleSideEnum.Defender;
            }
            mapEvent.GetStrengthsRelativeToParty(side, out float enlistingLordStrength, out float enemyStrength);

            if (enemyStrength > 0)
            {
                return enlistingLordStrength / enemyStrength;
            }

            return 1;
        }
        
        private void BattleMenuOpened(MenuCallbackArgs obj)
        {

            if (_startBattle && obj.MenuContext.GameMenu.StringId == "encounter" && !debugSkipBattles)
            {
                _startBattle = false;
                
                MenuHelper.EncounterAttackConsequence(obj);
            }
            if (debugSkipBattles && _hirelingEnlistingLordIsAttacking)
            {
                _startBattle = false;
            }

        }


        private void LeaveEnlistingParty(string menuToReturn)
        {
            var desertion = _durationInDays < _minimumServeDays;
            var damage = new TextObject("");
            if (desertion)
            {
                damage = new TextObject("This will harm your relations with the entire faction.");
            }
            TextObject titleText = new TextObject("{=FLT0000044}Abandon Party", null);
            TextObject text = new TextObject("{=FLT0000046}Are you sure you want to abandon the party? "+ damage, null);
            TextObject affrimativeText = new TextObject("{=FLT0000047}Yes", null);
            TextObject negativeText = new TextObject("{=FLT0000048}No", null);
            InformationManager.ShowInquiry(new InquiryData(titleText.ToString(), text.ToString(), true, true, affrimativeText.ToString(), negativeText.ToString(), delegate ()
            {
                if (desertion)
                {
                    ChangeCrimeRatingAction.Apply(_hirelingEnlistingLord.MapFaction, 55f, true);
                    foreach (Clan clan in _hirelingEnlistingLord.Clan.Kingdom.Clans)
                    {
                        bool flag2 = !clan.IsUnderMercenaryService;
                        if (flag2)
                        {
                            ChangeRelationAction.ApplyPlayerRelation(clan.Leader, -10, true, true);
                        }
                    }
                }
                
                LeaveLordPartyAction();
                GameMenu.ExitToLast();
            }, delegate ()
            {
                GameMenu.ActivateGameMenu(menuToReturn);
            }, "", 0f, null, null, null), false, false);
        }
        
        // INIT PHASE


        private void InitializeDialogs(CampaignGameStarter campaignGameStarter)
        {
            campaignGameStarter.AddPlayerLine("convincelord", "lord_talk_speak_diplomacy_2", "payedsword", "I am hereby offering my sword.", null, EnlistPlayer);
            campaignGameStarter.AddDialogLine("payedsword", "payedsword", "end", "As you wish.", null, null, 200, null);
        }

        private void SetupButtonTexts(CampaignGameStarter campaignGameStarter)
        {
            var text = new TextObject("{PAUSE_ONOFF_TEXT}");
            campaignGameStarter.AddGameMenuOption("hireling_menu","pause_time_option",text.Value, null, new GameMenuOption.OnConsequenceDelegate(PauseModeToggle));
            
            TextObject pauseText = GameTexts.FindText("Hireling","PauseTime");
            
            
            pauseText.SetTextVariable("PAUSE_ONOFF", "off");

           GameTexts.SetVariable("PAUSE_ONOFF_TEXT", pauseText);

           var activity0 = new TextObject("{HIRELINGACTIVITYTEXT0}");
           var activity1 = new TextObject("{HIRELINGACTIVITYTEXT1}");
           var activity2 = new TextObject("{HIRELINGACTIVITYTEXT2}");
           var activity3 = new TextObject("{HIRELINGACTIVITYTEXT3}");
           var activity4 = new TextObject("{HIRELINGACTIVITYTEXT4}");
           
           campaignGameStarter.AddGameMenuOption("hireling_menu","activity0_option",activity0.Value, null, _ => ToggleActivity(0));
           campaignGameStarter.AddGameMenuOption("hireling_menu","activity1_option",activity1.Value, null, _ => ToggleActivity(1));
           campaignGameStarter.AddGameMenuOption("hireling_menu","activity2_option",activity2.Value, null,_ => ToggleActivity(2));
           campaignGameStarter.AddGameMenuOption("hireling_menu","activity3_option",activity3.Value, null, _ => ToggleActivity(3));
           campaignGameStarter.AddGameMenuOption("hireling_menu","activity4_option",activity4.Value, null, _ => ToggleActivity(4));
        }

        private static void SetActivities()
        {
            var career = Hero.MainHero.GetCareer();
            for (var i = 0; i < 5; i++)
            {
                if (GameTexts.TryGetText("HirelingActivity" + i, out var text, career.StringId))
                {
                    GameTexts.SetVariable("HIRELINGACTIVITYTEXT"+i,text);
                } 
            }
        }

        private void ResetAllTexts()
        {
            
        }
        private void ToggleActivity(int i)
        {
            ResetAllTexts();
            var activities = CareerHelper.GetHirelingActivities();
            GameTexts.TryGetText("HIRELINGACTIVITYTEXT" + i,out var textObject);
            GameTexts.SetVariable("HIRELINGACTIVITYTEXT", $"[{textObject}])");
            
            _currentTrainedSkill = activities[i];
        }

        private void ServeAsAMercDialog(CampaignGameStarter campaignGameStarter)
        {
            InitializeDialogs(campaignGameStarter);
            TextObject infotext = new TextObject("{ENLISTING_TEXT}", null);
            
        
            
            campaignGameStarter.AddWaitGameMenu("hireling_menu", infotext.Value, new OnInitDelegate(this.party_wait_talk_to_other_members_on_init), new OnConditionDelegate(this.wait_on_condition),
                    null, new OnTickDelegate(this.wait_on_tick), GameMenu.MenuAndOptionType.WaitMenuHideProgressAndHoursOption, GameOverlays.MenuOverlayType.None, 0f, GameMenu.MenuFlags.None, null);
            TextObject textObjectHirelingEnterSettlement = new TextObject("Enter the settlement", null);


            campaignGameStarter.AddGameMenuOption("hireling_menu", "party_wait_leave", "Desert", delegate (MenuCallbackArgs args)
            {
                TextObject text = new TextObject("{=FLT0000045}This will damage your reputation with the {FACTION}", null);
                string faction_name = (_hirelingEnlistingLord != null) ? _hirelingEnlistingLord.MapFaction.Name.ToString() : "DATA CORRUPTION ERROR";
                text.SetTextVariable("FACTION", faction_name);
                args.Tooltip = text;
                args.optionLeaveType = GameMenuOption.LeaveType.Escape;
                return true;
            }, delegate (MenuCallbackArgs args)
            {
                LeaveEnlistingParty("hireling_menu");
            }, true, -1, false, null);

            campaignGameStarter.AddGameMenuOption("hireling_menu", "enter_town", textObjectHirelingEnterSettlement.ToString(), delegate (MenuCallbackArgs args)
            {

                args.optionLeaveType = GameMenuOption.LeaveType.Continue;
                TORCommon.Say((_hirelingEnlistingLord.PartyBelongedTo.CurrentSettlement != null).ToString());

                return _hirelingEnlistingLord.PartyBelongedTo.CurrentSettlement != null;
            }, delegate (MenuCallbackArgs args)
            {
                while (Campaign.Current.CurrentMenuContext != null)
                {
                    GameMenu.ExitToLast();
                }
               
                EncounterManager.StartSettlementEncounter(MobileParty.MainParty.Party.MobileParty, _hirelingEnlistingLord.PartyBelongedTo.CurrentSettlement);
                EnterSettlementAction.ApplyForParty(MobileParty.MainParty.Party.MobileParty, _hirelingEnlistingLord.CurrentSettlement);
            }, true, -1, false, null);
            
            
            SetupButtonTexts(campaignGameStarter);
            
            
            TextObject hirelingBattleTextMenu = new TextObject("This is a test of Hireling BattleMenu", null);
            campaignGameStarter.AddGameMenu("hireling_battle_menu", hirelingBattleTextMenu.Value, new OnInitDelegate(this.party_wait_talk_to_other_members_on_init), GameOverlays.MenuOverlayType.Encounter, GameMenu.MenuFlags.None, null);

            campaignGameStarter.AddGameMenuOption("hireling_battle_menu", "hireling_join_battle", "Join battle",
                hireling_battle_menu_join_battle_on_condition,
                delegate (MenuCallbackArgs args)
                {

                    while (Campaign.Current.CurrentMenuContext != null)
                    {
                        GameMenu.ExitToLast();
                    }
                    if (_hirelingEnlistingLord.PartyBelongedTo.MapEvent != null)
                    {
                        if (_hirelingEnlistingLordIsAttacking)
                        {
                            var mapEvent = _hirelingEnlistingLord.PartyBelongedTo.MapEvent;
                            TORCommon.Say("attack!"); 
                            StartBattleAction.Apply(PartyBase.MainParty, mapEvent.DefenderSide.LeaderParty);
                            
                            MobileParty.MainParty.CurrentSettlement= _hirelingEnlistingLord.PartyBelongedTo.MapEvent.MapEventSettlement;

                            if (mapEvent.IsSiegeAssault)
                            {
                                Game.Current.AfterTick += InitializeSiegeBattle;    //deliberate waiting until all information is copied over, atleast that's what I assume is happening?
                                _siegeBattleMissionStarted = true;
                            }
                        }
                        else
                        {
                            TORCommon.Say("defend!");
                            StartBattleAction.Apply(_hirelingEnlistingLord.PartyBelongedTo.MapEvent.AttackerSide.LeaderParty, PartyBase.MainParty);
                        }
                        _startBattle = true;
                    }
                }
                , false, 4, false);

            campaignGameStarter.AddGameMenuOption("hireling_battle_menu", "hireling_avoid_combat", "Avoid Combat",
               hireling_battle_menu_avoid_combat_on_condition,
               delegate (MenuCallbackArgs args)
               {
                   _hirelingLordIsFightingWithoutPlayer = true;
                   _startBattle = false;
                   args.MenuContext.GameMenu.StartWait();
               }
               , false, 4, false);

            campaignGameStarter.AddGameMenuOption("hireling_battle_menu", "hireling_flee", "Desert",
               hireling_battle_menu_desert_on_condition,
               delegate (MenuCallbackArgs args)
               {
                   LeaveEnlistingParty("hireling_battle_menu");

               }
               , false, 4, false);
        }

        private void PauseModeToggle(MenuCallbackArgs args)
        {
            _pauseModeToggle = !_pauseModeToggle;

            var onOffText = "Off";
            if (_pauseModeToggle)
            {
                onOffText = "On";
            }

            TextObject text1 = args.Text;
            TextObject text2 = GameTexts.FindText("Hireling","PauseTime");
            text2.SetTextVariable("PAUSE_ONOFF", onOffText);
            
           // GameTexts.SetVariable("PAUSE_ONOFF",onOffText);
            GameTexts.SetVariable("PAUSE_ONOFF_TEXT", text2);
            args.Text = text2;
            args.MenuContext.Refresh();
        }

        public void LeaveLordPartyAction()
        {
            _hirelingEnlisted = false;
            _hirelingEnlistingLord = null;
            _hirelingWaitMenuShown = false;
            PlayerEncounter.Finish(true);
            UndoDiplomacy();
            ShowPlayerParty();

            _durationInDays = 0;
            _manuallyFoughtBattles = 0;

        }
        
        private void InitializeSiegeBattle(float tick)
        {
            if (!_hirelingEnlisted) return;
            if(!_siegeBattleMissionStarted) return;
            if(MobileParty.MainParty==null) return;
            var mainPartyMapEvent = MobileParty.MainParty.MapEvent;
            if (mainPartyMapEvent == null || mainPartyMapEvent.StringId == null) return; //wait until the main party event is assigned correctly
            
            StartBattleAction.Apply(PartyBase.MainParty, mainPartyMapEvent.DefenderSide.LeaderParty);
            _siegeBattleMissionStarted = false;
            Game.Current.AfterTick -= InitializeSiegeBattle;    //cleanup,  method is afterwards rendered harmless and will not affect performance 
        }
        
        private bool hireling_battle_menu_join_battle_on_condition(MenuCallbackArgs args)
        {
            var maxHitPointsHero = Hero.MainHero.MaxHitPoints;
            var hitPointsHero = Hero.MainHero.HitPoints;
            return hitPointsHero > maxHitPointsHero * 0.2;
        }
        
        private bool hireling_battle_menu_avoid_battle_on_condition(MenuCallbackArgs args)
        {
            var maxHitPointsHero = Hero.MainHero.MaxHitPoints;
            var hitPointsHero = Hero.MainHero.HitPoints;
            
            var lordEvent = _hirelingEnlistingLord.PartyBelongedTo.MapEvent;

            if(lordEvent==null) return false;
            
            var partyStrength = GetEnlistingLordEventStrengthRatio(lordEvent);
           
            var combatstregthThreshold  = partyStrength > _ratioPartyAgainstEnemyStrength;// || ennemy is 30% of our balance;

            _ratioPartyAgainstEnemyStrength = 0.3f;
            return hitPointsHero < maxHitPointsHero * 0.2 || combatstregthThreshold; 
        }
        
        private bool hireling_battle_menu_desert_on_condition(MenuCallbackArgs args)
        {
            return true;
        }

        private bool hireling_battle_menu_avoid_combat_on_condition(MenuCallbackArgs args)
        {
            return hireling_battle_menu_avoid_battle_on_condition(args);
        }


        // Token: 0x0600015C RID: 348 RVA: 0x000119BC File Offset: 0x0000FBBC   
        private bool wait_on_condition(MenuCallbackArgs args)
        {
            //TORCommon.Say("WAIT ON CONDITION");
            return true;
        }
        
        private void wait_on_tick(MenuCallbackArgs args, CampaignTime time)
        {
            bool flag = _hirelingEnlistingLord == null || !_hirelingEnlisted;
            if (flag)
            {
                while (Campaign.Current.CurrentMenuContext != null)
                {
                    GameMenu.ExitToLast();
                }
            }
            else
            {
                if(args.MenuContext?.GameMenu == null) return;
                TextObject text1 = args.MenuContext.GameMenu.GetText();
                TextObject text2 = GameTexts.FindText("Hireling","MainText");
                text2.SetTextVariable("ENLISTING_LORD", _hirelingEnlistingLord.Name);
                
                var days = $"{_durationInDays:0.0}";
                text2.SetTextVariable("ENLISTING_DURATION", days);
                var armyInfo = "";
                if(_hirelingEnlistingLord.PartyBelongedTo.Army!=null)
                {
                    armyInfo += "{newLine}";
                    armyInfo += $"is Part of {_hirelingEnlistingLord.PartyBelongedTo.Army.Name}";
                }
                text2.SetTextVariable("ENLISTING_ARMY", armyInfo);
                
                TextObject variable = text2;
                text1.SetTextVariable("ENLISTING_TEXT", variable);
                
                
                args.MenuContext.SetBackgroundMeshName(_hirelingEnlistingLord.MapFaction.Culture.EncounterBackgroundMesh);
            }
        }
        

        public override void SyncData(IDataStore dataStore)
        {
            dataStore.SyncData<bool>("_enlisted", ref _hirelingEnlisted);
            dataStore.SyncData<Hero>("_enlistingLord", ref _hirelingEnlistingLord);
            dataStore.SyncData<float>("_entryServiceTimeStamp", ref _entryServiceTimeStamp);
            dataStore.SyncData<int>("_manuallyFoughtBattles", ref _manuallyFoughtBattles);
            dataStore.SyncData<float>("_durationInDays", ref _durationInDays);
        }

        private void party_wait_talk_to_other_members_on_init(MenuCallbackArgs args)
        {

        }


        // BATTLE HANDLERS


        private void controlPlayerLoot(MapEvent mapEvent)
        {
            if (mapEvent.PlayerSide == mapEvent.WinningSide && IsEnlisted())
            {

                if (!_hirelingLordIsFightingWithoutPlayer)
                {
                    _manuallyFoughtBattles++;
                }
                
                PlayerEncounter.Current.RosterToReceiveLootItems.Clear();
                PlayerEncounter.Current.RosterToReceiveLootMembers.Clear();
                PlayerEncounter.Current.RosterToReceiveLootPrisoners.Clear();
            }

            _hirelingWaitMenuShown = false;
        }



        // SETTLEMENTS HANDLERS

        private void OnPartyLeavesSettlement(MobileParty mobileParty, Settlement settlement)
        {

            if (!_hirelingEnlisted || _hirelingEnlistingLord == null) return;
           
            if (_hirelingEnlistingLord.PartyBelongedTo == mobileParty || (MobileParty.MainParty == mobileParty && mobileParty.CurrentSettlement == null))
            {


                while (Campaign.Current.CurrentMenuContext != null)
                    GameMenu.ExitToLast();
                GameMenu.ActivateGameMenu("hireling_menu");
                if (PartyBase.MainParty.MobileParty.CurrentSettlement != null)
                    LeaveSettlementAction.ApplyForParty(PartyBase.MainParty.MobileParty);
            }
        }

        private void EnlistingLordPartyEntersSettlement(MobileParty mobileParty, Settlement settlement, Hero arg3)
        {
            if (!_hirelingEnlisted) return;
            if ( _hirelingEnlistingLord != null && _hirelingEnlistingLord.PartyBelongedTo == mobileParty)
            {
                if (_pauseModeToggle)
                {

                    while (Campaign.Current.CurrentMenuContext != null)
                        GameMenu.ExitToLast();
                    GameMenu.ActivateGameMenu("hireling_menu");
                    Campaign.Current.TimeControlMode = CampaignTimeControlMode.Stop;
                }

                //EnterSettlementAction.ApplyForParty(PartyBase.MainParty.MobileParty, settlement);
            }
        }


        // CAMPAIGN HANDLERS





        // MAP EVENT AND ENCOUNTERS

        // If the enlisted lord is fighting (in a map event) without us, we should fight with him

        // If the player is concerned by the end of a map event, we should start displaying the menu.
        private void mapEventEnded(MapEvent mapEvent)
        {
            if(_hirelingEnlistingLord == null|| !IsEnlisted()) return; 
        
            
            if (_hirelingEnlistingLord != null && !mapEvent.IsPlayerMapEvent && getEnlistingLordisInMapEvent(mapEvent))
            {
                
                GameMenu.SwitchToMenu("hireling_menu");
                _hirelingLordIsFightingWithoutPlayer = false;
            }
            if (mapEvent.IsPlayerMapEvent)
            {
                GameMenu.SwitchToMenu("hireling_menu");
            }
            // TODO: Refining this part
        }



        // ON TICKS HANDLERS
        private void OnTick(float dt)
        {
            if (_hirelingLordIsFightingWithoutPlayer)
            {
                if (!MobileParty.MainParty.ShouldBeIgnored)
                {
                    // This part has not been tested, but it should work.
                    MobileParty.MainParty.IgnoreForHours(1);
                }

            }

            if (_hirelingEnlisted && _hirelingEnlistingLord != null)
            {
                
                var menu = Campaign.Current.GameMenuManager.GetGameMenu("hireling_menu");
                _durationInDays = Campaign.Current.CampaignStartTime.ElapsedDaysUntilNow - _entryServiceTimeStamp;
                menu.RunOnTick(Campaign.Current.CurrentMenuContext,dt);


             
                
                if (!_hirelingWaitMenuShown)
                {
                    GameMenu.ActivateGameMenu("hireling_menu");
                    _hirelingWaitMenuShown = true;
                }


                HidePlayerParty();
                PartyBase.MainParty.MobileParty.Position2D = _hirelingEnlistingLord.PartyBelongedTo.Position2D;
                if (_hirelingEnlistingLord.PartyBelongedTo.MapEvent != null && MobileParty.MainParty.MapEvent == null)
                {
                    var mapEvent = _hirelingEnlistingLord.PartyBelongedTo.MapEvent;

                    
                    // TODO: CHECK THE DEFENDING PART
                    _hirelingEnlistingLordIsAttacking = false;

                    TORCommon.Say("Lord starts encounter");
                    foreach (var party in mapEvent.AttackerSide.Parties)
                    {
                        if (party.Party == _hirelingEnlistingLord.PartyBelongedTo.Party)
                        {

                            _hirelingEnlistingLordIsAttacking = true;
                            break;
                        }
                    }
                    
                    if (!_hirelingLordIsFightingWithoutPlayer)
                    {
                        GameMenu.ActivateGameMenu("hireling_battle_menu");
                    }
                }
                
            }
        }


        // ACTIONS

        private void UndoDiplomacy()
        {
            ChangeKingdomAction.ApplyByLeaveKingdomAsMercenary(Hero.MainHero.Clan, false);
        }


        private void EnlistPlayer()
        {
            HidePlayerParty();
            DisbandParty();
            Hero.MainHero.AddAttribute("enlisted");
            _hirelingEnlistingLord = CharacterObject.OneToOneConversationCharacter.HeroObject;
            ChangeKingdomAction.ApplyByJoinFactionAsMercenary(Hero.MainHero.Clan, _hirelingEnlistingLord.Clan.Kingdom, 25, false);
            MBTextManager.SetTextVariable("ENLISTINGLORDNAME", _hirelingEnlistingLord.EncyclopediaLinkWithName);

            
            while (Campaign.Current.CurrentMenuContext != null)
                GameMenu.ExitToLast();
            _hirelingEnlisted = true;

            SetActivities();

             _entryServiceTimeStamp = Campaign.Current.CampaignStartTime.ElapsedDaysUntilNow;
            GameMenu.ActivateGameMenu("hireling_menu");


        }

        private void ShowPlayerParty()
        {
            // Currently not working
            PartyBase.MainParty.MobileParty.IsVisible = true;
        }

        private void HidePlayerParty()
        {
            PartyBase.MainParty.MobileParty.IsVisible = false;
        }


        private void DisbandParty()
        {
            if (MobileParty.MainParty.MemberRoster.TotalManCount <= 1)
                return;
            List<TroopRosterElement> troopRosterElementList = new List<TroopRosterElement>();
            foreach (TroopRosterElement troopRosterElement in (List<TroopRosterElement>)MobileParty.MainParty.MemberRoster.GetTroopRoster())
            {
                if (troopRosterElement.Character != Hero.MainHero.CharacterObject && troopRosterElement.Character.HeroObject == null)
                    troopRosterElementList.Add(troopRosterElement);
            }
            if (troopRosterElementList.Count == 0)
                return;
            foreach (TroopRosterElement troopRosterElement in troopRosterElementList)
            {
                //Test.followingHero.PartyBelongedTo.MemberRoster.AddToCounts(troopRosterElement.Character, troopRosterElement.Number);
                MobileParty.MainParty.MemberRoster.AddToCounts(troopRosterElement.Character, -1 * troopRosterElement.Number);
            }
        }


        // GETTERS 
        public bool getEnlistingLordisInMapEvent(MapEvent mapEvent)
        {

            var lordIsInMapEvent = false;
            if (_hirelingEnlistingLord == null)
                return false;

            if (_hirelingEnlistingLord.PartyBelongedTo == null)
            {
                return false;
            }

            foreach (var party in mapEvent.AttackerSide.Parties)
            {
                if (party.Party == _hirelingEnlistingLord.PartyBelongedTo.Party)
                {
                    lordIsInMapEvent = true;
                }
            }
            foreach (var party in mapEvent.DefenderSide.Parties)
            {
                if (party.Party == _hirelingEnlistingLord.PartyBelongedTo.Party)
                {
                    lordIsInMapEvent = true;
                }
            }

            return lordIsInMapEvent;
        }
    }
}