/* Docking Alignement Display
 * Copyright (C) 2023  Safarte
 *
 * Use of this source code is governed by an MIT-style
 * license that can be found in the LICENSE file or at
 * https://opensource.org/licenses/MIT.
 */
using BepInEx;
using BepInEx.Logging;
using JetBrains.Annotations;
using KSP.UI.Binding;
using SpaceWarp;
using SpaceWarp.API.Assets;
using SpaceWarp.API.Mods;
using SpaceWarp.API.UI.Appbar;
using UitkForKsp2;
using UitkForKsp2.API;
using UnityEngine;
using UnityEngine.UIElements;

namespace DockingAlignmentDisplay;

[BepInPlugin(MyPluginInfo.PLUGIN_GUID, MyPluginInfo.PLUGIN_NAME, MyPluginInfo.PLUGIN_VERSION)]
[BepInDependency(SpaceWarpPlugin.ModGuid, SpaceWarpPlugin.ModVer)]
[BepInDependency(UitkForKsp2Plugin.ModGuid, UitkForKsp2Plugin.ModVer)]
public class DockingAlignmentDisplayPlugin : BaseSpaceWarpPlugin
{
    // These are useful in case some other mod wants to add a dependency to this one
    [PublicAPI] public const string ModGuid = MyPluginInfo.PLUGIN_GUID;
    [PublicAPI] public const string ModName = MyPluginInfo.PLUGIN_NAME;
    [PublicAPI] public const string ModVer = MyPluginInfo.PLUGIN_VERSION;

    // AppBar button related stuff
    public static bool InterfaceEnabled = false;
    public const string ToolbarFlightButtonID = "BTN-DockingAlignmentDisplayFlight";

    // UI controller
    DadUiController uiController;

    // Singleton instance of the plugin class
    public static DockingAlignmentDisplayPlugin Instance { get; set; }

    // Logger
    public new static ManualLogSource Logger { get; set; }

    /// <summary>
    /// Runs when the mod is first initialized.
    /// </summary>
    public override void OnInitialized()
    {
        base.OnInitialized();

        // Logger
        Logger = base.Logger;

        // Load UITK GUI
        var dadUxml = AssetManager.GetAsset<VisualTreeAsset>($"{Info.Metadata.GUID}/dad_ui/dockingalignmentdisplay.uxml");
        var dadWindow = Window.CreateFromUxml(dadUxml, "Docking Alignment Display Main Window", transform, true);
        uiController = dadWindow.gameObject.AddComponent<DadUiController>();

        // Add AppBar button
        Appbar.RegisterAppButton(
            "Docking Alignment",
            ToolbarFlightButtonID,
            AssetManager.GetAsset<Texture2D>($"{Info.Metadata.GUID}/images/icon.png"),
            ToggleButton
        );

        Instance = this;
    }

    /// <summary>
    /// Callback for the mod's app bar button
    /// </summary>
    /// <param name="toggle"></param>
    public void ToggleButton(bool toggle)
    {
        InterfaceEnabled = toggle;
        GameObject.Find(ToolbarFlightButtonID)?.GetComponent<UIValue_WriteBool_Toggle>()?.SetValue(InterfaceEnabled);
        uiController.SetEnabled(toggle);
    }
}
