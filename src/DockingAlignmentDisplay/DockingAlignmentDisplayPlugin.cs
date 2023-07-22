/* Docking Alignement Display
 * Copyright (C) 2023  Safarte
 *
 * This program is free software: you can redistribute it and/or modify
 * it under the terms of the GNU General Public License as published by
 * the Free Software Foundation, either version 3 of the License, or
 * (at your option) any later version.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU General Public License for more details.
 *
 * You should have received a copy of the GNU General Public License
 * along with this program.  If not, see <https://www.gnu.org/licenses/>.
 */
using BepInEx;
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

    public static bool InterfaceEnabled = false;
    public const string _ToolbarFlightButtonID = "BTN-DockingAlignmentDisplayFlight";

    DadUiController uiController;

    // Singleton instance of the plugin class
    public static DockingAlignmentDisplayPlugin Instance { get; set; }

    /// <summary>
    /// Runs when the mod is first initialized.
    /// </summary>
    public override void OnInitialized()
    {
        base.OnInitialized();

        // Load UITK GUI
        var dadUxml = AssetManager.GetAsset<VisualTreeAsset>($"{Info.Metadata.GUID}/dad_ui/dockingalignmentdisplay.uxml");
        var dadWindow = Window.CreateFromUxml(dadUxml, "Docking Alignment Display Main Window", transform, true);
        uiController = dadWindow.gameObject.AddComponent<DadUiController>();

        Appbar.RegisterAppButton("Docking Alignment Display", _ToolbarFlightButtonID, AssetManager.GetAsset<Texture2D>($"{Info.Metadata.GUID}/images/icon.png"),
        ToggleButton);

        Instance = this;
    }

    public void ToggleButton(bool toggle)
    {
        InterfaceEnabled = toggle;
        GameObject.Find(_ToolbarFlightButtonID)?.GetComponent<UIValue_WriteBool_Toggle>()?.SetValue(InterfaceEnabled);
        uiController.SetEnabled(toggle);
    }
}
