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
    }

    // List of players to initialize for the game
    public PlayerConfig[] playerConfigs;

    public bool enableFogOfWar = true;
    public GameObject fogOfWarPlane;

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

        // Set initial fog-of-war plane value. BaseUnitAIs will also use this value on start
        fogOfWarPlane.SetActive(enableFogOfWar);
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
        newPlayer.Init(newInventory);
        newPlayer.playerNumber = playerConf.playerNumber;
        newPlayer.teamNumber = playerConf.team;
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
        GameObject _Holder = Functions.GetOrCreatePlayerHolder(PlayerNumbers.Player1);
        // Add Inventory and UnitSelection scripts to Main Player
        Inventory newInventory = _Holder.AddComponent<Inventory>();
        Player newPlayer = _Holder.AddComponent<Player>();
        // @TODO: player team number in theory should always come from the first item in playerConfigs
        PlayerConfig playerConf = Array.Find(playerConfigs, p => p.playerNumber == PlayerNumbers.Player1);
        if (playerConf == null)
            throw new System.Exception("Error: Could not find Player1 in startup config. Cannot start.");

        newPlayer.teamNumber = playerConf.team;
        // @TODO: faction based path, e.g. faction == 'Taros' ? 'TaroCanvas' : faction == 'Aramon' ? 'AraCanvas'
        RectTransform square = GameObject.Find("AraCanvas/selection-box").GetComponent<RectTransform>();
        // Get audio clip for click sound
        AudioClip clip = Resources.Load<AudioClip>("runtime/audioclips/ara-click-01");
        if (clip == null)
            Debug.LogWarning("Warning: Could not load click sound for Player.");
        newPlayer.Init(newInventory, square, clip);

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

    public void PauseGame()
    {
        Time.timeScale = 0;
    }

    public void ResumeGame()
    {
        Time.timeScale = 1;
    }
}
