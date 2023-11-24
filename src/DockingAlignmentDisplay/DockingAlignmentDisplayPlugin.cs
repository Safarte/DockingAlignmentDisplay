/* Docking Alignement Display
 * Copyright (C) 2023  Safarte
 *
 * Use of this source code is governed by an MIT-style
 * license that can be found in the LICENSE file or at
 * https://opensource.org/licenses/MIT.
 */

using BepInEx;
using BepInEx.Configuration;
using JetBrains.Annotations;
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
    public const string ToolbarFlightButtonID = "BTN-DockingAlignmentDisplayFlight";

    // AppBar button related stuff
    public static bool InterfaceEnabled;

    // UI controller
    private DadUiController _uiController;

    // Config
    internal ConfigEntry<string> DockingTangentOffsetScale;

    // Singleton instance of the plugin class
    public static DockingAlignmentDisplayPlugin Instance { get; private set; }

    /// <summary>
    ///     Runs when the mod is first initialized.
    /// </summary>
    public override void OnInitialized()
    {
        base.OnInitialized();
        Instance = this;

        // Configuration
        DockingTangentOffsetScale = Config.Bind("Docking Alignment Display", "Docking Tangent Scale", "Linear",
            new ConfigDescription("The scaling of the docking tangent offset & velocity indicator crosshair",
                new AcceptableValueList<string>("Linear", "Log")));

        // Load UITK GUI
        var dadUxml =
            AssetManager.GetAsset<VisualTreeAsset>($"{Info.Metadata.GUID}/dad_ui/dockingalignmentdisplay.uxml");
        var dadWindow = Window.CreateFromUxml(dadUxml, "Docking Alignment Display Main Window", transform, true);
        _uiController = dadWindow.gameObject.AddComponent<DadUiController>();

        // Add AppBar button
        Appbar.RegisterAppButton(
            "Docking Alignment",
            ToolbarFlightButtonID,
            AssetManager.GetAsset<Texture2D>($"{Info.Metadata.GUID}/images/icon.png"),
            _uiController.SetEnabled
        );

        Instance = this;
    }
}