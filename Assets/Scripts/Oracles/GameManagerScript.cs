using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DarienEngine;

public class GameManagerScript : MonoBehaviour
{
    public static GameManagerScript Instance { get; private set; }

    public class AIPlayerContext
    {
        public GameObject holder;
        public AIPlayer playerScript;
        public InventoryAI inventory;
        public TeamNumbers team;
    }

    public class MainPlayerContext
    {
        public GameObject holder;
        public UnitSelectionScript unitSelectionScript;
        public Inventory inventory;
        public TeamNumbers team;
    }

    [System.Serializable]
    public class PlayerConfig
    {
        public PlayerNumbers playerNumber;
        public TeamNumbers team;
    }

    public Dictionary<PlayerNumbers, AIPlayerContext> AIPlayers = new Dictionary<PlayerNumbers, AIPlayerContext>();
    public MainPlayerContext PlayerMain;

    public PlayerConfig[] playerConfigs;

    private GameObject currentHovering = null;

    private void Awake()
    {
        Instance = this;

        // @TODO: playerConfigs should be set from skirmish menu. For now, set in inspector
        InitAllPlayers(playerConfigs);
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
        AIPlayer player = _Holder.AddComponent<AIPlayer>();
        // @TODO: can't add generic classes as components, need to split 
        InventoryAI newInventory = _Holder.AddComponent<InventoryAI>();
        // Set initial player vars
        player.playerNumber = playerConf.playerNumber;
        player.teamNumber = playerConf.team;
        // Instantiate new AI player context and add it to AIPlayers dictionary
        AIPlayerContext newAI = new AIPlayerContext
        {
            holder = _Holder,
            playerScript = player,
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
        UnitSelectionScript unitSelection = _Holder.AddComponent<UnitSelectionScript>();
        PlayerMain = new MainPlayerContext
        {
            holder = _Holder,
            unitSelectionScript = unitSelection,
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
