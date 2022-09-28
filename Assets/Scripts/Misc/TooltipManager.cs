using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TooltipManager : MonoBehaviour
{
    public GameObject tooltipPanelPrefab;
    private GameObject tooltip;
    private Text tooltipText;
    public Vector3 tooltipOffset = new Vector3(1.5f, 1.5f, 1.5f);

    void LateUpdate()
    {
        if (tooltip != null && tooltip.activeInHierarchy)
            tooltip.transform.position = Camera.main.WorldToScreenPoint(transform.position + tooltipOffset);
    }

    public void CreateNewTooltip()
    {
        // @TODO: note UIManager.Instance.canvasRoot.transform is sometimes null here b/c it's set in UIManager.Start() - Awake() doesn't work
        tooltip = Instantiate(tooltipPanelPrefab, GameObject.Find("/AraCanvas").transform);
        tooltipText = tooltip.GetComponentInChildren<Text>();
    }

    public void HideTooltip()
    {
        tooltip.SetActive(false);
    }

    public void ShowTooltip()
    {
        tooltip.SetActive(true);
    }

    public void ToggleTooltip()
    {
        if (tooltip.activeInHierarchy)
            HideTooltip();
        else
            ShowTooltip();
    }

    public void SetTooltipText(string text)
    {
        tooltipText.text = text;
    }
}
