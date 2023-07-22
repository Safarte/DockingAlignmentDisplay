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
    private static VisualElement _container;
    public static bool GUIEnabled = true;

    private void Start()
    {
        SetupDocument();
    }

    public void SetEnabled(bool newState)
    {
        if (newState)
        {
            _container.style.display = DisplayStyle.Flex;
        }
        else _container.style.display = DisplayStyle.None;

        GameObject.Find(DockingAlignmentDisplayPlugin._ToolbarFlightButtonID)?.GetComponent<UIValue_WriteBool_Toggle>()?.SetValue(newState);
    }

    private void SetupDocument()
    {
        var document = GetComponent<UIDocument>();
        if (document.TryGetComponent<DocumentLocalization>(out var localization))
        {
            localization.Localize();
        }
        else
        {
            document.EnableLocalization();
        }

        _container = document.rootVisualElement;
        _container[0].CenterByDefault();
        _container.style.display = DisplayStyle.None;
    }
}
