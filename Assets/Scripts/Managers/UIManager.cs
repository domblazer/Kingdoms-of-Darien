using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DarienEngine;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance { get; private set; }

    // battleMenuDefault is the right-side default image displaying faction shield
    public RectTransform battleMenuDefault;

    public RectTransform f4Menu;
    public RectTransform f2Menu;
    public RectTransform[] buildMenus;

    public RectTransform debugMenu;
    public Text debugText;

    // Chrystal ball UI elements
    public Image chrystalBallTexture;
    public Sprite[] chrystalBallTextureArray;

    // Static menu instances are virual representations of game menus from Canvas
    public static BattleMenu BattleMenuInstance;
    public static UnitInfoMenu UnitInfoInstance;
    public static F4Menu F4MenuInstance;

    public bool pauseOnStart = false;

    // Hierarchical path of Canvas, e.g. "/AraCanvas"
    private string canvasRootPath;
    public Canvas canvasRoot { get; set; }

    public bool tooltipActive = false;

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        // Get canvas root path from Player faction
        canvasRootPath = CanvasConfigs.GetCanvasRoot(GameManager.Instance.PlayerMain.player.playerFaction);
        canvasRoot = GameObject.Find(canvasRootPath).GetComponent<Canvas>();

        // Instantiate virtual menus from Canvas
        BattleMenuInstance = new BattleMenu(battleMenuDefault, canvasRootPath);
        UnitInfoInstance = new UnitInfoMenu(canvasRootPath);
        F4MenuInstance = new F4Menu(canvasRootPath);

        // Deactivate all menus on start. @Note: by default, the canvas prefab should have menus set in "active" state
        ToggleF4Menu(false);
        ToggleF2InfoMenu(pauseOnStart);

        ToggleDebugMenu(false);

        BattleMenuInstance.Toggle(false);
        UnitInfoInstance.Toggle(false);
        foreach (RectTransform buildMenu in buildMenus)
            buildMenu.gameObject.SetActive(false);
    }

    private void Update()
    {
        // Capture menu toggle input events
        if (Input.GetKeyDown(KeyCode.F2))
            ToggleF2InfoMenu(!f2Menu.gameObject.activeInHierarchy);
        if (Input.GetKeyDown(KeyCode.F4))
            ToggleF4Menu(!f4Menu.gameObject.activeInHierarchy);

        // @TODO: remove later: Use F5 to trigger debug menu for now. 
        if (Input.GetKeyDown(KeyCode.F5))
            ToggleDebugMenu(!debugMenu.gameObject.activeInHierarchy);

        // @TODO: Tilda key is usually used to toggle the health bars, for now toggling the army debug panels
        // @TODO: it's also not ideal this input is captured both here and in BaseUnitAI
        if (Input.GetKeyDown(KeyCode.BackQuote))
            UIManager.Instance.tooltipActive = !UIManager.Instance.tooltipActive;
    }

    public void ToggleF4Menu(bool value)
    {
        f4Menu.gameObject.SetActive(value);
    }

    public void ToggleF2InfoMenu(bool value)
    {
        if (value)
            GameManager.Instance.PauseGame();
        else
            GameManager.Instance.ResumeGame();
        f2Menu.gameObject.SetActive(value);
    }

    public void ToggleDebugMenu(bool value)
    {
        debugMenu.gameObject.SetActive(value);
    }

    /// <summary>Class <c>BattleMenu</c> is a virtual representation of the right-side game menu.</summary>
    public class BattleMenu
    {
        /// <summary>Class <c>BattleMenuButtons</c> models all buttons belonging to Battle Menu.</summary>
        public class BattleMenuButtons
        {
            public Button moveBtn;
            public Button patrolBtn;
            public Button attackBtn;
            public Button guardBtn;
            public Button repairBtn;
            public Button cleanBtn;
            public Button stopBtn;
            public Button specialAttackOneBtn;
            public Button specialAttackTwoBtn;
            public Button specialAttackThreeBtn;
            public Button offensiveBtn;
            public Button defensiveBtn;
            public Button passiveBtn;

            // Find all Buttons elements within the Canvas
            public BattleMenuButtons(string rootPath)
            {
                moveBtn = GameObject.Find(rootPath + CanvasConfigs.moveBtnPath).GetComponent<Button>();
                patrolBtn = GameObject.Find(rootPath + CanvasConfigs.patrolBtnPath).GetComponent<Button>();
                attackBtn = GameObject.Find(rootPath + CanvasConfigs.attackBtnPath).GetComponent<Button>();
                guardBtn = GameObject.Find(rootPath + CanvasConfigs.guardBtnPath).GetComponent<Button>();
                repairBtn = GameObject.Find(rootPath + CanvasConfigs.repairBtnPath).GetComponent<Button>();
                cleanBtn = GameObject.Find(rootPath + CanvasConfigs.cleanBtnPath).GetComponent<Button>();
                stopBtn = GameObject.Find(rootPath + CanvasConfigs.stopBtnPath).GetComponent<Button>();
                specialAttackOneBtn = GameObject.Find(rootPath + CanvasConfigs.specialAttackOneBtnPath).GetComponent<Button>();
                specialAttackTwoBtn = GameObject.Find(rootPath + CanvasConfigs.specialAttackTwoBtnPath).GetComponent<Button>();
                specialAttackThreeBtn = GameObject.Find(rootPath + CanvasConfigs.specialAttackThreeBtnPath).GetComponent<Button>();
                offensiveBtn = GameObject.Find(rootPath + CanvasConfigs.offensiveBtnPath).GetComponent<Button>();
                defensiveBtn = GameObject.Find(rootPath + CanvasConfigs.defensiveBtnPath).GetComponent<Button>();
                passiveBtn = GameObject.Find(rootPath + CanvasConfigs.passiveBtnPath).GetComponent<Button>();
            }
        }
        public RectTransform battleMenuDefault;
        public BattleMenuButtons battleMenuButtons;

        // Mana text UI elements
        public Text manaProportionText;
        public Text manaRechargeRateText;
        public Text manaDrainRateText;

        // Construct virtual representation of Battle Menu
        public BattleMenu(RectTransform menuDefault, string canvasRootPath)
        {
            manaProportionText = GameObject.Find(canvasRootPath + CanvasConfigs.manaProportionTextPath).GetComponent<Text>();
            manaRechargeRateText = GameObject.Find(canvasRootPath + CanvasConfigs.manaRechargeRateTextPath).GetComponent<Text>();
            manaDrainRateText = GameObject.Find(canvasRootPath + CanvasConfigs.manaDrainRateTextPath).GetComponent<Text>();

            battleMenuDefault = menuDefault;
            battleMenuButtons = new BattleMenuButtons(canvasRootPath);

            battleMenuButtons.moveBtn.onClick.AddListener(delegate { PrimeCommand(CommandTypes.Move); });
            battleMenuButtons.patrolBtn.onClick.AddListener(delegate { PrimeCommand(CommandTypes.Patrol); });
            battleMenuButtons.attackBtn.onClick.AddListener(delegate { PrimeCommand(CommandTypes.Attack); });
            battleMenuButtons.guardBtn.onClick.AddListener(delegate { PrimeCommand(CommandTypes.Guard); });

            // @TODO: this is stopping but then units don't go back to autopicking targets
            battleMenuButtons.stopBtn.onClick.AddListener(delegate { GameManager.Instance.PlayerMain.player.StopAllSelectedUnits(); });
            // @TODO: listeners for all other action buttons
        }

        // Clicking on a Battle Menu command button will ready that command to be sent to selected units
        public void PrimeCommand(CommandTypes commandType)
        {
            Debug.Log(string.Format("PrimeCommand({0})", commandType));
            if (CommandMappings.CursorMap.TryGetValue(commandType, out CursorManager.CursorType ct))
                CursorManager.Instance.SetActiveCursorType(ct);
            // Now the next click will send this command to the selected units
            GameManager.Instance.PlayerMain.player.SetPrimedCommand(commandType);
        }

        public void Toggle(bool value)
        {
            battleMenuDefault.gameObject.SetActive(!value);
            battleMenuButtons.moveBtn.gameObject.SetActive(value);
            battleMenuButtons.patrolBtn.gameObject.SetActive(value);
            battleMenuButtons.attackBtn.gameObject.SetActive(value);
            battleMenuButtons.guardBtn.gameObject.SetActive(value);
            battleMenuButtons.repairBtn.gameObject.SetActive(value);
            battleMenuButtons.cleanBtn.gameObject.SetActive(value);
            battleMenuButtons.stopBtn.gameObject.SetActive(value);
            battleMenuButtons.specialAttackOneBtn.gameObject.SetActive(value);
            battleMenuButtons.specialAttackTwoBtn.gameObject.SetActive(value);
            battleMenuButtons.specialAttackThreeBtn.gameObject.SetActive(value);
            battleMenuButtons.offensiveBtn.gameObject.SetActive(value);
            battleMenuButtons.defensiveBtn.gameObject.SetActive(value);
            battleMenuButtons.passiveBtn.gameObject.SetActive(value);
        }

        public void Set(bool canMove, bool canAttack, bool canBuild, AttackBehavior.Weapon[] weapons)
        {
            battleMenuDefault.gameObject.SetActive(false);
            if (canMove)
            {
                battleMenuButtons.moveBtn.gameObject.SetActive(true);
                battleMenuButtons.patrolBtn.gameObject.SetActive(true);
            }
            if (canAttack)
            {
                battleMenuButtons.attackBtn.gameObject.SetActive(true);
                battleMenuButtons.guardBtn.gameObject.SetActive(true);
            }
            if (canBuild)
            {
                battleMenuButtons.repairBtn.gameObject.SetActive(true);
                battleMenuButtons.cleanBtn.gameObject.SetActive(true);
            }
            battleMenuButtons.stopBtn.gameObject.SetActive(true);
            battleMenuButtons.offensiveBtn.gameObject.SetActive(true);
            battleMenuButtons.defensiveBtn.gameObject.SetActive(true);
            battleMenuButtons.passiveBtn.gameObject.SetActive(true);

            if (weapons != null && weapons.Length == 1 && weapons[0].specialAttack)
            {
                battleMenuButtons.specialAttackOneBtn.gameObject.SetActive(true);
                battleMenuButtons.specialAttackOneBtn.image = weapons[0].specialAttackIcon;
            }
            else if (weapons != null && weapons.Length == 2 && weapons[0].specialAttack && weapons[1].specialAttack)
            {
                battleMenuButtons.specialAttackOneBtn.gameObject.SetActive(true);
                battleMenuButtons.specialAttackTwoBtn.gameObject.SetActive(true);
                battleMenuButtons.specialAttackOneBtn.image = weapons[0].specialAttackIcon;
                battleMenuButtons.specialAttackTwoBtn.image = weapons[1].specialAttackIcon;
            }
            else if (weapons != null && weapons.Length == 3 && weapons[0].specialAttack && weapons[1].specialAttack && weapons[2].specialAttack)
            {
                battleMenuButtons.specialAttackOneBtn.gameObject.SetActive(true);
                battleMenuButtons.specialAttackTwoBtn.gameObject.SetActive(true);
                battleMenuButtons.specialAttackThreeBtn.gameObject.SetActive(true);
                battleMenuButtons.specialAttackOneBtn.image = weapons[0].specialAttackIcon;
                battleMenuButtons.specialAttackTwoBtn.image = weapons[1].specialAttackIcon;
                battleMenuButtons.specialAttackThreeBtn.image = weapons[2].specialAttackIcon;
            }
        }

        public void UpdateManaText(Inventory inv)
        {
            manaProportionText.text = inv.currentMana + "/" + inv.totalManaStorage;
            manaRechargeRateText.text = "+" + inv.totalManaIncome;
            manaDrainRateText.text = "-" + Mathf.RoundToInt(inv.totalManaDrainPerSecond);
        }
    }

    public class UnitInfoMenu
    {
        public UnitInfo unitInfo;
        public class UnitInfo
        {
            public class PrimaryUnit
            {
                public Image unitIcon;
                public Slider healthBar;
                public Slider manaBar;
                public Text unitNameText;
                public Text statusText;
                public PrimaryUnit(string rootPath)
                {
                    unitIcon = GameObject.Find(rootPath + CanvasConfigs.primaryUnitIconPath).GetComponent<Image>();
                    healthBar = GameObject.Find(rootPath + CanvasConfigs.primaryUnitHealthBarPath).GetComponent<Slider>();
                    manaBar = GameObject.Find(rootPath + CanvasConfigs.primaryUnitManaBarPath).GetComponent<Slider>();
                    unitNameText = GameObject.Find(rootPath + CanvasConfigs.primaryUnitNameTextPath).GetComponent<Text>();
                    statusText = GameObject.Find(rootPath + CanvasConfigs.primaryUnitStatusTextPath).GetComponent<Text>();
                }
            }
            public PrimaryUnit primaryUnit;

            public class SecondaryUnit
            {
                public Slider healthBar;
                public Text unitNameText;
                public SecondaryUnit(string rootPath)
                {
                    healthBar = GameObject.Find(rootPath + CanvasConfigs.secondaryUnitHealthBarPath).GetComponent<Slider>();
                    unitNameText = GameObject.Find(rootPath + CanvasConfigs.secondaryUnitNameTextPath).GetComponent<Text>();
                }
            }
            public SecondaryUnit secondaryUnit;
            public UnitInfo(string canvasRootPath)
            {
                primaryUnit = new PrimaryUnit(canvasRootPath);
                secondaryUnit = new SecondaryUnit(canvasRootPath);
            }
        }
        public UnitInfoMenu(string canvasRootPath)
        {
            unitInfo = new UnitInfo(canvasRootPath);
        }

        public void Toggle(bool value)
        {
            unitInfo.primaryUnit.unitIcon.gameObject.SetActive(value);
            unitInfo.primaryUnit.healthBar.gameObject.SetActive(value);
            unitInfo.primaryUnit.manaBar.gameObject.SetActive(value);
            unitInfo.secondaryUnit.healthBar.gameObject.SetActive(value);

            if (!value)
            {
                unitInfo.primaryUnit.unitNameText.text = "";
                unitInfo.primaryUnit.statusText.text = "";
                unitInfo.secondaryUnit.unitNameText.text = "";
            }
        }

        public void Set(RTSUnit primaryUnit, GameObject secondaryUnit, bool excludeStatus = false)
        {
            // Sprite icon, string unitName, float health, int mana, string status
            unitInfo.primaryUnit.unitIcon.gameObject.SetActive(true);
            unitInfo.primaryUnit.healthBar.gameObject.SetActive(true);

            if (primaryUnit.mana != 0)
            {
                unitInfo.primaryUnit.manaBar.gameObject.SetActive(true);
                unitInfo.primaryUnit.manaBar.value = primaryUnit.mana;
            }

            unitInfo.primaryUnit.unitIcon.sprite = primaryUnit.unitIcon;
            unitInfo.primaryUnit.healthBar.value = primaryUnit.health;
            unitInfo.primaryUnit.unitNameText.text = primaryUnit.unitName;

            if (!excludeStatus)
                unitInfo.primaryUnit.statusText.text = primaryUnit.state.Value;
            else
                unitInfo.primaryUnit.statusText.text = "";

            if (secondaryUnit != null)
            {
                if (secondaryUnit.GetComponent<RTSUnit>())
                {
                    RTSUnit secondary = secondaryUnit.GetComponent<RTSUnit>();
                    // secondary unit name
                    unitInfo.secondaryUnit.unitNameText.text = secondary.unitName;
                    // secondary unit health
                    unitInfo.secondaryUnit.healthBar.gameObject.SetActive(true);
                    unitInfo.secondaryUnit.healthBar.value = secondary.health;
                }
                else if (secondaryUnit.GetComponent<IntangibleUnitBase>())
                {
                    IntangibleUnitBase secondary = secondaryUnit.GetComponent<IntangibleUnitBase>();
                    // secondary unit name
                    unitInfo.secondaryUnit.unitNameText.text = secondary.finalUnit.unitName;
                    // secondary unit health
                    unitInfo.secondaryUnit.healthBar.gameObject.SetActive(true);
                    unitInfo.secondaryUnit.healthBar.value = secondary.health * 100;
                }
            }
            // If secondaryUnit is cleared, clear the UI too
            else if (secondaryUnit == null & unitInfo.secondaryUnit.healthBar.gameObject.activeInHierarchy)
            {
                unitInfo.secondaryUnit.healthBar.gameObject.SetActive(false);
                unitInfo.secondaryUnit.unitNameText.text = "";
            }
        }

        // Set intangible UI
        public void Set(IntangibleUnitBase intangibleUnit, UnitBuilderBase mainBuilder)
        {
            // Sprite icon, string unitName, float health, int mana, string status
            unitInfo.primaryUnit.unitIcon.gameObject.SetActive(true);
            unitInfo.primaryUnit.healthBar.gameObject.SetActive(true);

            // Set main unit UI
            unitInfo.primaryUnit.unitIcon.sprite = intangibleUnit.finalUnit.unitIcon;
            unitInfo.primaryUnit.healthBar.value = intangibleUnit.health * 100;
            unitInfo.primaryUnit.unitNameText.text = intangibleUnit.finalUnit.unitName;

            // Set secondary unit UI
            if (mainBuilder)
            {
                unitInfo.primaryUnit.statusText.text = "Intangible Mass";
                unitInfo.secondaryUnit.unitNameText.text = mainBuilder.BaseUnit.unitName;
                unitInfo.secondaryUnit.healthBar.gameObject.SetActive(true);
                unitInfo.secondaryUnit.healthBar.value = mainBuilder.BaseUnit.health;
            }
            else
            {
                unitInfo.secondaryUnit.healthBar.gameObject.SetActive(false);
                unitInfo.secondaryUnit.unitNameText.text = "";
            }
        }
    }

    public class F4Menu
    {
        public Text selectedUnitCountText;
        public Text totalUnitsCountText;
        public F4Menu(string rootPath)
        {
            selectedUnitCountText = GameObject.Find(rootPath + CanvasConfigs.selectedUnitCountTextPath).GetComponent<Text>();
            totalUnitsCountText = GameObject.Find(rootPath + CanvasConfigs.totalUnitsCountTextPath).GetComponent<Text>();
        }
    }

    public void SetManaUI(Inventory inventory)
    {
        BattleMenuInstance.UpdateManaText(inventory);

        // Get index of chrystal ball texture by mana proportion 

        float prct = ((float)inventory.currentMana / (float)inventory.totalManaStorage) * 100;
        int textureIndex = Mathf.RoundToInt(prct / (100 / chrystalBallTextureArray.Length));
        if (textureIndex > chrystalBallTextureArray.Length - 1)
            textureIndex = chrystalBallTextureArray.Length - 1;
        // Debug.Log("Chrystal ball (corrected) texture indx " + textureIndex);
        chrystalBallTexture.sprite = chrystalBallTextureArray[textureIndex];
    }

    public void SetDebugText(string text)
    {
        debugText.text = text;
    }
}
