/* Docking Alignement Display
 * Copyright (C) 2023  Safarte
 *
 * Use of this source code is governed by an MIT-style
 * license that can be found in the LICENSE file or at
 * https://opensource.org/licenses/MIT.
 */

using KSP.Game;
using KSP.UI.Binding;
using UitkForKsp2.API;
using UnityEngine;
using UnityEngine.UIElements;

namespace DockingAlignmentDisplay;

/// <summary>
///     Docking Alignment Display UITK GUI controller
/// </summary>
internal class DadUiController : KerbalMonoBehaviour
{
    // Color classes
    private const string Gold = "bg-gold";
    private const string Green = "bg-green";
    private const string Red = "bg-red";

    // GUI Window
    private static VisualElement _container;
    private static bool _initialized;

    // Current Target
    private readonly Target _target = new();

    // Angle crosshair
    private VisualElement _angleCrosshair;
    private VisualElement _angleHori;
    private VisualElement _angleVert;

    // Closing Distance
    private Label _cdstLabel;

    // Close Button
    private Button _closeButton;

    // Closing Velocity
    private Label _cvelLabel;

    // No Target error screen
    private VisualElement _noTargetScreen;

    // Rotation marker
    private VisualElement _rotationMarker;
    private VisualElement _rotationMarkerSprite;


    // Flight Controls Mode
    //Label CtrlLabel;

    // Main display
    private VisualElement _screen;
    private float _screenHeight;
    private float _screenWidth;

    // Tangent crosshair
    private VisualElement _tangentCrosshairHori;
    private VisualElement _tangentCrosshairVert;

    // Tangent Offset
    private Label _tofsLabel;

    // Tangent velocity marker
    private VisualElement _tvelArrow;

    // Tangent Velocity
    private Label _tvelLabel;
    private VisualElement _tvelLine;
    private VisualElement _tvelMarker;

    private bool _uiEnabled;

    private void Start()
    {
        SetupDocument();
    }

    private void Update()
    {
        if (!_initialized) InitElements();

        if (!_uiEnabled || GameManager.Instance.Game is null) return;

        // Close UI on ESC key press
        if (_uiEnabled && Input.GetKey(KeyCode.Escape)) SetEnabled(false);

        // Close UI if not controlling a vessel
        if (Game.ViewController?.GetActiveSimVessel() is null) SetEnabled(false);

        // Update screen dimensions
        _screenWidth = _screen.resolvedStyle.width;
        _screenHeight = _screen.resolvedStyle.height;

        // Update target data
        _target.Update();

        // Update flight controls mode indicator
        //if (Game?.ViewController?.GetActiveSimVessel(true) != null)
        //{
        //    Vehicle.ActiveVesselVehicle.OnFlightControlsModeChange -= OnFlightControlsModeChanged;
        //    Vehicle.ActiveVesselVehicle.OnFlightControlsModeChange += OnFlightControlsModeChanged;
        //}

        if (_target.IsValid)
        {
            // Hide "No Target" screen
            _noTargetScreen.style.display = DisplayStyle.None;

            UpdateTangentCrosshair(_target.RelativePosition);
            UpdateAngleCrosshair(_target.RelativeOrientation);
            UpdateRollIndicator(_target.RelativeRoll);
            UpdateTvelIndicator(_target.RelativeVelocity);
            UpdateMetrics(_target.RelativePosition, _target.RelativeVelocity, true);
        }
        else
        {
            // Show "No Target" screen
            _noTargetScreen.style.display = DisplayStyle.Flex;

            UpdateMetrics(Vector3.zero, Vector3.zero, false);
        }
    }

    public void SetEnabled(bool newState)
    {
        _uiEnabled = newState;
        _container.style.display = newState ? DisplayStyle.Flex : DisplayStyle.None;

        // Update toolbar button
        GameObject.Find(DockingAlignmentDisplayPlugin.ToolbarFlightButtonID)?.GetComponent<UIValue_WriteBool_Toggle>()
            ?.SetValue(newState);
    }

    private void SetupDocument()
    {
        var document = GetComponent<UIDocument>();

        // Set up localization
        if (document.TryGetComponent<DocumentLocalization>(out var localization))
            localization.Localize();
        else
            document.EnableLocalization();

        // root Visual Element
        _container = document.rootVisualElement;

        // Move the GUI to its starting position
        _container[0].transform.position = new Vector2(500, 50);
        _container[0].CenterByDefault();

        // Hide the GUI by default
        _container.style.display = DisplayStyle.None;
    }

