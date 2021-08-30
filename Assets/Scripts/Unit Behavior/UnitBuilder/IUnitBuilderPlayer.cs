using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MenuItem
{
    public Button menuButton;
    public GameObject prefab;
    public int buildQueueCount = 0;
}

public interface IUnitBuilderPlayer
{
    float lastClickTime { get; set; }
    float clickDelay { get; set; }
    List<MenuItem> virtualMenu { get; set; }

    void QueueBuild(MenuItem item, Vector2 clickPoint);
    void InitVirtualMenu(GameObject[] prefabs);
    void ProtectDoubleClick();
    void ReleaseButtonListeners();
}
