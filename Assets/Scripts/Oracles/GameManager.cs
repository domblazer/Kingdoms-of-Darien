using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DarienEngine;
using DarienEngine.AI;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [System.Serializable]
    public class PlayerConfig
    {
        public PlayerNumbers playerNumber;
        public TeamNumbers team;
        public Factions faction;
        // public FactionColor factionColor;
        public Transform startPosition;
    }

    // List of players to initialize for the game
    public PlayerConfig[] playerConfigs;

    public int unitLimit = 500;
    public bool enableFogOfWar = true;
    public GameObject fogOfWarPlane;
    public Vector2 mapSize = new Vector2(90, 80);
    public Vector2 mapCenter = new Vector2(500, 500);

    public class MapInfo
    {
        public Rect bounds;
        public MapInfo(Vector2 mapSize, Vector2 mapCenter)
        {
            bounds = new Rect(mapCenter.x - mapSize.x / 2, mapCenter.y - mapSize.y / 2, mapSize.x, mapSize.y);
        }

        public bool PointInsideBounds(float x, float y)
        {
            return bounds.Contains(new Vector2(x, y));
        }
    }
    public MapInfo mapInfo { get; set; }

    public GameObject moveCommandSticker;
    public GameObject guardCommandSticker;
    public GameObject patrolCommandSticker;

    public Dictionary<PlayerNumbers, AIPlayerContext> AIPlayers { get; set; } = new Dictionary<PlayerNumbers, AIPlayerContext>();
    public MainPlayerContext PlayerMain { get; set; }
    public AudioSource AudioSource { get; set; }
    private GameObject currentHovering = null;

    private void Awake()
    {
        Instance = this;
        AudioSource = GetComponent<AudioSource>();
        if (!AudioSource)
            Debug.LogWarning("GameManager could not find AudioSource. Some sounds may not play.");

        // @TODO: playerConfigs should be set from skirmish menu. For now, set in inspector
        InitAllPlayers(playerConfigs);

        // Load all the hit sounds from Resources
        SoundHitClasses.LoadSoundHitMap();

        // Set initial fog-of-war plane value. BaseUnitAIs will also use this value on start
        fogOfWarPlane.SetActive(enableFogOfWar);

        // Establish map bounds
        mapInfo = new MapInfo(mapSize, mapCenter);
    }

    private void InitAllPlayers(PlayerConfig[] playerConfigs)
    {
        foreach (PlayerConfig playerConf in playerConfigs)
        {
            if (playerConf.playerNumber == PlayerNumbers.Player1)
                InitMainPlayer();
            else
                InitAIPlayer(playerConf);
        }
    }

    private GameObject InitAIPlayer(PlayerConfig playerConf)
    {
        GameObject _Holder = Functions.GetOrCreatePlayerHolder(playerConf.playerNumber);
        // Add AIPlayer and Inventory scripts to holder object
        AIPlayer newPlayer = _Holder.AddComponent<AIPlayer>();
        InventoryAI newInventory = _Holder.AddComponent<InventoryAI>();
        // Set initial player vars
        newInventory.unitLimit = unitLimit;
        newPlayer.Init(newInventory);
        newPlayer.playerNumber = playerConf.playerNumber;
        newPlayer.teamNumber = playerConf.team;
        newPlayer.playerFaction = playerConf.faction;
        newPlayer.playerStartPosition = playerConf.startPosition;
        // Instantiate new AI player context and add it to AIPlayers dictionary
        AIPlayerContext newAI = new AIPlayerContext
        {
            holder = _Holder,
            player = newPlayer,
            inventory = newInventory
        };
        AIPlayers.Add(playerConf.playerNumber, newAI);
        return _Holder;
    }

    private MainPlayerContext InitMainPlayer()
    {
        // Get the Player1 conf for main (human) player
        PlayerConfig playerConf = Array.Find(playerConfigs, p => p.playerNumber == PlayerNumbers.Player1);
        if (playerConf == null)
            throw new System.Exception("Error: Could not find Player1 in startup config. Cannot start.");

        // Create the game object that represents Player1
        GameObject _Holder = Functions.GetOrCreatePlayerHolder(PlayerNumbers.Player1);
        // Add Inventory and UnitSelection scripts
        Inventory newInventory = _Holder.AddComponent<Inventory>();
        Player newPlayer = _Holder.AddComponent<Player>();
        // Set initial vars
        newPlayer.teamNumber = playerConf.team;
        newPlayer.playerStartPosition = playerConf.startPosition;
        newPlayer.playerFaction = playerConf.faction;
        newInventory.unitLimit = unitLimit;

        // Get the selection box UI image
        RectTransform selectionSquare = GameObject.Find(CanvasConfigs.GetCanvasRoot(newPlayer.playerFaction) + "/selection-box").GetComponent<RectTransform>();
        if (selectionSquare == null)
            Debug.LogError("GameManager Error: Could not load selection square image.");
        // Get audio clip for click sound
        // @TODO: get appropriate click sound per faction
        AudioClip clip = Resources.Load<AudioClip>("runtime/audioclips/TONEARA");
        if (clip == null)
            Debug.LogWarning("Warning: Could not load click sound for Player.");

        // Init the main player
        newPlayer.Init(newInventory, selectionSquare, clip);
        PlayerMain = new MainPlayerContext
        {
            holder = _Holder,
            player = newPlayer,
            inventory = newInventory
        };
        return PlayerMain;
    }

    public void SetHovering(GameObject obj)
    {
        currentHovering = obj;
    }

    public void ClearHovering()
    {
        currentHovering = null;
    }

    public bool IsHovering()
    {
        return currentHovering != null;
    }

    // If hovering over unit not 'compare'
    public bool IsHoveringOther(GameObject compare)
    {
        return currentHovering != null && compare != currentHovering;
    }

    public void PauseGame()
    {
        Time.timeScale = 0;
    }

    public void ResumeGame()
    {
        Time.timeScale = 1;
    }

    public GameObject InstantiateHelper(GameObject obj, Vector3 position)
    {
        return Instantiate(obj, position, obj.transform.rotation);
    }

    public void DestroyHelper(GameObject obj)
    {
        if (obj != null)
            Destroy(obj);
    }

    /* void OnDrawGizmos()
    {
        // Green
        Gizmos.color = new Color(0.0f, 1.0f, 0.0f);
        DrawRect(mapInfo.bounds);
    }

    void OnDrawGizmosSelected()
    {
        // Orange
        Gizmos.color = new Color(1.0f, 0.5f, 0.0f);
        DrawRect(mapInfo.bounds);
    }

    void DrawRect(Rect rect)
    {
        Debug.Log("draw bounds: " + rect);
        Gizmos.DrawWireCube(new Vector3(rect.center.x, 0.01f, rect.center.y), new Vector3(rect.size.x, 0.01f, rect.size.y));
    } */
}
