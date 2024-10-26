﻿using SandBox;
using SandBox.View.Map;
using System;
using System.Collections.Generic;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Actions;
using TaleWorlds.CampaignSystem.Party;
using TaleWorlds.CampaignSystem.Settlements;
using TaleWorlds.Core;
using TaleWorlds.Engine;
using TaleWorlds.Library;
using TaleWorlds.Localization;
using TaleWorlds.ObjectSystem;
using TaleWorlds.ScreenSystem;
using TOR_Core.CampaignMechanics.RaidingParties;
using TOR_Core.Utilities;

namespace TOR_Core.CampaignMechanics.TORCustomSettlement
{
    public class CursedSiteComponent : TORBaseSettlementComponent, IDisposable
    {
        private int _wardHours = 0;
        private bool _isMarkerShown = false;
        private GameEntity _markerEntity;
        private Decal _markerDecal;
        public int WardHours
        {
            get { return _wardHours; }
            set 
            { 
                _wardHours = value;
                if (_wardHours == 0) IsActive = true;
            }
        }
        public void HourlyTick() => WardHours = Math.Max(0, WardHours - 1);

        public override void OnInit()
        {
            base.OnInit();
            InformationManager.OnShowTooltip += OnShowTooltip;
            InformationManager.OnHideTooltip += OnHideTooltip;
        }

        private void OnShowTooltip(Type type, object[] args)
        {
            if (type == typeof(Settlement) && ScreenManager.TopScreen is MapScreen)
            {
                var settlement = args[0] as Settlement;
                if (settlement == Settlement)
                {
                    ShowAreaMarker(true);
                }
            }
        }

        private void OnHideTooltip()
        {
            if (_isMarkerShown) ShowAreaMarker(false);
        }

        private void ShowAreaMarker(bool shouldShow)
        {
            if (_markerEntity == null) CreateVisuals();
            if (_markerEntity != null)
            {
                MatrixFrame frame = new MatrixFrame(Mat3.Identity, PartyVisualManager.Current.GetVisualOfParty(Settlement.Party).GetVisualPosition());
                frame.Scale(new Vec3(32, 32, 1));
                _markerEntity.SetGlobalFrame(frame);
                _markerDecal.SetFactor1Linear(4281663744U);
                _markerEntity.SetVisibilityExcludeParents(shouldShow);
            }
            _isMarkerShown = shouldShow;
        }

        private void CreateVisuals()
        {
            MapScene mapScene = Campaign.Current.MapSceneWrapper as MapScene;
            _markerEntity = GameEntity.CreateEmpty(mapScene.Scene, true);
            _markerEntity.Name = "CursedSiteMarker";
            _markerDecal = Decal.CreateDecal();
            if (_markerDecal != null && _markerEntity != null)
            {
                Material resource = Material.GetFromResource("decal_city_circle_a");
                _markerDecal.SetMaterial(resource);
                mapScene.Scene.AddDecalInstance(_markerDecal, "editor_set", false);
                _markerEntity.AddComponent(_markerDecal);
            }
        }

        public void Dispose()
        {
            InformationManager.OnShowTooltip -= OnShowTooltip;
            InformationManager.OnHideTooltip -= OnHideTooltip;
            _markerDecal = null;
            _markerEntity = null;
        }
    }

    public class ShrineComponent : TORBaseSettlementComponent { }

    public class OakOfAgesComponent : TORBaseSettlementComponent { }

    public class WorldRootsComponent : TORBaseSettlementComponent { }

    public class ChaosPortalComponent : BaseRaiderSpawnerComponent
    {
        public override string BattleSceneName => "TOR_chaos_portal_001_forceatmo";

