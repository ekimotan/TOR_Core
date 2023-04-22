﻿using HarmonyLib;
using SandBox.ViewModelCollection.Nameplate;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Settlements;
using TaleWorlds.CampaignSystem.ViewModelCollection;
using TaleWorlds.Core.ViewModelCollection.Information;
using TaleWorlds.Engine;
using TaleWorlds.Library;
using TaleWorlds.ObjectSystem;
using TOR_Core.CampaignMechanics.RegimentsOfRenown;
using TOR_Core.CampaignMechanics.TORCustomSettlement;
using TOR_Core.CampaignMechanics.TORCustomSettlement.SettlementTypes;
using TOR_Core.Extensions;

namespace TOR_Core.HarmonyPatches
{
    [HarmonyPatch]
    public static class SettlementPatches
    {
        [HarmonyPostfix]
        [HarmonyPatch(typeof(Settlement), "Deserialize")]
        public static void DeserializePostfix(MBObjectManager objectManager, XmlNode node, Settlement __instance)
        {
            if (__instance.SettlementComponent is TORCustomSettlementComponent)
            {
                string clanName = node.Attributes["owner"].Value;
                clanName = clanName.Split('.')[1];
                Clan clan = Clan.FindFirst(x => x.StringId == clanName);
                if (clan != null)
                {
                    var comp = __instance.SettlementComponent as TORCustomSettlementComponent;
                    comp.SetClan(clan);
                }
            }
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(Settlement))]
        [HarmonyPatch("OwnerClan", MethodType.Getter)]
        public static bool OwnerClanPrefix(ref Clan __result, Settlement __instance)
        {
            if (__instance.SettlementComponent is TORCustomSettlementComponent)
            {
                var comp = __instance.SettlementComponent as TORCustomSettlementComponent;
                __result = comp.OwnerClan;
                return false;
            }
            return true;
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(PropertyBasedTooltipVM), "Refresh")]
        public static void AddExtrasToSettlementInfo(PropertyBasedTooltipVM __instance, Type ____shownType, object[] ____typeArgs)
        {
            if(____shownType == typeof(Settlement))
            {
                var settlement = ____typeArgs[0] as Settlement;
                if (settlement.SettlementComponent is TORCustomSettlementComponent)
                {
                    var comp = settlement.SettlementComponent as TORCustomSettlementComponent;
                    if (comp.SettlementType is Shrine)
                    {
                        var shrine = comp.SettlementType as Shrine;
                        if(shrine.Religion != null)
                        {
                            var copy = __instance.TooltipPropertyList.Where(x => !string.IsNullOrWhiteSpace(x.ValueLabel)).ToList();
                            copy.Insert(copy.Count - 1, new TooltipProperty("Affiliation", shrine.Religion.Name.ToString(), 0));
                            __instance.TooltipPropertyList.Clear();
                            foreach (var item in copy) __instance.TooltipPropertyList.Add(item);
                        }
                    }
                }
            }
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(SettlementNameplatesVM), "Initialize")]
        public static bool AddCustomNamePlateVM(SettlementNameplatesVM __instance, IEnumerable<Tuple<Settlement, GameEntity>> settlements, ref IEnumerable<Tuple<Settlement, GameEntity>> ____allHideouts, Camera ____mapCamera, Action<Vec2> ____fastMoveCameraToPosition)
        {
            IEnumerable<Tuple<Settlement, GameEntity>> enumerable = from x in settlements
                                                                    where !x.Item1.IsHideout
                                                                    select x;
            ____allHideouts = from x in settlements
                                where x.Item1.IsHideout
                                select x;

            foreach (Tuple<Settlement, GameEntity> tuple in enumerable)
            {
                ToRSettlementNameplateVM item = new ToRSettlementNameplateVM(tuple.Item1, tuple.Item2, ____mapCamera, ____fastMoveCameraToPosition);
                __instance.Nameplates.Add(item);
            }
            foreach (Tuple<Settlement, GameEntity> tuple2 in ____allHideouts)
            {
                if (tuple2.Item1.Hideout.IsSpotted)
                {
                    ToRSettlementNameplateVM item2 = new ToRSettlementNameplateVM(tuple2.Item1, tuple2.Item2, ____mapCamera, ____fastMoveCameraToPosition);
                    __instance.Nameplates.Add(item2);
                }
            }
            foreach (SettlementNameplateVM settlementNameplateVM in __instance.Nameplates)
            {
                Settlement settlement = settlementNameplateVM.Settlement;
                if (((settlement != null) ? settlement.SiegeEvent : null) != null)
                {
                    SettlementNameplateVM settlementNameplateVM2 = settlementNameplateVM;
                    Settlement settlement2 = settlementNameplateVM.Settlement;
                    settlementNameplateVM2.OnSiegeEventStartedOnSettlement((settlement2 != null) ? settlement2.SiegeEvent : null);
                }
                else if (settlementNameplateVM.Settlement.IsTown || settlementNameplateVM.Settlement.IsCastle)
                {
                    Clan ownerClan = settlementNameplateVM.Settlement.OwnerClan;
                    if (ownerClan != null && ownerClan.IsRebelClan)
                    {
                        settlementNameplateVM.OnRebelliousClanFormed(settlementNameplateVM.Settlement.OwnerClan);
                    }
                }
            }
            RefreshRelationsOfNameplates(__instance.Nameplates);

            return false;
        }

        private static void RefreshRelationsOfNameplates(MBBindingList<SettlementNameplateVM> namePlates)
        {
            foreach (SettlementNameplateVM settlementNameplateVM in namePlates)
            {
                settlementNameplateVM.RefreshRelationStatus();
            }
        }
    }
}
