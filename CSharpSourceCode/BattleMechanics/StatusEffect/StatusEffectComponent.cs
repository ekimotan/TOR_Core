using System.Collections.Generic;
using TaleWorlds.MountAndBlade;
using TOR_Core.Extensions;
using TaleWorlds.Engine;
using System.Linq;
using TOR_Core.BattleMechanics.DamageSystem;
using TOR_Core.Utilities;
using TaleWorlds.Library;
using TaleWorlds.Core;

namespace TOR_Core.BattleMechanics.StatusEffect
{
    public class StatusEffectComponent : AgentComponent
    {
        private float _updateFrequency = 1;
        private float _deltaSinceLastTick = MBRandom.RandomFloatRanged(0, 0.1f);
        private Dictionary<StatusEffect, EffectData> _currentEffects;
        private EffectAggregate _effectAggregate;

        public StatusEffectComponent(Agent agent) : base(agent)
        {
            _currentEffects = new Dictionary<StatusEffect, EffectData>();
            _effectAggregate = new EffectAggregate();
        }

        public void RunStatusEffect(string id, Agent applierAgent, float duration, bool append)
        {
            if (Agent == null)
                return;

            StatusEffect effect = _currentEffects.Keys.Where(e => e.Template.Id.Equals(id)).FirstOrDefault();
            if (effect != null)
            {
                if (append) effect.CurrentDuration += duration;
            }
            else
            {
                effect = StatusEffectManager.CreateNewStatusEffect(id);
                effect.CurrentDuration = duration;
                AddEffect(effect, applierAgent);
            }
        }

        public override void OnAgentRemoved() => CleanUp();

        public void OnElapsed(float dt)
        {
            foreach (StatusEffect effect in _currentEffects.Keys)
            {
                effect.CurrentDuration--;
                if (effect.CurrentDuration <= 0)
                {
                    RemoveEffect(effect);
                    return;
                }
            }
            CalculateEffectAggregate();
            StatusEffect dotEffect = _currentEffects.Keys.Where(x => x.Template.Type == StatusEffectTemplate.EffectType.DamageOverTime).FirstOrDefault();
            EffectData data = null;
            if (dotEffect != null)
            {
                data = _currentEffects[dotEffect];
            }

            //Temporary method for applying effects from the aggregate. This needs to go to a damage manager/calculator which will use the 
            //aggregated information to determine how much damage to apply to the agent
            if (Agent.IsActive() && Agent != null && !Agent.IsFadingOut())
            {
                if (_effectAggregate.DamageOverTime > 0 && data != null)
                {
                    Agent.ApplyDamage((int)_effectAggregate.DamageOverTime, Agent.Position, data.ApplierAgent, false, false);
                }
                else if (_effectAggregate.HealthOverTime > 0)
                {
                    Agent.Heal((int)_effectAggregate.HealthOverTime);
                }
            }
        }

        private void CalculateEffectAggregate()
        {
            _effectAggregate = new EffectAggregate();
            foreach (var effect in _currentEffects.Keys)
            {
                _effectAggregate.AddEffect(effect);
            }
        }

        public void OnTick(float dt)
        {
            _deltaSinceLastTick += dt;
            if (_deltaSinceLastTick > _updateFrequency)
            {
                _deltaSinceLastTick = MBRandom.RandomFloatRanged(0, 0.1f);
                OnElapsed(dt);
            }
        }

        private void RemoveEffect(StatusEffect effect)
        {
            EffectData data = _currentEffects[effect];

            data.ParticleEntities.ForEach(pe =>
            {
                pe.RemoveAllParticleSystems();
                pe = null;
            });

            _currentEffects.Remove(effect);
        }

        public float[] GetAmplifiers()
        {
            return _effectAggregate.DamageAmplification;
        }

        public float[] GetResistances()
        {
            return _effectAggregate.Resistance;
        }

        public List<string> GetTemporaryAttributes()
        {
            List<string> list = new List<string>();
            foreach(var effect in _currentEffects.Keys)
            {
                foreach(var attribute in effect.Template.TemporaryAttributes)
                {
                    if (!list.Contains(attribute)) list.Add(attribute);
                }
            }
            return list;
        }

        private void AddEffect(StatusEffect effect, Agent applierAgent)
        {
            List<GameEntity> childEntities;
            TORParticleSystem.ApplyParticleToAgent(Agent, effect.Template.ParticleId, out childEntities, effect.Template.ParticleIntensity, effect.Template.ApplyToRootBoneOnly);

            EffectData data = new EffectData(effect, childEntities, applierAgent);
            data.ParticleEntities = childEntities;

            _currentEffects.Add(effect, data);
        }

        private void CleanUp()
        {
            foreach (var item in _currentEffects.ToList())
            {
                RemoveEffect(item.Key);
            }
            _currentEffects.Clear();
            _effectAggregate = null;
        }

        private class EffectData
        {
            public EffectData(StatusEffect effect, List<GameEntity> particleEntities, Agent applierAgent)
            {
                Effect = effect;
                ParticleEntities = particleEntities;
                ApplierAgent = applierAgent;
            }

            public List<GameEntity> ParticleEntities { get; set; }
            public StatusEffect Effect { get; set; }
            public Agent ApplierAgent { get; set; }
        }

        private class EffectAggregate
        {
            public float HealthOverTime { get; set; } = 0;
            public float DamageOverTime { get; set; } = 0;
            public readonly float[] DamageAmplification = new float[(int)DamageType.All + 1];
            public readonly float[] Resistance = new float[(int)DamageType.All + 1];

            public void AddEffect(StatusEffect effect)
            {
                var template = effect.Template;
                switch (template.Type)
                {
                    case StatusEffectTemplate.EffectType.DamageOverTime:
                        DamageOverTime += template.DamageOverTime;
                        break;
                    case StatusEffectTemplate.EffectType.HealthOverTime:
                        HealthOverTime += template.HealthOverTime;
                        break;
                    case StatusEffectTemplate.EffectType.DamageAmplification :
                        DamageAmplification[(int)template.DamageAmplifier.AmplifiedDamageType] = template.DamageAmplifier.DamageAmplifier;
                        break;
                    case StatusEffectTemplate.EffectType.Resistance:
                        Resistance[(int)template.Resistance.ResistedDamageType] = template.Resistance.ReductionPercent;
                        break;
                }
            }
        }
    }
}