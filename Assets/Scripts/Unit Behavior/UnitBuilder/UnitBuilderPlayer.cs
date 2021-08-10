using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UnitBuilderPlayer<T> : UnitBuilderBase<T>
{
    public RectTransform menuRoot;
    // [System.Serializable]
    public class MenuItem
    {
        public Button menuButton;
        public GameObject prefab;
        public int buildQueueCount = 0;
    }
    public List<MenuItem> virtualMenu = new List<MenuItem>();

    protected float lastClickTime;
    protected float clickDelay = 0.25f;

    protected void InitVirtualMenu(GameObject[] prefabs)
    {
        Button[] menuChildren = menuRoot.GetComponentsInChildren<Button>();
        foreach (var (button, index) in menuChildren.WithIndex())
            virtualMenu.Add(new MenuItem { menuButton = button, prefab = prefabs[index] });
    }

    protected void ProtectDoubleClick()
    {
        if (lastClickTime + clickDelay > Time.unscaledTime)
            return;
        lastClickTime = Time.unscaledTime;
    }

    // Clear listeners for next selected builder
    public void ReleaseButtonListeners()
    {
        foreach (MenuItem virtualMenuItem in virtualMenu)
            virtualMenuItem.menuButton.onClick.RemoveAllListeners();
    }
}
