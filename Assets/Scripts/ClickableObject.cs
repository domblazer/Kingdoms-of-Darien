using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class ClickableObject : MonoBehaviour, IPointerClickHandler
{
    public delegate void ClickActionCall();

    private ClickActionCall leftClickHandler;
    private ClickActionCall rightClickHandler;
    private ClickActionCall middleClickHandler;

    public void OnPointerClick(PointerEventData eventData)
    {
        if (eventData.button == PointerEventData.InputButton.Left && leftClickHandler != null)
            leftClickHandler();
        else if (eventData.button == PointerEventData.InputButton.Middle && middleClickHandler != null)
            middleClickHandler();
        else if (eventData.button == PointerEventData.InputButton.Right && rightClickHandler != null)
            rightClickHandler();
    }

    public void OnRightClick(ClickActionCall callback)
    {
        rightClickHandler = callback;
    }

    public void OnMiddleClick(ClickActionCall callback)
    {
        middleClickHandler = callback;
    }

    public void OnLeftClick(ClickActionCall callback)
    {
        leftClickHandler = callback;
    }

    public void RemoveAllListeners()
    {
        rightClickHandler = null;
        leftClickHandler = null;
        middleClickHandler = null;
    }
}
