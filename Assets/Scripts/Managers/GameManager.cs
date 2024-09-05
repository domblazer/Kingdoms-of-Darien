using System;
using System.Collections;
using System.Collections.Generic;
using DarienEngine;
using DarienEngine.AI;
using UnityEngine;

/// <summary>
/// Class <c>GameManager</c> initializes and tracks the virtual Player structures, provides global helper functions and properties of the game
/// such as map bounds, manages the pause/resume state of the game, and provides the AudioSource used for system sounds.
/// </summary>
public class GameManager : MonoBehaviour
{
    // Static instance of the GameManager to provide static scope
    public static GameManager Instance { get; private set; }

    [Serializable]
    // Represents the setup for a player
    public class PlayerConfig
    {
        public PlayerNumbers playerNumber;
        public TeamNumbers team;
        public Factions faction;
        // public FactionColor factionColor;
        public Transform startPosition;
    }

    [Tooltip("List of players in the match.")]
    public PlayerConfig[] playerConfigs;

    [Tooltip("Global unit build limit.")]
    public int unitLimit = 500;

    [Tooltip("Enable or disable to Fog of War.")]
    public bool enableFogOfWar = true;

    [Tooltip("Scene object, FogOfWarPlane. Required if enableFogOfWar is checked.")]
    public GameObject fogOfWarPlane;

    [Tooltip("Length and width of the map relative to the center point.")]
    public Vector2 mapSize = new Vector2(90, 80);

    [Tooltip("Center point of the map plane.")]
    public Vector2 mapCenter = new Vector2(500, 500);

    // Wrapper for map information
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

    [Tooltip("Command sticker prefab for the Move command.")]
    public GameObject moveCommandSticker;

    [Tooltip("Command sticker prefab for the Guard command.")]
    public GameObject guardCommandSticker;

    [Tooltip("Command sticker prefab for the Patrol command.")]
    public GameObject patrolCommandSticker;

    // Player context vars
    public MainPlayerContext PlayerMain { get; set; }
    public Dictionary<PlayerNumbers, AIPlayerContext> AIPlayers { get; set; } = new Dictionary<PlayerNumbers, AIPlayerContext>();

    public AudioSource AudioSource { get; set; }

    // Private helper keeping track if player mouse is hovering over any unit
    public GameObject currentHovering = null;

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

    // Setup all Players
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

    // Creates a virtual Player structure for an AI Player
    private GameObject InitAIPlayer(PlayerConfig playerConf)
    {
        // Add AIPlayer and Inventory scripts to holder object
        GameObject _Holder = Functions.GetOrCreatePlayerHolder(playerConf.playerNumber);
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

    // Initializes the human Player, a virtual structure that manages human Player behavior
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

    // Set the unit the mouse is currently hovering over
    public void SetHovering(GameObject obj)
    {
        // @TODO: currentHovering can be an intangible
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

    // Helper to provide Instantiate function to non-MonoBehavior classes
    public GameObject InstantiateHelper(GameObject obj, Vector3 position)
    {
        return Instantiate(obj, position, obj.transform.rotation);
    }

    // Helper to provide Destroy function to non-MonoBehavior classes
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