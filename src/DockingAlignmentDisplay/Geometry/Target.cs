/* Docking Alignement Display
 * Copyright (C) 2023  Safarte
 *
 * Use of this source code is governed by an MIT-style
 * license that can be found in the LICENSE file or at
 * https://opensource.org/licenses/MIT.
 */
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

    public Vector3 RelativePosition
    {
        get
        {
            // Target's frame of reference
            var targetFrame = _currentTarget.transform.coordinateSystem;

            // Center of own vessel and of target part
            Position center = _activeVessel.controlTransform.Position;
            Position targetCenter = _currentTarget.transform.Position;

            // Convert to target's frame of reference
            Vector3 localCenter = targetFrame.ToLocalPosition(center);
            Vector3 localTargetCenter = targetFrame.ToLocalPosition(targetCenter);

            // Relative position vector
            Vector3 error = localCenter - localTargetCenter;

            // Basis change
            error = new Vector3(-error.z, error.x, error.y);

            return error;
        }
    }

    public Vector3 RelativeVelocity
    {
        get
        {
            // Target's frame of reference
            var targetFrame = _currentTarget.transform.coordinateSystem;

            // Center of own vessel and of target part
            var vel = _activeVessel.Velocity.relativeVelocity;
            var targetVel = _currentTarget.Part.PartOwner.SimulationObject.Vessel.Velocity.relativeVelocity;

            // Convert to target's frame of reference
            Vector3 localVel = targetFrame.ToLocalVector(vel);
            Vector3 localTargetVel = targetFrame.ToLocalVector(targetVel);

            // Relative position vector
            Vector3 error = localVel - localTargetVel;

            // Basis change
            error = new Vector3(-error.z, error.x, -error.y);

            return error;
        }
    }

    public Vector3 RelativeOrientation
    {
        get
        {
            // Target's frame of reference
            var targetFrame = _currentTarget.transform.coordinateSystem;

            // Docking port "up" vector
            var up = _activeVessel.controlTransform.up;

            // Convert to target's frame of reference
            var localUp = targetFrame.ToLocalVector(up);

            return localUp;
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
        }
    }
}
