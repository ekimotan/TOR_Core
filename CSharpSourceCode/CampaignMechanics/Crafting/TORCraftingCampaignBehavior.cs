using HarmonyLib;
using Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.CampaignBehaviors;
using TaleWorlds.CampaignSystem.ComponentInterfaces;
using TaleWorlds.CampaignSystem.GameMenus;
using TaleWorlds.CampaignSystem.Overlay;
using TaleWorlds.CampaignSystem.Settlements;
using TaleWorlds.CampaignSystem.ViewModelCollection.GameMenu;
using TaleWorlds.Core;
using TaleWorlds.Localization;
using TaleWorlds.ObjectSystem;
using TOR_Core.Extensions;
using static Ink.Compiler;

namespace TOR_Core.CampaignMechanics.Crafting
{
    public class TORCraftingCampaignBehavior : CraftingCampaignBehavior
    {
        private bool _hasSmithyBeenRemoved;

        public override void RegisterEvents()
        {
            base.RegisterEvents();
            CampaignEvents.OnSessionLaunchedEvent.AddNonSerializedListener(this, OnSessionStart);
            CampaignEvents.TickEvent.AddNonSerializedListener(this, OnTick);
        }

        private void OnTick(float obj)
        {
            if (!_hasSmithyBeenRemoved)
            {
                var menu = Campaign.Current.GameMenuManager.GetGameMenu("town");
                menu?.RemoveGameMenuOption("town_smithy");
                _hasSmithyBeenRemoved = true;

                RearrangeTownMenus(menu);
            }
        }

        private void RearrangeTownMenus(GameMenu menu)
        {
            var options = AccessTools.Field(typeof(GameMenu), "_menuItems").GetValue(menu) as List<GameMenuOption>;
            var backStreet = options.FirstOrDefault(x => x.IdString == "town_backstreet");
            var artisan = options.FirstOrDefault(x => x.IdString == "town_artisan");
            if (backStreet != null && artisan != null)
            {
                int backStreetIndex = -1;
                int artisanIndex = -1;
                List<GameMenuOption> newOptions = new List<GameMenuOption>();
                for (int i = 0; i < options.Count; i++)
                {
                    var option = options[i];
                    if (option == backStreet)
                    {
                        backStreetIndex = i;
                    }
                    else if (option == artisan)
                    {
                        artisanIndex = i;
                        continue;
                    }
                    if (i == backStreetIndex + 1)
                    {
                        newOptions.Add(options[artisanIndex]);
                    }
                    newOptions.Add(option);
                }
                AccessTools.Field(typeof(GameMenu), "_menuItems").SetValue(menu, newOptions);
            }
        }

        private void OnSessionStart(CampaignGameStarter starter)
        {
            AccessTools.Property(typeof(ItemObject), "Name").SetValue(DefaultItems.IronIngot6, new TextObject("{=ironingot6_name}Star Metal{@Plural}loads of star metal{\\@}"));

            AddTownMenu(starter);
        }

        private void AddTownMenu(CampaignGameStarter starter)
        {
            starter.AddGameMenuOption("town", "town_artisan", "Go to the artisan district",
                args => 
                {
                    args.optionLeaveType = GameMenuOption.LeaveType.Submenu;
                    return MenuHelper.SetOptionProperties(args, true, false, TextObject.Empty);
                },
                args =>
                {
                    GameMenu.SwitchToMenu("town_artisan");
                }, 
                false, 4, false, null);

            starter.AddGameMenu("town_artisan", "{ARTISAN_INTRODUCTION}",
                delegate (MenuCallbackArgs args)
                {
                    args.MenuTitle = new TextObject("Artisan District");
                    var intro = new TextObject("{=tor_settlement_artisan_introduction}You have arrived at {SETTLEMENT_NAME}'s Artisan District. It is a lively place where artisans and craftsmen of varying professions are busy in their workshops.");
                    MBTextManager.SetTextVariable("SETTLEMENT_NAME", Settlement.CurrentSettlement.Name);
                    MBTextManager.SetTextVariable("ARTISAN_INTRODUCTION", intro);
                },
                GameOverlays.MenuOverlayType.SettlementWithBoth, GameMenu.MenuFlags.None, null);

            starter.AddGameMenuOption("town_artisan", "town_artisan_smithy", "Visit the weaponsmith",
                game_menu_craft_weapon_on_condition,
                args =>
                {
                    CraftingHelper.OpenCrafting(CraftingTemplate.All[0], null);
                }, 
                false, -1, false, null);

            starter.AddGameMenuOption("town_artisan", "town_artisan_enchanting", "Visit the enchanter",
                game_menu_enchant_weapon_on_condition,
                args =>
                {
                    EnchantingScreen.Open();
                },
                false, -1, false, null);

            starter.AddGameMenuOption("town_artisan", "town_artisan_leave", "Leave",
                args =>
                {
                    args.optionLeaveType = GameMenuOption.LeaveType.Leave;
                    return true;
                },
                args =>
                {
                    GameMenu.SwitchToMenu("town");
                }, 
                true, -1, false, null);
        }

        private static bool game_menu_craft_weapon_on_condition(MenuCallbackArgs args)
        {
            bool canPlayerDo = Campaign.Current.Models.SettlementAccessModel.CanMainHeroDoSettlementAction(Settlement.CurrentSettlement, SettlementAccessModel.SettlementAction.Craft, out bool shouldBeDisabled, out TextObject disabledText);
            args.optionLeaveType = GameMenuOption.LeaveType.Craft;
            ICraftingCampaignBehavior campaignBehavior = Campaign.Current.GetCampaignBehavior<ICraftingCampaignBehavior>();
            if (Settlement.CurrentSettlement.IsTown && 
                campaignBehavior != null && 
                campaignBehavior.CraftingOrders != null && 
                campaignBehavior.CraftingOrders.TryGetValue(Settlement.CurrentSettlement.Town, out CraftingOrderSlots craftingOrderSlots) && 
                craftingOrderSlots.CustomOrders.Count > 0)
            {
                args.OptionQuestData |= GameMenuOption.IssueQuestFlags.ActiveIssue;
            }
            return MenuHelper.SetOptionProperties(args, canPlayerDo, shouldBeDisabled, disabledText);
        }

        private static bool game_menu_enchant_weapon_on_condition(MenuCallbackArgs args)
        {
            bool canPlayerDo = Campaign.Current.Models.SettlementAccessModel.CanMainHeroDoSettlementAction(Settlement.CurrentSettlement, SettlementAccessModel.SettlementAction.Craft, out bool shouldBeDisabled, out TextObject disabledText);
            args.optionLeaveType = GameMenuOption.LeaveType.Craft;
            return MenuHelper.SetOptionProperties(args, canPlayerDo, shouldBeDisabled, disabledText);
        }
    }
}
