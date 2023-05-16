using System;
using System.Collections.Generic;
using System.Linq;
using TaleWorlds.Engine;
using TaleWorlds.Library;
using TaleWorlds.MountAndBlade;
using TOR_Core.AbilitySystem.Crosshairs;
using TOR_Core.BattleMechanics.AI.Decision;
using TOR_Core.BattleMechanics.TriggeredEffect;

namespace TOR_Core.AbilitySystem.Scripts
{
    public abstract class AbilityScript : ScriptComponentBehavior
    {
        protected Ability _ability;
        private int _soundIndex = -1;
        private SoundEvent _sound;
        protected Agent _casterAgent;
        protected float _abilityLife = -1;
        public bool IsFading { get; protected set; }
        protected float _timeSinceLastTick = 0;
        protected bool _hasCollided;
        private bool _hasTickedOnce;
        protected bool _hasTriggered;
        protected Vec3 _previousFrameOrigin;
        private float _minArmingTimeForCollision = 0.1f;
        private bool _canCollide;
        private SeekerController _controller;
        private bool _soundStarted;
        private Agent _targetAgent = null;

        public void SetTargetSeeking(Target target, SeekerParameters parameters)
        {
            _controller = new SeekerController(target, parameters);
        }
        public virtual void SetAgent(Agent agent)
        {
            _casterAgent = agent;
        }
        public virtual void SetExplicitSingleTarget(Agent agent)
        {
            _targetAgent = agent;
        }
        protected override bool MovesEntity() => true;
        protected virtual bool ShouldMove()
        {
            return _ability.Template.AbilityEffectType == AbilityEffectType.Missile ||
                   _ability.Template.AbilityEffectType == AbilityEffectType.SeekerMissile ||
                   _ability.Template.AbilityEffectType == AbilityEffectType.Vortex ||
                   _ability.Template.AbilityEffectType == AbilityEffectType.Wind;
        }

        private bool IsSingleTarget() => _ability.Template.AbilityTargetType == AbilityTargetType.SingleAlly || _ability.Template.AbilityTargetType == AbilityTargetType.SingleEnemy;

        protected override void OnInit()
        {
            base.OnInit();
            SetScriptComponentToTick(GetTickRequirement());
        }

        public override TickRequirement GetTickRequirement()
        {
            return TickRequirement.Tick;
        }

        public virtual void Initialize(Ability ability)
        {
            _ability = ability;
            if (_ability.Template.SoundEffectToPlay != "none" && _ability.Template.SoundEffectToPlay != null)
            {
                _soundIndex = SoundEvent.GetEventIdFromString(_ability.Template.SoundEffectToPlay);
                _sound = SoundEvent.CreateEvent(_soundIndex, Scene);
            }
        }

        protected override void OnTick(float dt)
        {
            base.OnTick(dt);
            if (_ability == null) return;
            if (IsFading) return;
            _timeSinceLastTick += dt;
            UpdateLifeTime(dt);

            var frame = GameEntity.GetGlobalFrame();
            if (_controller != null) frame = _controller.CalculateRotatedFrame(frame, dt);
            UpdateSound(frame.origin);

            if (_ability.Template.TriggerType == TriggerType.OnCollision && CollidedWithAgent())
            {
                HandleCollision(frame.origin, frame.origin.NormalizedCopy());
            }
            if(_ability.Template.TriggerType == TriggerType.EveryTick && !_hasTriggered)
            {
                TriggerEffects(frame.origin, frame.origin.NormalizedCopy());
                _hasTriggered = true;
            }
            if (_ability.Template.TriggerType == TriggerType.EveryTick && _timeSinceLastTick > _ability.Template.TickInterval)
            {
                _timeSinceLastTick = 0;
                TriggerEffects(frame.origin, frame.origin.NormalizedCopy());
            }
            else if (_ability.Template.TriggerType == TriggerType.TickOnce && _abilityLife > _ability.Template.TickInterval && !_hasTriggered)
            {
                var position = frame.origin;
                var normal = frame.origin.NormalizedCopy();
                if(_ability.Template.AbilityEffectType == AbilityEffectType.Blast)
                {
                    position = frame.Advance(_ability.Template.Offset).origin;
                    normal = frame.rotation.f.NormalizedCopy();
                }
                TriggerEffects(position, normal);
                _hasTriggered = true;
            }
            _hasTickedOnce = true;
            if (ShouldMove())
            {
                UpdatePosition(frame, dt);
            }
        }