    private void InitElements()
    {
        // Close Button
        _closeButton = _container.Q<Button>("close-button");
        _closeButton.clicked += () => SetEnabled(false);

        // Labels
        _cdstLabel = _container.Q<Label>("cdst");
        _cvelLabel = _container.Q<Label>("cvel");
        _tofsLabel = _container.Q<Label>("tofs");
        _tvelLabel = _container.Q<Label>("tvel");
        //CtrlLabel = s_container.Q<Label>("ctrl-mode");

        // Main display
        _screen = _container.Q<VisualElement>("screen");
        _screenWidth = _screen.resolvedStyle.width;
        _screenHeight = _screen.resolvedStyle.height;

        // Error crosshairs
        _tangentCrosshairHori = _container.Q<VisualElement>("tangent-hori");
        _tangentCrosshairVert = _container.Q<VisualElement>("tangent-vert");
        _angleCrosshair = _container.Q<VisualElement>("angle-cross");
        _angleHori = _container.Q<VisualElement>("angle-hori");
        _angleVert = _container.Q<VisualElement>("angle-vert");

        // Rotate Angle crosshair
        _angleCrosshair.transform.rotation = Quaternion.AngleAxis(45, Vector3.forward);

        // Rotation marker
        _rotationMarker = _container.Q<VisualElement>("rotation-marker");
        _rotationMarkerSprite = _container.Q<VisualElement>("rotation-marker-sprite");

        // Tvel indicator
        _tvelMarker = _container.Q<VisualElement>("tvel-marker");
        _tvelLine = _container.Q<VisualElement>("tvel-line");
        _tvelArrow = _container.Q<VisualElement>("tvel-arrow");

        // No Target error screen
        _noTargetScreen = _container.Q<VisualElement>("no-target");
        _noTargetScreen.style.display = DisplayStyle.None;

        _initialized = true;
    }

    //private void OnFlightControlsModeChanged(FlightControlsMode mode)
    //{
    //    CtrlLabel.text = "CTRL: " + (mode == FlightControlsMode.Docking ? "DOCKING" : "NORMAL");
    //}

    /// <summary>
    ///     Converts <c>value</c> to a 4-number string with 1 decimal point and correct unit prefix
    /// </summary>
    /// <param name="value"></param>
    /// <returns></returns>
    private static string ToDisplay(double value)
    {
        var exponent = Math.Log10(Math.Abs(value));

        var unitPrefixes = new List<string> { "", "k", "M" };
        var prefixIndex = (int)Math.Floor(exponent / 3);
        var prefix = exponent < 0 ? "c" : unitPrefixes[prefixIndex];

        var multiplier = Math.Pow(10, exponent < 0 ? 2 : 3 * prefixIndex);

        return $"{value * multiplier,7:F1} {prefix}";
    }

    /// <summary>
    ///     Moves the tangent error crosshair to the specified position on the screen.
    ///     If the (x,y) error is larger than 100m, draw the crosshair at the edge of the screen.
    ///     Set the crosshair's color to red if positionError.z &lt; 0 (the craft is behind the target docking port).
    /// </summary>
    /// <param name="relativePos">Relative position in the target parallel frame</param>
    /// <param name="validTarget">Is the target valid</param>
    private void UpdateTangentCrosshair(Vector3 relativePos)
    {
        // Display crosshair
        _tangentCrosshairHori.style.display = DisplayStyle.Flex;
        _tangentCrosshairVert.style.display = DisplayStyle.Flex;

        // Update color according to course distance (z position error)
        _tangentCrosshairHori.EnableInClassList(Green, relativePos.z > 0);
        _tangentCrosshairVert.EnableInClassList(Green, relativePos.z > 0);
        _tangentCrosshairHori.EnableInClassList(Red, relativePos.z <= 0);
        _tangentCrosshairVert.EnableInClassList(Red, relativePos.z <= 0);

        // Convert xy error to screen coordinates with log scale
        var screenX = Mathf.Sign(relativePos.x) * _screenWidth *
            (Mathf.Log10(Mathf.Clamp(Mathf.Abs(relativePos.x), 0.1f, 990f)) + 1) / 8;
        var screenY = Mathf.Sign(relativePos.y) * _screenHeight *
            (Mathf.Log10(Mathf.Clamp(Mathf.Abs(relativePos.y), 0.1f, 990f)) + 1) / 8;

        // Move crosshair to desired position
        _tangentCrosshairHori.transform.position = new Vector3(0, -screenY);
        _tangentCrosshairVert.transform.position = new Vector3(screenX, 0);
    }

