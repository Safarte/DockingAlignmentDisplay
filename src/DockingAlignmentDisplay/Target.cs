/* Docking Alignement Display
 * Copyright (C) 2023  Safarte
 *
 * Use of this source code is governed by an MIT-style
 * license that can be found in the LICENSE file or at
 * https://opensource.org/licenses/MIT.
 */

using KSP.Api;
using KSP.Sim.impl;
using SpaceWarp.API.Game;
using UnityEngine;

namespace DockingAlignmentDisplay;

internal class Target
{
    // Current active vessel
    private VesselComponent _activeVessel;

    // Current target
    private SimulationObjectModel _currentTarget;

    // Active vessel's orbit
    private PatchedConicsOrbit _orbit;

    // Target's frame of reference
    private ICoordinateSystem _targetFrame;

    // Target's orbit
    private PatchedConicsOrbit _targetOrbit;

    // Target's basis
    private Vector3d _tgtFwd;
    private Vector3d _tgtLeft;
    private Vector3d _tgtUp;

    public Vector3 RelativePosition
    {
        get
        {
            // Center of own vessel and of target part
            var center = _activeVessel.ControlTransform.Position;
            var targetCenter = _currentTarget.transform.Position;

            // Convert to target's frame of reference
            Vector3 localCenter = _targetFrame.ToLocalPosition(center);
            Vector3 localTargetCenter = _targetFrame.ToLocalPosition(targetCenter);

            // Relative position vector
            var tgtToVessel = localTargetCenter - localCenter;

            // Compute offset
            var offset = Vector3.ProjectOnPlane(tgtToVessel, _tgtUp);

            // Use the active vessel orientation as a reference
            var localLeft = _targetFrame.ToLocalVector(_activeVessel.ControlTransform.left);
            var localFwd = _targetFrame.ToLocalVector(_activeVessel.ControlTransform.forward);

            // Full relative position vector
            var relPos = -new Vector3(Vector3.Dot(localLeft, offset), Vector3.Dot(localFwd, offset),
                Vector3.Dot(_tgtUp, tgtToVessel));

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
            var velDiff = localTargetVel - localVel;

            // Project onto docking port plane
            var velProj = Vector3.ProjectOnPlane(velDiff, _tgtUp);

            // Relative velocity
            var relVel = new Vector3(Vector3.Dot(_tgtLeft, velProj), -Vector3.Dot(_tgtFwd, velProj),
                Vector3.Dot(_tgtUp, velDiff));

            return relVel;
        }
    }

    public Vector3 RelativeOrientation
    {
        get
        {
            // Docking port "up" vector
            var up = _activeVessel.ControlTransform.up;

            // Convert to target's frame of reference
            var localUp = _targetFrame.ToLocalVector(up);

            // Project onto docking port plane
            var upProj = Vector3.ProjectOnPlane(localUp, _tgtUp);

            // Full relative orientation
            var relOrientation = new Vector3(-Vector3.Dot(_tgtLeft, upProj), -Vector3.Dot(_tgtFwd, upProj),
                Vector3.Dot(_tgtUp, localUp));

            return relOrientation;
        }
    }

    public float RelativeRoll
    {
        get
        {
            // Docking port "up" vector
            var fwd = _activeVessel.ControlTransform.forward;

            // Convert to target's frame of reference
            var localFwd = _targetFrame.ToLocalVector(fwd).normalized;

            // Relative roll in radians ([-PI, PI])
            var relRoll = -Mathf.Sign(Vector3.Dot(localFwd, _tgtLeft.normalized)) *
                          Mathf.Acos(Vector3.Dot(localFwd, _tgtFwd.normalized));

            return relRoll;
        }
    }

    public bool IsValid => _activeVessel.HasTargetObject && _currentTarget is { IsPart: true } &&
                           _targetOrbit != null &&
                           _orbit.referenceBody == _targetOrbit.referenceBody;

    public void Update()
    {
        _activeVessel = Vehicle.ActiveSimVessel;
        if (_activeVessel is not { HasTargetObject: true }) return;

        // Get own orbit
        _orbit = _activeVessel.Orbit;

        // Get current target
        _currentTarget = _activeVessel.TargetObject;

        // Get target's orbit
        _targetOrbit = _currentTarget.Orbit as PatchedConicsOrbit;
        if (_currentTarget.IsPart)
            _targetOrbit = _currentTarget.Part.PartOwner.SimulationObject.Orbit as PatchedConicsOrbit;

        // Target frame & basis
        _targetFrame = _currentTarget.transform.coordinateSystem;
        _tgtFwd = _targetFrame.ToLocalVector(_currentTarget.transform.forward);
        _tgtLeft = _targetFrame.ToLocalVector(_currentTarget.transform.left);
        _tgtUp = _targetFrame.ToLocalVector(_currentTarget.transform.up);
    }
}