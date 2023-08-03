/* Docking Alignement Display
 * Copyright (C) 2023  Safarte
 *
 * Use of this source code is governed by an MIT-style
 * license that can be found in the LICENSE file or at
 * https://opensource.org/licenses/MIT.
 */
using KSP.Api;
using KSP.Game;
using KSP.Sim;
using KSP.Sim.impl;
using UnityEngine;

namespace DockingAlignmentDisplay.Geometry;

internal class Target
{
    // Current active vessel
    private VesselComponent _activeVessel;

    // Current target
    private SimulationObjectModel _currentTarget;

    // Active vessel's orbit
    private PatchedConicsOrbit _orbit;

    // Target's orbit
    private PatchedConicsOrbit _targetOrbit;

    // Target's frame of reference
    private ICoordinateSystem _targetFrame;

    // Target's basis
    private Vector3d _tgtUp;
    private Vector3d _tgtFwd;
    private Vector3d _tgtLeft;

    public Vector3 RelativePosition
    {
        get
        {
            // Center of own vessel and of target part
            Position center = _activeVessel.controlTransform.Position;
            Position targetCenter = _currentTarget.transform.Position;

            // Convert to target's frame of reference
            Vector3 localCenter = _targetFrame.ToLocalPosition(center);
            Vector3 localTargetCenter = _targetFrame.ToLocalPosition(targetCenter);

            // Relative position vector
            Vector3 tgtToVessel = localCenter - localTargetCenter;

            // Compute offset
            Vector3 offset = Vector3.ProjectOnPlane(tgtToVessel, _tgtUp);

            // Full relative position vector
            Vector3 relPos = new Vector3(-Vector3.Dot(_tgtFwd, offset), -Vector3.Dot(_tgtLeft, offset), Vector3.Dot(_tgtUp, tgtToVessel));

            return relPos;
        }
    }

    public Vector3 RelativeVelocity
    {
        get
        {
            // Center of own vessel and of target part
            var vel = _activeVessel.Velocity.relativeVelocity;
            var targetVel = _currentTarget.Part.PartOwner.SimulationObject.Vessel.Velocity.relativeVelocity;

            // Convert to target's frame of reference
            Vector3 localVel = _targetFrame.ToLocalVector(vel);
            Vector3 localTargetVel = _targetFrame.ToLocalVector(targetVel);

            // Relative position vector
            Vector3 velDiff = localVel - localTargetVel;

            // Project onto docking port plane
            Vector3 velProj = Vector3.ProjectOnPlane(velDiff, _tgtUp);

            // Relative velocity
            Vector3 relVel = new Vector3(Vector3.Dot(_tgtFwd, velProj), Vector3.Dot(_tgtLeft, velProj), -Vector3.Dot(_tgtUp, velDiff));

            return relVel;
        }
    }

    public Vector3 RelativeOrientation
    {
        get
        {
            // Docking port "up" vector
            var up = _activeVessel.controlTransform.up;

            // Convert to target's frame of reference
            var localUp = _targetFrame.ToLocalVector(up);

            // Project onto docking port plane
            Vector3 upProj = Vector3.ProjectOnPlane(localUp, _tgtUp);

            // Full relative orientation
            Vector3 relOrientation = new Vector3(Vector3.Dot(_tgtFwd, upProj), -Vector3.Dot(_tgtLeft, upProj), Vector3.Dot(_tgtUp, localUp));

            return relOrientation;
        }
    }

    public bool IsValid
    {
        get => _currentTarget != null && _currentTarget.IsPart && _targetOrbit != null && _orbit.referenceBody == _targetOrbit.referenceBody;
    }

    public void Update(GameInstance game)
    {
        // Get active vessel
        _activeVessel = game?.ViewController?.GetActiveVehicle(true)?.GetSimVessel(true);

        // Get own's orbit
        _orbit = _activeVessel?.Orbit;

        // Get current target
        _currentTarget = _activeVessel?.TargetObject;

        if (_currentTarget != null)
        {
            // Get target's orbit
            _targetOrbit = _currentTarget?.Orbit as PatchedConicsOrbit;
            if (_currentTarget.IsPart)
                _targetOrbit = _currentTarget?.Part.PartOwner.SimulationObject.Orbit as PatchedConicsOrbit;

            // Target frame & up
            _targetFrame = _currentTarget.transform.coordinateSystem;
            _tgtUp = _targetFrame.ToLocalVector(_currentTarget.transform.up);
            _tgtFwd = _targetFrame.ToLocalVector(_currentTarget.transform.forward);
            _tgtLeft = _targetFrame.ToLocalVector(_currentTarget.transform.left);
        }
    }
}