        protected virtual void UpdatePosition(MatrixFrame frame, float dt)
        {
            var newframe = GetNextFrame(frame, dt);
            GameEntity.SetGlobalFrame(newframe);
            if (GameEntity.GetBodyShape() != null) GameEntity.GetBodyShape().ManualInvalidate();
        }

        protected virtual MatrixFrame GetNextFrame(MatrixFrame oldFrame, float dt)
        {
            _previousFrameOrigin = oldFrame.origin;
            return oldFrame.Advance(_ability.Template.BaseMovementSpeed * dt);
        }

        protected void UpdateLifeTime(float dt)
        {
            if (_abilityLife < 0) _abilityLife = 0;
            else _abilityLife += dt;
            if (_ability != null)
            {
                if (_abilityLife > _ability.Template.Duration && !IsFading)
                {
                    Stop();
                }
            }
            if (_abilityLife > _minArmingTimeForCollision)
            {
                _canCollide = true;
            }
        }

        protected void UpdateSound(Vec3 position)
        {
            if(_sound != null)
            {
                _sound.SetPosition(position);
                if (IsSoundPlaying()) return;
                else
                {
                    if (!_soundStarted)
                    {
                        _sound.Play();
                        _soundStarted = true;
                    }
                    else if (_ability.Template.ShouldSoundLoopOverDuration)
                    {
                        _sound.Play();
                    }
                    else
                    {
                        _sound.Release();
                        _sound = null;
                    }
                }
            }
        }

        private bool IsSoundPlaying()
        {
            return _sound != null && _sound.IsValid && _sound.IsPlaying();
        }

        protected virtual bool CollidedWithAgent()
        {
            var collisionRadius = _ability.Template.Radius + 1;
            MBList<Agent> agents = new MBList<Agent>();
            agents = Mission.Current.GetNearbyAgents(GameEntity.GetGlobalFrame().origin.AsVec2, collisionRadius, agents);
            return agents.Any(agent => agent != _casterAgent && Math.Abs(GameEntity.GetGlobalFrame().origin.Z - agent.Position.Z) < collisionRadius);
        }

        protected override void OnPhysicsCollision(ref PhysicsContact contact)
        {
            base.OnPhysicsCollision(ref contact);
            if (_ability.Template.TriggerType == TriggerType.OnCollision && _canCollide)
            {
                HandleCollision(contact.ContactPair0.Contact0.Position, contact.ContactPair0.Contact0.Normal);
            }
        }

        protected virtual void HandleCollision(Vec3 position, Vec3 normal)
        {
            if (!_hasTickedOnce) return;
            if (!_hasCollided && position.IsValid && position.IsNonZero)
            {
                GameEntity.FadeOut(0.05f, true);
                IsFading = true;
                TriggerEffects(position, normal);
                _hasCollided = true;
            }
        }

        protected void TriggerEffects(Vec3 position, Vec3 normal)
        {
            var effects = GetEffectsToTrigger();
            foreach(var effect in effects)
            {
                if (effect != null)
                {
                    if (_ability.Template.AbilityTargetType == AbilityTargetType.Self)
                    {
                        effect.Trigger(position, normal, _casterAgent, _ability.Template, new MBList<Agent>(1) { _casterAgent });
                    }
                    else if (IsSingleTarget() && _targetAgent != null)
                    {
                        effect.Trigger(position, normal, _casterAgent, _ability.Template, new MBList<Agent>(1) { _targetAgent });
                    }
                    else effect.Trigger(position, normal, _casterAgent, _ability.Template);
                }
            }
        }

        protected virtual List<TriggeredEffect> GetEffectsToTrigger()
        {
            List<TriggeredEffect> effects = new List<TriggeredEffect>();
            if (_ability == null) return effects; 
            foreach(var effect in _ability.Template.AssociatedTriggeredEffectTemplates)
            {
                effects.Add(new TriggeredEffect(effect));
            }
            return effects;
        }

        protected override void OnRemoved(int removeReason)
        {
            base.OnRemoved(removeReason);
            _sound?.Release();
            _sound = null;
            _ability = null;
            _casterAgent = null;
        }

        public virtual void Stop()
        {
            GameEntity.FadeOut(0.05f, true);
            IsFading = true;
        }
    }
}
