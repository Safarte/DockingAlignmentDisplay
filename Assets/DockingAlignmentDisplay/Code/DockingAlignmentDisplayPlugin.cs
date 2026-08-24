/* Docking Alignement Display
 * Copyright (C) 2026  Safarte
 *
 * Use of this source code is governed by an MIT-style
 * license that can be found in the LICENSE file or at
 * https://opensource.org/licenses/MIT.
 */

using System;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Redux.ExtraModTypes;
using ReduxLib.Configuration;
using ReduxLib.Configuration.Attributes;
using SpaceWarp2.UI.API.Appbar;
using UitkForKsp2.API;
using UnityEngine;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.UIElements;
using Scale = DockingAlignmentDisplay.Utils.Scale;

// ReSharper disable once CheckNamespace
namespace DockingAlignmentDisplay
{
    public class DockingAlignmentDisplayPlugin : KerbalMod
    {
        [PublicAPI] public const string ModGuid = "DockingAlignmentDisplay";
        [PublicAPI] public const string ModName = "Docking Alignment Display";

        [PublicAPI] public static DockingAlignmentDisplayPlugin Instance { get; set; }

        public const string ToolbarFlightButtonID = "BTN-DockingAlignmentDisplayFlight";

        [ConfigSection("UI")]
        [ConfigValue(
            "Indicators scaling",
            "The scaling for distance/speed/etc indicators (Linear/Log)"
        )]
        public readonly ConfigValue<Scale> IndicatorsScaling = new ConfigDescription<Scale>();
        
        public override void OnPreInitialized()
        {
            SWConfiguration.Bind(this);
        }

        // ReSharper disable once AsyncVoidMethod
        public override async void OnInitialized()
        {
            Instance = this;

            var dadUxmlHandle = Assets.LoadAssetAsync<VisualTreeAsset>($"Assets/{ModGuid}/UI/{ModGuid}.uxml");
            var appbarIconHandle = Assets.LoadAssetAsync<Texture2D>($"Assets/{ModGuid}/Images/icon.png");

            try
            {
                await Task.WhenAll(dadUxmlHandle.Task, appbarIconHandle.Task);
            }
            catch (Exception e)
            {
                Debug.LogError(
                    $"{ModName}: An exception occurred while loading UI assets."
                );
                Debug.LogException(e);
                return;
            }

            if (dadUxmlHandle.Status != AsyncOperationStatus.Succeeded)
            {
                Debug.LogError(
                    $"{ModName}: Failed to load the window UXML asset. Status: {dadUxmlHandle.Status}. Exception: {dadUxmlHandle.OperationException}"
                );
                return;
            }
            
            var dadUxml = dadUxmlHandle.Result;
            var windowOptions = new WindowOptions
            {
                // The ID of the window. It should be unique to your mod.
                WindowId = "DockingAlignmentDisplay_UI",
                // The transform of parent game object of the window.
                // If null, it will be created under the main canvas.
                Parent = null,
                // Whether the window can be hidden with F2.
                IsHidingEnabled = true,
                // Whether the UI will use the "Scale" option in the settings.
                UseStockScale = true,
                // Whether to disable game input when typing into text fields.
                DisableGameInputForTextFields = true,
                MoveOptions = new MoveOptions
                {
                    // Whether the window can be moved by dragging.
                    IsMovingEnabled = true,
                    // Whether the window can only be moved within the screen bounds.
                    CheckScreenBounds = true
                }
            };
            var dadWindow = Window.Create(windowOptions, dadUxml);
            var dadWindowController = dadWindow.gameObject.AddComponent<DadWindowController>();

            if (appbarIconHandle.Status != AsyncOperationStatus.Succeeded)
            {
                Debug.LogError(
                    $"{ModName}: Failed to load the AppBar icon. Status: {appbarIconHandle.Status}. Exception: {appbarIconHandle.OperationException}"
                );
                return;
            }

            var appbarIcon = appbarIconHandle.Result;
            Appbar.RegisterAppButton(
                "Docking Alignment",
                ToolbarFlightButtonID,
                appbarIcon,
                dadWindowController.SetEnabled
            );
        }

        public override void OnPostInitialized()
        {
            /*
                Code that runs after all mods have been initialized goes here
            */
        }
    }
}