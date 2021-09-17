using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DarienEngine;

public class UnitBuilderPlayer : UnitBuilderBase<PlayerConjurerArgs>
{
    public RectTransform menuRoot;
    public List<PlayerConjurerArgs> virtualMenu { get; set; } = new List<PlayerConjurerArgs>();
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
            virtualMenu.Add(new PlayerConjurerArgs { menuButton = button, prefab = prefabs[index] });
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
        foreach (PlayerConjurerArgs virtualMenuItem in virtualMenu)
            virtualMenuItem.menuButton.onClick.RemoveAllListeners();
    }

    public void UpdateAllButtonsText()
    {
        foreach (PlayerConjurerArgs item in virtualMenu)
            UpdateButtonText(item);
    }

    void UpdateButtonText(PlayerConjurerArgs item)
    {
        string newBtnText = item.buildQueueCount == 0 ? "" : "+" + item.buildQueueCount.ToString();
        item.menuButton.GetComponentInChildren<Text>().text = newBtnText;
    }
}
