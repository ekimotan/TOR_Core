using SandBox.Missions.MissionLogics;
using TaleWorlds.CampaignSystem;
using TaleWorlds.Core;
using TaleWorlds.InputSystem;
using TaleWorlds.Library;
using TaleWorlds.MountAndBlade;
using TaleWorlds.MountAndBlade.View.MissionViews;
using TOR_Core.Extensions;
using TOR_Core.GameManagers;
using TOR_Core.Utilities;

namespace TOR_Core.AbilitySystem.Scripts
{
    public class SummonChampionScript : CareerAbilityScript
    {
        private MissionCameraFadeView _cameraView;
        private Agent _champion;
        private bool _championIsActive;
        private bool _isDisabled;
        private bool _isHideOutMission;
        private bool _summoned;
        private GameKey _specialMoveKey;
        private string _summonedChampionId;
        private Vec3 _targetPosition;
        
        protected override void OnInit()
        {
            var effects = GetEffectsToTrigger();

            foreach (var effect in effects)
                if (effect.SummonedTroopId != "none")
                {
                    _summonedChampionId = effect.SummonedTroopId;
                    if (_summonedChampionId.Contains("_plate") && _summonedChampionId.Contains("two_handed")) _summonedChampionId = "tor_vc_harbinger_champion_plate_two_handed";
                    break;
                }

            if (Mission.Current.GetMissionBehavior<HideoutMissionController>() != null) _isHideOutMission = true;

            Mission.Current.OnBeforeAgentRemoved += AgentRemoved;

            _cameraView = Mission.Current.GetMissionBehavior<MissionCameraFadeView>();

            _specialMoveKey = HotKeyManager.GetCategory(nameof(TORGameKeyContext)).GetGameKey("SpecialMove");

            _targetPosition = GameEntity.GlobalPosition;
        }

        private void AgentRemoved(Agent affectedagent, Agent affectoragent, AgentState agentstate, KillingBlow killingblow)
        {
            if (affectedagent == _champion) Stop();
        }

        protected override void OnTick(float dt)
        {
            if (!_summoned) InitialShiftToChampion();

            if (!_casterAgent.IsActive() && !_isDisabled)
            {
                var blow = new Blow();
                blow.OwnerId = _casterAgent.Index;
                _champion.Die(blow);
                _isDisabled = true;
                Stop();
            }

            if (( Input.IsKeyPressed(_specialMoveKey.KeyboardKey.InputKey) ||
                  Input.IsKeyPressed(_specialMoveKey.ControllerKey.InputKey) )
                && Hero.MainHero.HasCareerChoice("DeArcanisKadonKeystone"))
                switchBetweenAgents();
        }

        public override void Stop()
        {
            Mission.Current.OnBeforeAgentRemoved -= AgentRemoved;
            base.Stop();
            if (_championIsActive) ShiftControllerToCaster();
            _casterAgent.RemoveStatusEffect("greater_harbinger_ward_protection");
            //TORCommon.Say("stop");
        }

        private void InitialShiftToChampion()
        {
            var data = TORSummonHelper.GetAgentBuildData(_casterAgent, _summonedChampionId);

            _champion = TORSummonHelper.SpawnAgent(data, _targetPosition);

            _casterAgent.UnsetSpellCasterMode();


            _champion.ApplyStatusEffect("greater_harbinger_debuff", null, 9999f);

            if (_isHideOutMission)
            {
                _champion.Formation = null;
                _casterAgent.Team.PlayerOrderController.SelectAllFormations();
                _casterAgent.Team.PlayerOrderController.SetOrder(OrderType.Charge);
            }

            ShiftControllerToChampion();

            _summoned = true;
        }

        private void ShiftControllerToChampion()
        {
            if (_champion.Health > 0)
            {
                _casterAgent.Controller = Agent.ControllerType.None;
                _champion.Controller = Agent.ControllerType.Player;
                _casterAgent.SetTargetPosition(_casterAgent.Position.AsVec2);

                if (Hero.MainHero.HasCareerChoice("CodexMortificaKeystone")) _casterAgent.ApplyStatusEffect("greater_harbinger_ward_protection", null, 9999f);
                _championIsActive = true;
            }

            _champion.WieldInitialWeapons();

            _cameraView.BeginFadeOutAndIn(0.1f, 0.1f, 0.5f);
        }

        private void ShiftControllerToCaster()
        {
            _casterAgent.ClearTargetFrame();
            if (_isHideOutMission)
            {
                _casterAgent.Team.PlayerOrderController.SelectAllFormations();
                _casterAgent.Team.PlayerOrderController.SetOrder(OrderType.Charge);
            }

            if (_casterAgent.Health > 0)
            {
                _casterAgent.Controller = Agent.ControllerType.Player;
                _champion.Controller = Agent.ControllerType.AI;
                _casterAgent.RemoveStatusEffect("greater_harbinger_ward_protection");
            }

            _cameraView.BeginFadeOutAndIn(0.1f, 0.1f, 0.5f);
            _casterAgent.WieldInitialWeapons();
            //TODO there is an initial weird hit that is done by caster and champion. Have to investigate this. 
            _championIsActive = false;
        }


        private void switchBetweenAgents()
        {
            if (!_championIsActive && _summoned)
                ShiftControllerToChampion();
            else
                ShiftControllerToCaster();
        }
    }
}