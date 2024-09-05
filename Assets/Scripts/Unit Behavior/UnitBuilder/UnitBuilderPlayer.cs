using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DarienEngine;

/// <summary>
/// Class <c>UnitBuilderPlayer</c> implements functionality shared amongst playable Builders and Factories.
/// </summary>
public class UnitBuilderPlayer : UnitBuilderBase
{
    private RectTransform menuRoot;
    public List<ConjurerArgs> virtualMenu { get; set; } = new List<ConjurerArgs>();
    public float lastClickTime { get; set; }
    public float clickDelay { get; set; } = 0.25f;
    public bool isCurrentActive { get; set; } = false;

    public void ToggleBuildMenu(bool value)
    {
        menuRoot.gameObject.SetActive(value);
    }

    // Construct a "virtual" menu to represent behavior of menu
    public void InitVirtualMenu(GameObject[] prefabs)
    {
        // If menuRoot is unset on Start(), try to find it in the canvas
        if (!menuRoot)
            menuRoot = Functions.FindBuildMenu(baseUnit);
        if (!menuRoot)
            throw new System.Exception("Builder could not initialize virtual menu. Menu root not found.");
        // Get all the buttons in the build menu
        Button[] menuChildren = menuRoot.GetComponentsInChildren<Button>();
        foreach (var (button, index) in menuChildren.WithIndex())
        {
            ClickableObject clicker = button.gameObject.GetComponent<ClickableObject>();
            if (!clicker)
                clicker = button.gameObject.AddComponent<ClickableObject>();
            virtualMenu.Add(new ConjurerArgs
            {
                menuButton = button,
                prefab = prefabs[index],
                clickHandler = clicker
            });
        }
    }

    // Handle small click delay to prevent double clicks on menu
    public void ProtectDoubleClick()
    {
        if (lastClickTime + clickDelay > Time.unscaledTime)
            return;
        lastClickTime = Time.unscaledTime;
    }

    // Clear listeners for next selected builder
    public void ReleaseButtonListeners()
    {
        foreach (ConjurerArgs virtualMenuItem in virtualMenu)
        {
            virtualMenuItem.clickHandler.RemoveAllListeners();
            // virtualMenuItem.menuButton.onClick.RemoveAllListeners();
        }
    }

    public void UpdateAllButtonsText()
    {
        foreach (ConjurerArgs item in virtualMenu)
            UpdateButtonText(item);
    }

    void UpdateButtonText(ConjurerArgs item)
    {
        string newBtnText = item.buildQueueCount == 0 ? "" : "+" + item.buildQueueCount.ToString();
        item.menuButton.GetComponentInChildren<Text>().text = newBtnText;
    }
}
