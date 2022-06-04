using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Class <c>CursorManager</c> handles the cursor sprites and behavior.
/// </summary>
public class CursorManager : MonoBehaviour
{
    public static CursorManager Instance { get; private set; }
    public event EventHandler<OnCursorChangedEventArgs> OnCursorChanged;
    public class OnCursorChangedEventArgs : EventArgs
    {
        public CursorType cursorType;
    }

    [SerializeField] public List<GameCursor> cursors = new List<GameCursor>();
    private GameCursor activeCursor;

    public enum CursorType
    {
        Normal, Move, Select, Attack, CantAttack, Guard, Patrol, Load, Unload, Revive, Clean, Repair, HourGlass
    }

    [System.Serializable]
    public class GameCursor
    {
        public CursorType cursorType;
        public Texture2D[] textureArray;
        public float frameRate = 0.1f;
        public Vector2 offset;
    }

    private int currentFrame;
    private float frameTimer;
    private int frameCount;

    private void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        SetActiveCursorType(CursorType.Normal);
    }

    void Update()
    {
        // If active cursor has a texture animation, run the frames
        if (activeCursor.textureArray.Length > 1)
        {
            frameTimer -= Time.deltaTime;
            if (frameTimer <= 0f)
            {
                frameTimer += activeCursor.frameRate;
                currentFrame = (currentFrame + 1) % frameCount;
                Cursor.SetCursor(activeCursor.textureArray[currentFrame], activeCursor.offset, CursorMode.Auto);
            }
        }
        else
        {
            Cursor.SetCursor(activeCursor.textureArray[0], activeCursor.offset, CursorMode.Auto);
        }
    }

    public void SetActiveCursorType(CursorType cursorType)
    {
        SetActiveCursor(GetCursorByType(cursorType));
        OnCursorChanged?.Invoke(this, new OnCursorChangedEventArgs { cursorType = cursorType });
    }

    public void SetActiveCursorType(string cursorTypeName)
    {
        CursorType cursorType = CursorType.Normal;
        if (cursorTypeName == "Normal") { cursorType = CursorType.Normal; }
        else if (cursorTypeName == "Move") { cursorType = CursorType.Move; }
        else if (cursorTypeName == "Select") { cursorType = CursorType.Select; }
        else if (cursorTypeName == "Attack") { cursorType = CursorType.Attack; }
        else if (cursorTypeName == "CantAttack") { cursorType = CursorType.CantAttack; }
        else if (cursorTypeName == "Guard") { cursorType = CursorType.Guard; }
        else if (cursorTypeName == "Patrol") { cursorType = CursorType.Patrol; }
        else if (cursorTypeName == "Load") { cursorType = CursorType.Load; }
        else if (cursorTypeName == "Unload") { cursorType = CursorType.Unload; }
        else if (cursorTypeName == "Revive") { cursorType = CursorType.Revive; }
        else if (cursorTypeName == "Clean") { cursorType = CursorType.Clean; }
        else if (cursorTypeName == "Repair") { cursorType = CursorType.Repair; }
        else if (cursorTypeName == "HourGlass") { cursorType = CursorType.HourGlass; }
        SetActiveCursor(GetCursorByType(cursorType));
        OnCursorChanged?.Invoke(this, new OnCursorChangedEventArgs { cursorType = cursorType });
        Debug.Log("cursorType changed by string: " + cursorType);
    }

    private GameCursor GetCursorByType(CursorType cursorType)
    {
        foreach (GameCursor cursor in cursors)
        {
            if (cursor.cursorType == cursorType)
            {
                return cursor;
            }
        }
        return null;
    }

    private void SetActiveCursor(GameCursor gameCursor)
    {
        activeCursor = gameCursor;
        currentFrame = 0;
        frameTimer = gameCursor.frameRate;
        frameCount = gameCursor.textureArray.Length;
    }
}