        public override List<string> RewardItemIds => new()
        {
            "tor_empire_weapon_sword_runefang_001",
            
            "tor_chaos_weapon_metal_ud_lance_001",
            
            "tor_chaos_arm_gauntlet_slaneesh_warrior_001",
            "tor_chaos_arm_gauntlet_khorne_warrior_001",
            "tor_chaos_arm_gauntlet_khorne_warrior_002",
            "tor_chaos_arm_gauntlet_tzeentch_warrior_001",
            "tor_chaos_arm_gauntlet_hallow",
            "tor_chaos_arm_bracers_marauder_001",
            
            "tor_chaos_leg_boots_slaneesh_warrior_001",
            "tor_chaos_leg_boots_nurgle_knight_001",
            "tor_chaos_leg_boots_khorne_warrior_001",
            "tor_chaos_leg_boots_khorne_warrior_002",
            "tor_chaos_leg_boots_tzeentch_warrior_001",
            "tor_chaos_leg_boots_hallow",
            "tor_chaos_leg_boots_marauder_001",
            "tor_chaos_leg_boots_chaos_warrior_001",
            
            "tor_chaos_body_armor_slaneesh_warrior_001",
            "tor_chaos_body_armor_khorne_warrior_001",
            "tor_chaos_body_armor_khorne_warrior_002",
            "tor_chaos_body_armor_tzeentch_warrior_001",
            "tor_chaos_body_armor_nurgle_knight_001",
            "tor_chaos_body_robe_cultist_001",
            "tor_chaos_body_armor_abomination",
            "tor_chaos_body_armor_marauder_001",
            "tor_chaos_body_armor_marauder_002",
            "tor_chaos_body_armor_chaos_warrior_001",
            
            "tor_chaos_shoulder_pauldron_nurgle_001",
            "tor_chaos_shoulder_pauldron_nurgle_002",
            "tor_chaos_shoulder_pauldron_slaneesh_warrior_001",
            "tor_chaos_shoulder_pauldron_chosen_001",
            "tor_chaos_shoulder_pauldron_khorne_warrior_001",
            "tor_chaos_shoulder_pauldron_khorne_warrior_002",
            "tor_chaos_shoulder_pauldron_tzeentch_warrior_001",
            "tor_chaos_shoulder_cape_marauder_001",
            "tor_chaos_shoulder_fur_marauder_001",
            "tor_chaos_shoulder_cape_chaos_warrior_001",
            
            "tor_empire_staff_cw_001_combined",
            "tor_empire_staff_cw_002_combined",
            "tor_empire_staff_cw_003_combined",
            "tor_empire_staff_bw_001_combined",
            "tor_empire_staff_bw_002_combined",
            "tor_empire_staff_lw_001_combined",
            "tor_empire_staff_lw_002_combined",
            "tor_empire_staff_lw_003_combined",
            
        };
        

        public override void SpawnNewParty(out MobileParty party, Settlement initialTarget)
        {
            PartyTemplateObject template = MBObjectManager.Instance.GetObject<PartyTemplateObject>("chaos_lordparty_template");
            Clan chaosClan = Clan.FindFirst(x => x.StringId == "chaos_clan_1");
            var find = TORCommon.FindSettlementsAroundPosition(Settlement.Position2D, 60, x => !x.IsRaided && !x.IsUnderRaid && x.IsVillage).GetRandomElementInefficiently();
            var chaosRaidingParty = RaidingPartyComponent.CreateRaidingParty("chaos_clan_1_party_" + RaidingPartyCount + 1, Settlement, "{=tor_chaos_raiders_str}Chaos Raiders", template, chaosClan, MBRandom.RandomInt(75, 99));
            SetPartyAiAction.GetActionForRaidingSettlement(chaosRaidingParty, initialTarget ?? find);
            ((RaidingPartyComponent)chaosRaidingParty.PartyComponent).Target = initialTarget ?? find;

            party = chaosRaidingParty;
        }
    }

    public class HerdStoneComponent : BaseRaiderSpawnerComponent
    {
        public override string BattleSceneName => "TOR_beastmen_herdstone_001";

        public override List<string> RewardItemIds =>
            new()
            {
                
                "tor_vc_blood_keep_grandmaster_helm",
                
                "tor_empire_weapon_mace_holy_hammer",
                
                "tor_vc_shoulder_pauldron_vlad",
                "tor_vc_shoulder_pauldron_red_duke",
                "tor_vc_shoulder_pauldron_blood_dragon_knight_001",
                
                
                
                "tor_vc_arm_gauntlet_vlad",
                "tor_vc_arm_gauntlet_red_duke",
                "tor_vc_arm_gauntlet_blood_dragon_knight_001",
                
                "tor_empire_weapon_scythe_morr_001",
                
                
                "tor_vc_body_armor_red_duke",
                "tor_vc_body_armor_vlad",
                "tor_vc_body_armor_blood_dragon_knight_001",
                
                
                "tor_vc_leg_boots_vlad",
                "tor_vc_leg_boots_blood_dragon_knight_001",
                "tor_vc_leg_boots_red_duke",
                
                "tor_he_weapon_2h_sword_001",
                
                "tor_vc_weapon_thaxe_black_axe_of_krell",
                
                "tor_empire_weapon_sword_runefang_001", 
                
                "tor_we_weapon_bow_legendary",
                
                "tor_vc_weapon_sword_khopesh_001",
                
                
                "tor_empire_staff_cw_001_combined",
                "tor_empire_staff_cw_002_combined",
                "tor_empire_staff_cw_003_combined",
                "tor_empire_staff_bw_001_combined",
                "tor_empire_staff_bw_002_combined",
                "tor_empire_staff_lw_001_combined",
                "tor_empire_staff_lw_002_combined",
                "tor_empire_staff_lw_003_combined",
            };

