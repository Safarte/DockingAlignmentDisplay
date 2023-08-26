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
    // Flight Controls Mode
    //Label CtrlLabel;

    // Main display
    VisualElement Screen;
    private float _screen_width;
    private float _screen_height;

    // Error crosshairs
    VisualElement TangentCrosshairHori;
    VisualElement TangentCrosshairVert;

    VisualElement AngleCrosshair;
    VisualElement AngleHori;
    VisualElement AngleVert;

    // Rotation marker
    VisualElement RotationMarker;
    VisualElement RotationMarkerSprite;

    // Tangent velocity marker
    VisualElement TvelMarker;
    VisualElement TvelLine;
    VisualElement TvelArrow;

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

        // Close UI if not controlling a vessel
        if (Game?.ViewController?.GetActiveSimVessel(true) is null)
        {
            DockingAlignmentDisplayPlugin.Instance.ToggleButton(false);
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
        target.Update();

        // Update flight controls mode indicator
        //if (Game?.ViewController?.GetActiveSimVessel(true) != null)
        //{
        //    Vehicle.ActiveVesselVehicle.OnFlightControlsModeChange -= OnFlightControlsModeChanged;
        //    Vehicle.ActiveVesselVehicle.OnFlightControlsModeChange += OnFlightControlsModeChanged;
        //}

        if (target.IsValid)
        {
            // Hide "No Target" screen
            NoTargetScreen.style.display = DisplayStyle.None;

            UpdateTangentCrosshair(target.RelativePosition, true);
            UpdateMetrics(target.RelativePosition, target.RelativeVelocity, true);
            UpdateAngleCrosshair(target.RelativeOrientation, true);
            UpdateRollIndicator(target.RelativeRoll, true);
            UpdateTvelIndicator(target.RelativeVelocity, true);
        }
        else
        {
            // Show "No Target" screen
            NoTargetScreen.style.display = DisplayStyle.Flex;

            UpdateTangentCrosshair(Vector3.zero, false);
            UpdateMetrics(Vector3.zero, Vector3.zero, false);
            UpdateAngleCrosshair(Vector3.zero, false);
            UpdateRollIndicator(0, false);
            UpdateTvelIndicator(Vector3.zero, false);
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
        //CtrlLabel = s_container.Q<Label>("ctrl-mode");

        // Main display
        Screen = s_container.Q<VisualElement>("screen");
        _screen_width = Screen.resolvedStyle.width;
        _screen_height = Screen.resolvedStyle.height;

        // Error crosshairs
        TangentCrosshairHori = s_container.Q<VisualElement>("tangent-hori");
        TangentCrosshairVert = s_container.Q<VisualElement>("tangent-vert");
        AngleCrosshair = s_container.Q<VisualElement>("angle-cross");
        AngleHori = s_container.Q<VisualElement>("angle-hori");
        AngleVert = s_container.Q<VisualElement>("angle-vert");

        // Rotate Angle crosshair
        AngleCrosshair.transform.rotation = Quaternion.AngleAxis(45, Vector3.forward);

        // Rotation marker
        RotationMarker = s_container.Q<VisualElement>("rotation-marker");
        RotationMarkerSprite = s_container.Q<VisualElement>("rotation-marker-sprite");

        // Tvel indicator
        TvelMarker = s_container.Q<VisualElement>("tvel-marker");
        TvelLine = s_container.Q<VisualElement>("tvel-line");
        TvelArrow = s_container.Q<VisualElement>("tvel-arrow");

        // No Target error screen
        NoTargetScreen = s_container.Q<VisualElement>("no-target");
        NoTargetScreen.style.display = DisplayStyle.None;

        _initialized = true;
    }

    //private void OnFlightControlsModeChanged(FlightControlsMode mode)
    //{
    //    CtrlLabel.text = "CTRL: " + (mode == FlightControlsMode.Docking ? "DOCKING" : "NORMAL");
    //}

    /// <summary>
    /// Converts <c>value</c> to a 4-number string with 1 decimal point and correct unit prefix
    /// </summary>
    /// <param name="value"></param>
    /// <returns></returns>
    private string ToDisplay(double value)
    {
        var exponent = Math.Log10(Math.Abs(value));

        List<string> unitPrefixes = new List<string> { "", "k", "M" };
        var prefixIndex = (int)Math.Floor(exponent / 3);
        var prefix = exponent < 0 ? "c" : unitPrefixes[prefixIndex];

        var multiplier = Math.Pow(10, exponent < 0 ? 2 : 3 * prefixIndex);

        return $"{value * multiplier,7:F1} {prefix}";
    }

    /// <summary>
    /// Moves the tangent error crosshair to the specified position on the screen.
    /// If the (x,y) error is larger than 100m, draw the crosshair at the edge of the screen.
    /// Set the crosshair's color to red if positionError.z < 0 (the craft is behind the target docking port).
    /// </summary>
    /// <param name="relativePos">Relative position in the target parallel frame</param>
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
            TangentCrosshairHori.EnableInClassList(_red, relativePos.z <= 0);
            TangentCrosshairVert.EnableInClassList(_red, relativePos.z <= 0);

            // Convert xy error to screen coordinates with log scale
            var screenX = Mathf.Sign(relativePos.x) * _screen_width * (Mathf.Log10(Mathf.Clamp(Mathf.Abs(relativePos.x), 0.1f, 990f)) + 1) / 8;
            var screenY = Mathf.Sign(relativePos.y) * _screen_height * (Mathf.Log10(Mathf.Clamp(Mathf.Abs(relativePos.y), 0.1f, 990f)) + 1) / 8;

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

    /// <summary>
    /// Moves the angle error crosshair to the required position on the screen based on the relative orientation.
    /// If the angle error is larger than 40°, draw the crosshair at the edge of the screen.
    /// Set the crosshair's color to red if the docking port is pointed away from the target.
    /// </summary>
    /// <param name="relativeOrientation">Relative orientation (docking port's "up" in the parallel frame)</param>
    /// <param name="validTarget"></param>
    private void UpdateAngleCrosshair(Vector3 relativeOrientation, bool validTarget)
    {
        if (validTarget)
        {
            // Display crosshair
            AngleCrosshair.style.display = DisplayStyle.Flex;

            // Adjust crosshair size
            AngleHori.style.width = new StyleLength(0.3f * _screen_width);
            AngleVert.style.height = new StyleLength(0.3f * _screen_height);

            // Update color based on if we are pointed toward or away from the target port
            AngleHori.EnableInClassList(_gold, relativeOrientation.z < 0);
            AngleVert.EnableInClassList(_gold, relativeOrientation.z < 0);
            AngleHori.EnableInClassList(_red, relativeOrientation.z >= 0);
            AngleVert.EnableInClassList(_red, relativeOrientation.z >= 0);

            // Error to screen position
            var screenX = Mathf.Clamp(90 / 40 * Mathf.Asin(-relativeOrientation.x) * 2 / Mathf.PI, -0.99f, 0.99f);
            var screenY = Mathf.Clamp(90 / 40 * Mathf.Asin(-relativeOrientation.y) * 2 / Mathf.PI, -0.99f, 0.99f);

            // Move crosshair to desired position
            AngleCrosshair.transform.position = new Vector3(_screen_width * screenX / 2, _screen_height * screenY / 2);
        }
        else
        {
            AngleCrosshair.style.display = DisplayStyle.None;
        }
    }

    /// <summary>
    /// Moves the rotation indicator arrow based on the relative rotation between the current vessel's docking port
    /// and the target docking port. The arrow is green is the angle is less that 5° and red otherwise.
    /// </summary>
    /// <param name="relativeRoll">Relative roll between the two docking ports</param>
    /// <param name="validTarget"></param>
    private void UpdateRollIndicator(float relativeRoll, bool validTarget)
    {
        if (validTarget)
        {
            // Display indicator
            RotationMarker.style.display = DisplayStyle.Flex;

            // Color
            var rollDeg = Mathf.Abs(relativeRoll * Mathf.Rad2Deg);
            RotationMarkerSprite.EnableInClassList("tint-green", rollDeg <= 5);
            RotationMarkerSprite.EnableInClassList("tint-red", rollDeg > 5);

            // Screen position
            var screenX = 0.98f * Mathf.Sin(relativeRoll) * _screen_width / 2;
            var screenY = -0.98f * Mathf.Cos(relativeRoll) * _screen_height / 2;

            // Rotate indicator
            RotationMarker.transform.rotation = Quaternion.AngleAxis(Mathf.Rad2Deg * relativeRoll, Vector3.forward);

            // Translate indicator
            RotationMarker.transform.position = new Vector3(screenX, screenY);
        }
        else
        {
            RotationMarker.style.display = DisplayStyle.None;
        }
    }

    /// <summary>
    /// Moves the tangent velocity indicator arrow based on the relative velocity between the two crafts in the target's
    /// docking port parallel frame.
    /// </summary>
    /// <param name="relativeVel">Relative velocity in the target parallel frame</param>
    /// <param name="validTarget"></param>
    private void UpdateTvelIndicator(Vector3 relativeVel, bool validTarget)
    {
        if (validTarget)
        {
            // Display indicator
            TvelMarker.style.display = DisplayStyle.Flex;

            // Tangent velocity vector
            Vector2 tVel = new Vector2(relativeVel.x, -relativeVel.y);

            // Magnitude
            float mag = tVel.magnitude;

            // Display indicator if magnitude > 10cm/s
            // TvelMarker.style.display = mag > 0.1f ? DisplayStyle.Flex : DisplayStyle.None;

            // Angle from vertical
            Vector2 upVec = new Vector2(0, 1);
            float angle = Vector2.SignedAngle(upVec, tVel);

            // Rotate indicator
            TvelMarker.transform.rotation = Quaternion.AngleAxis(angle, Vector3.forward);

            // Arrow length
            float screenMag = (Mathf.Log10(Mathf.Clamp(mag, 0.1f, 990f)) + 1) / 8 * (_screen_height / 2);
            TvelLine.style.height = screenMag;
            TvelArrow.style.bottom = screenMag - 5;
        }
        else
        {
            TvelMarker.style.display = DisplayStyle.None;
        }
    }

    /// <summary>
    /// Updates the numerical metrics that represent: course distance, course velocity, tangent offset and tangent velocity.
    /// </summary>
    /// <param name="relativePos">Relative position in the target parallel frame</param>
    /// <param name="relativeVel">Relative velocity in the target parallel frame</param>
    /// <param name="validTarget"></param>
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
            string na = "N/A ";
            CdstLabel.text = $"CDST:{na,7}m";
            CvelLabel.text = $"CVEL:{na,7}m/s";
            TofsLabel.text = $"TOFS:{na,7}m";
            TvelLabel.text = $"TVEL:{na,7}m/s";
        }
    }
}
