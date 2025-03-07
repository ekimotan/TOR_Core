﻿using TaleWorlds.Library;
using TOR_Core.BattleMechanics.AI.CommonAIFunctions;

namespace TOR_Core.AbilitySystem
{
    public class SeekerController
    {
        private Target _target;
        private Vec3 _prevError;
        private SeekerParameters _parameters;
        private bool enabled = true;

        public SeekerController(Target target, SeekerParameters parameters)
        {
            _target = new Target
            {
                Agent = target.Agent,
                Formation = target.Formation
            };
            _prevError = Vec3.Zero;
            _parameters = parameters;
        }

        public MatrixFrame CalculateRotatedFrame(MatrixFrame globalFrame, float dt)
        {
            if (enabled && _target != null && (_target.Agent != null || (_target.Formation != null && _target.Formation.CountOfUnits > 0)))
            {
                var particleDirection = globalFrame.origin + globalFrame.rotation.f.NormalizedCopy();
                var error = _target.GetPosition() - particleDirection;
                if (error.Length < _parameters.DisableDistance)
                {
                    enabled = false;
                    return globalFrame;
                }

                if (error.Length < _parameters.MaxDistance && error.Length > _parameters.MinDistance)
                {
                    var correction = error * _parameters.Proportional + (error - _prevError) * _parameters.Derivative;
                    var newDirection = particleDirection + correction * dt;
                    var globalFrameRotation = Mat3.CreateMat3WithForward(newDirection - globalFrame.origin);

                    globalFrame.rotation = globalFrameRotation;
                }

                _prevError = error;
            }

            return globalFrame;
        }
    }
}