/* Docking Alignement Display
 * Copyright (C) 2023  Safarte
 *
 * Use of this source code is governed by an MIT-style
 * license that can be found in the LICENSE file or at
 * https://opensource.org/licenses/MIT.
 */
using DockingAlignmentDisplay.Geometry;
using KSP.Game;
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

    // Color classes
    string _red = "bg-red";
    string _green = "bg-green";
    string _gold = "bg-gold";

    // Current Target
    Target target = new Target();

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
    VisualElement TangentCrosshairHori;
    VisualElement TangentCrosshairVert;
    VisualElement AngleCrosshair;

    // Rotation marker
    VisualElement RotationMarker;

    // No Target error screen
    VisualElement NoTargetScreen;

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

        // Update target data
        target.Update(Game);

        if (target.IsValid)
        {
            // Hide "No Target" screen
            NoTargetScreen.style.display = DisplayStyle.None;

            UpdateTangentCrosshair(target.RelativePosition, true);
            UpdateMetrics(target.RelativePosition, target.RelativeVelocity, true);
            AngleCrosshair.transform.position = new Vector3(20, 50);
            RotationMarker.transform.position = new Vector3(0, -_screen_height / 2);
        }
        else
        {
            // Show "No Target" screen
            NoTargetScreen.style.display = DisplayStyle.Flex;

            UpdateTangentCrosshair(Vector3.zero, false);
            UpdateMetrics(Vector3.zero, Vector3.zero, false);
            AngleCrosshair.style.display = DisplayStyle.None;
            RotationMarker.style.display = DisplayStyle.None;
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
        TangentCrosshairHori = s_container.Q<VisualElement>("tangent-hori");
        TangentCrosshairVert = s_container.Q<VisualElement>("tangent-vert");
        AngleCrosshair = s_container.Q<VisualElement>("angle-cross");

        // Rotate Angle crosshair
        AngleCrosshair.transform.rotation = Quaternion.AngleAxis(45, Vector3.forward);

        // Rotation marker
        RotationMarker = s_container.Q<VisualElement>("rotation-marker");

        // No Target error screen
        NoTargetScreen = s_container.Q<VisualElement>("no-target");
        NoTargetScreen.style.display = DisplayStyle.None;

        _initialized = true;
    }

    /// <summary>
    /// Converts <c>value</c> to a 4-number string with 1 decimal point and correct unit prefix
    /// </summary>
    /// <param name="value"></param>
    /// <returns></returns>
    private string ToDisplay(double value)
    {
        var exponent = Math.Log10(value);

        List<string> unitPrefixes = new List<string> { "", "k", "M" };
        var prefixIndex = (int)Math.Floor(exponent / 3);
        var prefix = exponent < 0 ? "c" : unitPrefixes[prefixIndex];

        var multiplier = Math.Pow(10, exponent < 0 ? 2 : 3 * prefixIndex);

        return $"{value * multiplier,7:F1} {prefix}";
    }

    /// <summary>
    /// Moves the tangent error crosshair to the specified (<c>x</c>,<c>y</c>) position on the screen.
    /// If the (x,y) error is larger than 100m, draw the crosshair at the edge of the screen.
    /// Set the crosshair's color to red if positionError.z < 0 (the craft is behind the target docking port).
    /// </summary>
    /// <param name="relativePos">Relative position in the parallel frame</param>
    private void UpdateTangentCrosshair(Vector3 relativePos, bool validTarget)
    {
        if (validTarget)
        {
            // Display crosshair
            TangentCrosshairHori.style.display = DisplayStyle.Flex;
            TangentCrosshairVert.style.display = DisplayStyle.Flex;

            // Update color according to course distance (z position error)
            TangentCrosshairHori.EnableInClassList(_green, relativePos.z > 0);
            TangentCrosshairVert.EnableInClassList(_green, relativePos.z > 0);
            TangentCrosshairHori.EnableInClassList(_red, relativePos.z < 0);
            TangentCrosshairVert.EnableInClassList(_red, relativePos.z < 0);

            // Convert xy error to screen coordinates with log scale
            var screenX = Mathf.Sign(relativePos.x) * _screen_width * (Mathf.Log10(Mathf.Clamp(Mathf.Abs(relativePos.x), 0.01f, 90f)) + 2) / 8;
            var screenY = Mathf.Sign(relativePos.y) * _screen_height * (Mathf.Log10(Mathf.Clamp(Mathf.Abs(relativePos.y), 0.01f, 90f)) + 2) / 8;

            // Move crosshair to desired position
            TangentCrosshairHori.transform.position = new Vector3(0, -screenY);
            TangentCrosshairVert.transform.position = new Vector3(screenX, 0);
        }
        else
        {
            // Hide crosshair
            TangentCrosshairHori.style.display = DisplayStyle.None;
            TangentCrosshairVert.style.display = DisplayStyle.None;
        }
    }

    // TODO
    private void UpdateMetrics(Vector3 relativePos, Vector3 relativeVel, bool validTarget)
    {
        if (validTarget)
        {
            // Compute metrics to display
            var cDst = relativePos.z;
            var cVel = relativeVel.z;
            var tOfs = new Vector2(relativePos.x, relativePos.y).magnitude;
            var tVel = new Vector2(relativeVel.x, relativeVel.y).magnitude;

            // Update UI text
            CdstLabel.text = $"CDST:{ToDisplay(cDst)}m";
            CvelLabel.text = $"CVEL:{ToDisplay(cVel)}m/s";
            TofsLabel.text = $"TOFS:{ToDisplay(tOfs)}m";
            TvelLabel.text = $"TVEL:{ToDisplay(tVel)}m/s";
        }
        else
        {
            // Update UI text metrics with "N/A"
            string na = "N/A";
            CdstLabel.text = $"CDST:{na,7}m";
            CvelLabel.text = $"CVEL:{na,7}m/s";
            TofsLabel.text = $"TOFS:{na,7}m";
            TvelLabel.text = $"TVEL:{na,7}m/s";
        }
    }
}
