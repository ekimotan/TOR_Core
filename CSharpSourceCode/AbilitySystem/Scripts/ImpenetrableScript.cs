﻿using TaleWorlds.CampaignSystem;
using TaleWorlds.Core;
using TaleWorlds.Library;
using TaleWorlds.MountAndBlade;
using TOR_Core.BattleMechanics;
using TOR_Core.Extensions;

namespace TOR_Core.AbilitySystem.Scripts;

public class ImpenetrableScript : CareerAbilityScript
{
    
    protected override void OnBeforeTick(float dt)
    {
        var propagate = Hero.MainHero.HasCareerChoice("ShieldWallKeystone");

        if (!propagate) return;
        
        MBList<Agent> list = new MBList<Agent>();

        Mission.Current.GetNearbyAgents(CasterAgent.Position.AsVec2, 8, list);
        

        if (list.IsEmpty())
        {
            return;
        }
                
        SetExplicitTargetAgents(list);

    }

    protected override void OnBeforeRemoved(int removeReason)
    {
        var perkBehavior = Mission.Current.GetMissionBehavior<CareerPerkMissionBehavior>();

        var bonus = 0f;
         bonus =  perkBehavior.CareerMissionVariables[0];

        perkBehavior.CareerMissionVariables[0] = 0;


        if (Hero.MainHero.HasCareerChoice("GromrilArmorKeystone") && Agent.Main != null &&  bonus >0)
        {
            for (int i = 0; i < bonus; i++)
            {
                Agent.Main.ApplyStatusEffect("impenetrable_res_buff",Agent.Main,10,false,false,true);  
            }
        }
        
        if (Hero.MainHero.HasCareerChoice("RuneWeaponsKeystone") && Agent.Main != null &&  bonus >0)
        {
            for (int i = 0; i < bonus; i++)
            {
                Agent.Main.ApplyStatusEffect("impenetrable_dmg_buff",Agent.Main,10,false,false,true);  
            }
        }
    }
}