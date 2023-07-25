/* Docking Alignement Display
 * Copyright (C) 2023  Safarte
 *
 * Use of this source code is governed by an MIT-style
 * license that can be found in the LICENSE file or at
 * https://opensource.org/licenses/MIT.
 */
using KSP.Game;
using KSP.Sim.impl;
using KSP.UI.Binding;
using UitkForKsp2.API;
using UnityEngine;
using UnityEngine.UIElements;

namespace DockingAlignmentDisplay;

/// <summary>
/// Docking Alignment Display UITK GUI controller
/// </summary>
internal class DadUiController : KerbalMonoBehaviour
{
    // GUI Window
    private static VisualElement s_container;
    private bool _initialized = false;
    public static bool GUIEnabled = true;

    // Active Vessel
    VesselComponent _activeVessel;

    // Target
    SimulationObjectModel _currentTarget;

    // Close Button
    Button CloseButton;

    // Closing Distance
    Label CdstLabel;
    // Closing Velocity
    Label CvelLabel;
    // Tangent Offset
    Label TofsLabel;
    // Tangent Velocity
    Label TvelLabel;

    // Main display
    VisualElement Screen;
    private float _screen_width;
    private float _screen_height;

    // Error crosshairs
    VisualElement TangentCrosshairVert;
    VisualElement TangentCrosshairHori;
    VisualElement AngleCrosshair;

    // Rotation marker
    VisualElement RotationMarker;

    private void Start()
    {
        SetupDocument();
    }

    private void Update()
    {
        if (!_initialized)
        {
            InitElements();
            return;
        }

        // Get active vessel & current target
        _activeVessel = Game?.ViewController?.GetActiveVehicle(true)?.GetSimVessel(true);
        _currentTarget = _activeVessel?.TargetObject;

        // Update GUI display status
        if (GUIEnabled && DockingAlignmentDisplayPlugin.InterfaceEnabled)
            s_container.style.display = DisplayStyle.Flex;
        else
        {
            s_container.style.display = DisplayStyle.None;
            return;
        }

        // Update screen dimensions
        _screen_width = Screen.resolvedStyle.width;
        _screen_height = Screen.resolvedStyle.height;

        // Update RDIST & RVEL
        PatchedConicsOrbit targetOrbit = _currentTarget?.Orbit as PatchedConicsOrbit;

        if (_currentTarget != null)
        {
            if (_currentTarget.IsPart)
            {
                targetOrbit = _currentTarget?.Part.PartOwner.SimulationObject.Orbit as PatchedConicsOrbit;
            }

            if (targetOrbit != null)
            {
                var relDist = (_activeVessel.Orbit.Position - targetOrbit.Position).magnitude;
                var relSpeed = (_activeVessel.Orbit.relativeVelocity - targetOrbit.relativeVelocity).magnitude;

                if (_activeVessel.Orbit.referenceBody == targetOrbit.referenceBody)
                {
                    CdstLabel.text = $"CDST:{toDisplay(relDist)}m";
                    CvelLabel.text = $"CVEL:{toDisplay(relSpeed)}m/s";

                    TangentCrosshairVert.transform.position = new Vector3(-20, 0);
                    TangentCrosshairHori.transform.position = new Vector3(0, -20);
                    AngleCrosshair.transform.position = new Vector3(20, 50);

                    RotationMarker.transform.position = new Vector3(0, -_screen_height / 2);
                }
            }
        }
    }

    public void SetEnabled(bool newState)
    {
        if (newState)
        {
            s_container.style.display = DisplayStyle.Flex;
        }
        else s_container.style.display = DisplayStyle.None;

        // Update toolbar button
        GameObject.Find(DockingAlignmentDisplayPlugin.ToolbarFlightButtonID)?.GetComponent<UIValue_WriteBool_Toggle>()?.SetValue(newState);
    }

    private void SetupDocument()
    {
        var document = GetComponent<UIDocument>();

        // Set up localization
        if (document.TryGetComponent<DocumentLocalization>(out var localization))
        {
            localization.Localize();
        }
        else
        {
            document.EnableLocalization();
        }

        // root Visual Element
        s_container = document.rootVisualElement;

        // Move the GUI to its starting position
        s_container[0].transform.position = new Vector2(500, 50);
        s_container[0].CenterByDefault();

        // Hide the GUI by default
        s_container.style.display = DisplayStyle.None;
    }

    private void InitElements()
    {
        // Close Button
        CloseButton = s_container.Q<Button>("close-button");
        CloseButton.clicked += () => DockingAlignmentDisplayPlugin.Instance.ToggleButton(false);

        // Labels
        CdstLabel = s_container.Q<Label>("cdst");
        CvelLabel = s_container.Q<Label>("cvel");
        TofsLabel = s_container.Q<Label>("tofs");
        TvelLabel = s_container.Q<Label>("tvel");

        // Main display
        Screen = s_container.Q<VisualElement>("screen");
        _screen_width = Screen.resolvedStyle.width;
        _screen_height = Screen.resolvedStyle.height;

        // Error crosshairs
        TangentCrosshairVert = s_container.Q<VisualElement>("tangent-vert");
        TangentCrosshairHori = s_container.Q<VisualElement>("tangent-hori");
        AngleCrosshair = s_container.Q<VisualElement>("angle-cross");

        // Rotate Angle crosshair
        AngleCrosshair.transform.rotation = Quaternion.AngleAxis(45, Vector3.forward);

        // Rotation marker
        RotationMarker = s_container.Q<VisualElement>("rotation-marker");

        _initialized = true;
    }

    private string toDisplay(double value)
    {
        var exponent = Math.Log10(value);

        List<string> unitPrefixes = new List<string> { "", "k", "M" };
        var prefixIndex = (int)Math.Floor(exponent / 3);
        var prefix = exponent < 0 ? "c" : unitPrefixes[prefixIndex];

        var multiplier = Math.Pow(10, exponent < 0 ? 2 : 3 * prefixIndex);

        return $"{value * multiplier,7:F1} {prefix}";
    }
}