    /// <summary>
    ///     Moves the angle error crosshair to the required position on the screen based on the relative orientation.
    ///     If the angle error is larger than 40°, draw the crosshair at the edge of the screen.
    ///     Set the crosshair's color to red if the docking port is pointed away from the target.
    /// </summary>
    /// <param name="relativeOrientation">Relative orientation (docking port's "up" in the parallel frame)</param>
    /// <param name="validTarget"></param>
    private void UpdateAngleCrosshair(Vector3 relativeOrientation)
    {
        // Display crosshair
        _angleCrosshair.style.display = DisplayStyle.Flex;

        // Adjust crosshair size
        _angleHori.style.width = new StyleLength(0.3f * _screenWidth);
        _angleVert.style.height = new StyleLength(0.3f * _screenHeight);

        // Update color based on if we are pointed toward or away from the target port
        _angleHori.EnableInClassList(Gold, relativeOrientation.z < 0);
        _angleVert.EnableInClassList(Gold, relativeOrientation.z < 0);
        _angleHori.EnableInClassList(Red, relativeOrientation.z >= 0);
        _angleVert.EnableInClassList(Red, relativeOrientation.z >= 0);

        // Error to screen position
        var screenX = Mathf.Clamp(2.25F * Mathf.Asin(-relativeOrientation.x) * 2 / Mathf.PI, -0.99f, 0.99f);
        var screenY = Mathf.Clamp(2.25F * Mathf.Asin(-relativeOrientation.y) * 2 / Mathf.PI, -0.99f, 0.99f);

        // Move crosshair to desired position
        _angleCrosshair.transform.position = new Vector3(_screenWidth * screenX / 2, _screenHeight * screenY / 2);
    }

    /// <summary>
    ///     Moves the rotation indicator arrow based on the relative rotation between the current vessel's docking port
    ///     and the target docking port. The arrow is green is the angle is less that 5° and red otherwise.
    /// </summary>
    /// <param name="relativeRoll">Relative roll between the two docking ports</param>
    /// <param name="validTarget"></param>
    private void UpdateRollIndicator(float relativeRoll)
    {
        // Display indicator
        _rotationMarker.style.display = DisplayStyle.Flex;

        // Color
        var rollDeg = Mathf.Abs(relativeRoll * Mathf.Rad2Deg);
        _rotationMarkerSprite.EnableInClassList("tint-green", rollDeg <= 5);
        _rotationMarkerSprite.EnableInClassList("tint-red", rollDeg > 5);

        // Screen position
        var screenX = 0.98f * Mathf.Sin(relativeRoll) * _screenWidth / 2;
        var screenY = -0.98f * Mathf.Cos(relativeRoll) * _screenHeight / 2;

        // Rotate indicator
        _rotationMarker.transform.rotation = Quaternion.AngleAxis(Mathf.Rad2Deg * relativeRoll, Vector3.forward);

        // Translate indicator
        _rotationMarker.transform.position = new Vector3(screenX, screenY);
    }

    /// <summary>
    ///     Moves the tangent velocity indicator arrow based on the relative velocity between the two crafts in the target's
    ///     docking port parallel frame.
    /// </summary>
    /// <param name="relativeVel">Relative velocity in the target parallel frame</param>
    /// <param name="validTarget"></param>
    private void UpdateTvelIndicator(Vector3 relativeVel)
    {
        // Display indicator
        _tvelMarker.style.display = DisplayStyle.Flex;

        // Tangent velocity vector
        var tVel = new Vector2(relativeVel.x, -relativeVel.y);

        // Magnitude
        var mag = tVel.magnitude;

        // Display indicator if magnitude > 10cm/s
        // TvelMarker.style.display = mag > 0.1f ? DisplayStyle.Flex : DisplayStyle.None;

        // Angle from vertical
        var upVec = new Vector2(0, 1);
        var angle = Vector2.SignedAngle(upVec, tVel);

        // Rotate indicator
        _tvelMarker.transform.rotation = Quaternion.AngleAxis(angle, Vector3.forward);

        // Arrow length
        var screenMag = (Mathf.Log10(Mathf.Clamp(mag, 0.1f, 990f)) + 1) / 8 * (_screenHeight / 2);
        _tvelLine.style.height = screenMag;
        _tvelArrow.style.bottom = screenMag - 5;
    }

    /// <summary>
    ///     Updates the numerical metrics that represent: course distance, course velocity, tangent offset and tangent
    ///     velocity.
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
            _cdstLabel.text = $"CDST:{ToDisplay(cDst)}m";
            _cvelLabel.text = $"CVEL:{ToDisplay(cVel)}m/s";
            _tofsLabel.text = $"TOFS:{ToDisplay(tOfs)}m";
            _tvelLabel.text = $"TVEL:{ToDisplay(tVel)}m/s";
        }
        else
        {
            // Update UI text metrics with "N/A"
            const string na = "N/A ";
            _cdstLabel.text = $"CDST:{na,7}m";
            _cvelLabel.text = $"CVEL:{na,7}m/s";
            _tofsLabel.text = $"TOFS:{na,7}m";
            _tvelLabel.text = $"TVEL:{na,7}m/s";
        }
    }
}