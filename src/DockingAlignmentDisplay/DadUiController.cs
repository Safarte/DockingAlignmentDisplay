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

    // Relative Distance & Velocity Labels
    Label RDstLabel;
    Label RVelLabel;

    // Error crosshairs
    VisualElement TangentCrosshair;
    VisualElement AngleCrosshair;

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
                    RDstLabel.text = $"RDST: {toDisplay(relDist)}m";
                    RVelLabel.text = $"RVEL: {toDisplay(relSpeed)}m/s";

                    TangentCrosshair.transform.position = new Vector3(-20, -20, 0);
                    AngleCrosshair.transform.position = new Vector3(20, 10, 0);
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

        // RDist & RVel Labels
        RDstLabel = s_container.Q<Label>("rdst");
        RVelLabel = s_container.Q<Label>("rvel");

        // Error crosshairs
        TangentCrosshair = s_container.Q<VisualElement>("tangent-cross");
        AngleCrosshair = s_container.Q<VisualElement>("angle-cross");

        // Rotate Angle crosshair
        AngleCrosshair.transform.rotation = Quaternion.AngleAxis(45, Vector3.forward);

        _initialized = true;
    }

    private string toDisplay(double value)
    {
        return $"{value,5:F1} ";
    }
}
