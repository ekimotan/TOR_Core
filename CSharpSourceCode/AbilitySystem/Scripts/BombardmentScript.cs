﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TaleWorlds.Library;
using TaleWorlds.MountAndBlade;
using static TaleWorlds.Engine.GameEntityPhysicsExtensions;
using TOR_Core.Utilities;

namespace TOR_Core.AbilitySystem.Scripts
{
    public class BombardmentScript : AbilityScript
    {
        private bool _impulseGiven;

        protected override void OnAfterTick(float dt)
        {
            if (!_impulseGiven && Ability.Template.TriggerType == TriggerType.OnCollision)
            {
                _impulseGiven = true;
                GameEntity.ApplyLocalImpulseToDynamicBody(GameEntity.CenterOfMass, new Vec3(0, 0, -100));
            }
        }

        protected override void HandleCollision(Vec3 position, Vec3 normal)
        {
            normal.RotateAboutX(90f.ToRadians());
            base.HandleCollision(position, normal);
        }
    }
}