        public override void SpawnNewParty(out MobileParty party, Settlement initialTarget)
        {
            PartyTemplateObject template = MBObjectManager.Instance.GetObject<PartyTemplateObject>("ungor_party");
            Clan beastmenClan = Clan.FindFirst(x => x.StringId == "beastmen_clan_1");
            var find = TORCommon.FindSettlementsAroundPosition(Settlement.Position2D, 60, x => !x.IsRaided && !x.IsUnderRaid && x.IsVillage).GetRandomElementInefficiently();
            var raidingParty = RaidingPartyComponent.CreateRaidingParty("beastmen_clan_1_party_" + RaidingPartyCount + 1, Settlement, new TextObject ("{=tor_beastmen_raiders_str}Beastmen Raiders").ToString(), template, beastmenClan, MBRandom.RandomInt(75, 99));
            SetPartyAiAction.GetActionForRaidingSettlement(raidingParty, initialTarget ?? find);
            ((RaidingPartyComponent)raidingParty.PartyComponent).Target = initialTarget ?? find;

            party = raidingParty;
        }
    }

    public class SlaverCampComponent : BaseRaiderSpawnerComponent
    {
        public override string BattleSceneName => "TOR_slaver_bay_001";

        public override List<string> RewardItemIds => 
        new()
        {
            "tor_he_head_helm_phoenix_001",
            "tor_he_head_helm_whitelion_001",
            
            "tor_he_shoulder_cape_phoenix_001",
            "tor_he_shoulder_cape_whitelion_001",
            "tor_he_body_armour_phoenix_001",
            "tor_he_body_armour_whitelion_001",
            
            "tor_he_arm_bracers_phoenix_001",
            "tor_he_leg_boots_phoenix_001",
            
            "tor_he_leg_boots_whitelion_001",
            "tor_he_arm_bracers_whitelion_001",
            
            "tor_he_weapon_2h_axe_whitelion_001",
            
            "tor_he_weapon_halberd_phoenix_001",
            
            "tor_we_weapon_bow_legendary",
                
            "tor_vc_weapon_sword_khopesh_001",
                
                
            "tor_empire_staff_cw_001_combined",
            "tor_empire_staff_cw_002_combined",
            "tor_empire_staff_cw_003_combined",
            "tor_empire_staff_bw_001_combined",
            "tor_empire_staff_bw_002_combined",
            "tor_empire_staff_lw_001_combined",
            "tor_empire_staff_lw_002_combined",
            "tor_empire_staff_lw_003_combined",
            
            
    
        };

        public override void SpawnNewParty(out MobileParty party, Settlement initialTarget)
        {
            PartyTemplateObject template = MBObjectManager.Instance.GetObject<PartyTemplateObject>("druchii_slaver_party");
            Clan clan = Clan.FindFirst(x => x.StringId == "druchii_clan_1");
            var find = TORCommon.FindSettlementsAroundPosition(Settlement.Position2D, 60, x => !x.IsRaided && !x.IsUnderRaid && x.IsVillage).GetRandomElementInefficiently();
            var raidingParty = RaidingPartyComponent.CreateRaidingParty("druchii_clan_1_party_" + RaidingPartyCount + 1, Settlement, new TextObject("{=tor_dark_elf_slavers}Druchii Slavers").ToString(), template, clan, MBRandom.RandomInt(75, 99));
            SetPartyAiAction.GetActionForRaidingSettlement(raidingParty, initialTarget ?? find);
            ((RaidingPartyComponent)raidingParty.PartyComponent).Target = initialTarget ?? find;

            party = raidingParty;
        }
    }
}
