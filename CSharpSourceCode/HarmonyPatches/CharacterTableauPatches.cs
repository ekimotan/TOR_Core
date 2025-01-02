using HarmonyLib;
using TaleWorlds.MountAndBlade.View;
using TaleWorlds.MountAndBlade.View.Tableaus;

namespace TOR_Core.HarmonyPatches;

[HarmonyPatch]
public static class CharacterTableauPatches
{
    [HarmonyPostfix]
    [HarmonyPatch(typeof(CharacterTableau), "SetRace")]
    public static void SetRacePostfix(CharacterTableau __instance)
    {
        var _agentVisuals = AccessTools.Field(typeof(CharacterTableau), "_agentVisuals")?.GetValue(__instance) as AgentVisuals;
        _agentVisuals?.Reset();
        var _oldAgentVisuals = AccessTools.Field(typeof(CharacterTableau), "_oldAgentVisuals")?.GetValue(__instance) as AgentVisuals;
        _oldAgentVisuals?.Reset();
        AccessTools.Method(typeof(CharacterTableau), "InitializeAgentVisuals").Invoke(__instance, new object[] { });
    }

}