using KSP.Game;
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

    public Vector3 RelativePosition { get => new Vector3(150, 60, 10); }

    public Vector3 RelativeVelocity { get => (_orbit.relativeVelocity - _targetOrbit.relativeVelocity).vector; }

    public Vector3 RelativeOrientation { get => new Vector3(1, 1, 1).normalized; }

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

        // Get target's orbit
        _targetOrbit = _currentTarget?.Orbit as PatchedConicsOrbit;
        if (_currentTarget.IsPart)
            _targetOrbit = _currentTarget?.Part.PartOwner.SimulationObject.Orbit as PatchedConicsOrbit;
    }
}
