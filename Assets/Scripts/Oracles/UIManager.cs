using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance { get; private set; }

    public Text manaProportionText;
    public Text manaRechargeRateText;
    public Text manaUsageRateText;
    public Text selectedUnitCountText;
    public Text totalUnitsCountText;
    public Text statisticsText;
    public Text debugText;

    [System.Serializable]
    public class UnitInfo
    {
        [System.Serializable]
        public class PrimaryUnit
        {
            public Image unitIcon;
            public Slider healthBar;
            public Slider manaBar;
            public Text unitNameText;
            public Text statusText;
        }
        public PrimaryUnit primaryUnit;

        [System.Serializable]
        public class SecondaryUnit
        {
            public Slider healthBar;
            public Text unitNameText;
        }
        public SecondaryUnit secondaryUnit;
    }
    public UnitInfo unitInfo;

    public RectTransform actionMenuDefault;
    [System.Serializable]
    public class ActionMenuButtons
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
    }
    public ActionMenuButtons actionMenuButtons;

    public class ActionMenuSettings
    {
        public bool showMove;
        public bool showPatrol;
        public bool showAttack;
        public bool showGuard;
        public bool showRepair;
        public bool showClean;
        public class SpecialAttackItem
        {
            public Image specialAttackIcon;
            public string specialAttackName;
        }

        public SpecialAttackItem[] specialAttacks;
    }

    public RectTransform f4Menu;
    public RectTransform f2Menu;
    public bool pauseOnStart = false;

    [HideInInspector]
    public ActionMenu actionMenuInstance;
    [HideInInspector]
    public UnitInfoPanel unitInfoInstance;

    public Image chrystalBallTexture;
    public Sprite[] chrystalBallTextureArray;

    private void Awake()
    {
        Instance = this;
        actionMenuInstance = new ActionMenu(actionMenuDefault, actionMenuButtons);
        unitInfoInstance = new UnitInfoPanel(unitInfo);
    }

    private void Start()
    {
        ToggleF4Menu(false);
        ToggleF2InfoMenu(pauseOnStart);
        actionMenuInstance.Toggle(false);
        unitInfoInstance.Toggle(false);
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.F2))
        {
            ToggleF2InfoMenu(!f2Menu.gameObject.activeInHierarchy);
        }
        if (Input.GetKeyDown(KeyCode.F4))
        {
            ToggleF4Menu(!f4Menu.gameObject.activeInHierarchy);
        }
    }

    public void ToggleF4Menu(bool value)
    {
        f4Menu.gameObject.SetActive(value);
    }

    public void ToggleF2InfoMenu(bool value)
    {
        if (value)
        {
            GameManager.Instance.PauseGame();
        }
        else
        {
            GameManager.Instance.ResumeGame();
        }
        f2Menu.gameObject.SetActive(value);
    }

    public class ActionMenu
    {
        public RectTransform actionMenuDefault;
        public ActionMenuButtons actionMenuButtons;
        public bool actionMenuActive = false;

        public ActionMenu(RectTransform actionMenu, ActionMenuButtons actionMenuBtns)
        {
            actionMenuDefault = actionMenu;
            actionMenuButtons = actionMenuBtns;
        }

        public void Toggle(bool value)
        {
            actionMenuActive = value;
            bool opp = !value;
            Debug.Log("show action menu default " + opp);
            actionMenuDefault.gameObject.SetActive(opp);
            actionMenuButtons.moveBtn.gameObject.SetActive(value);
            actionMenuButtons.patrolBtn.gameObject.SetActive(value);
            actionMenuButtons.attackBtn.gameObject.SetActive(value);
            actionMenuButtons.guardBtn.gameObject.SetActive(value);
            actionMenuButtons.repairBtn.gameObject.SetActive(value);
            actionMenuButtons.cleanBtn.gameObject.SetActive(value);
            actionMenuButtons.stopBtn.gameObject.SetActive(value);
            actionMenuButtons.specialAttackOneBtn.gameObject.SetActive(value);
            actionMenuButtons.specialAttackTwoBtn.gameObject.SetActive(value);
            actionMenuButtons.specialAttackThreeBtn.gameObject.SetActive(value);
            actionMenuButtons.offensiveBtn.gameObject.SetActive(value);
            actionMenuButtons.defensiveBtn.gameObject.SetActive(value);
            actionMenuButtons.passiveBtn.gameObject.SetActive(value);
        }

        public void Set(bool canMove, bool canAttack, bool canBuild, ActionMenuSettings.SpecialAttackItem[] specialAttacks)
        {
            actionMenuActive = true;
            actionMenuDefault.gameObject.SetActive(false);
            if (canMove)
            {
                actionMenuButtons.moveBtn.gameObject.SetActive(true);
                actionMenuButtons.patrolBtn.gameObject.SetActive(true);
            }
            if (canAttack)
            {
                actionMenuButtons.attackBtn.gameObject.SetActive(true);
                actionMenuButtons.guardBtn.gameObject.SetActive(true);
            }
            if (canBuild)
            {
                actionMenuButtons.repairBtn.gameObject.SetActive(true);
                actionMenuButtons.cleanBtn.gameObject.SetActive(true);
            }
            actionMenuButtons.stopBtn.gameObject.SetActive(true);
            actionMenuButtons.offensiveBtn.gameObject.SetActive(true);
            actionMenuButtons.defensiveBtn.gameObject.SetActive(true);
            actionMenuButtons.passiveBtn.gameObject.SetActive(true);

            if (specialAttacks != null && specialAttacks.Length == 1)
            {
                actionMenuButtons.specialAttackOneBtn.gameObject.SetActive(true);
                actionMenuButtons.specialAttackOneBtn.image = specialAttacks[0].specialAttackIcon;
            }
            else if (specialAttacks != null && specialAttacks.Length == 2)
            {
                actionMenuButtons.specialAttackOneBtn.gameObject.SetActive(true);
                actionMenuButtons.specialAttackTwoBtn.gameObject.SetActive(true);
                actionMenuButtons.specialAttackOneBtn.image = specialAttacks[0].specialAttackIcon;
                actionMenuButtons.specialAttackTwoBtn.image = specialAttacks[1].specialAttackIcon;
            }
            else if (specialAttacks != null && specialAttacks.Length == 3)
            {
                actionMenuButtons.specialAttackOneBtn.gameObject.SetActive(true);
                actionMenuButtons.specialAttackTwoBtn.gameObject.SetActive(true);
                actionMenuButtons.specialAttackThreeBtn.gameObject.SetActive(true);
                actionMenuButtons.specialAttackOneBtn.image = specialAttacks[0].specialAttackIcon;
                actionMenuButtons.specialAttackTwoBtn.image = specialAttacks[1].specialAttackIcon;
                actionMenuButtons.specialAttackThreeBtn.image = specialAttacks[2].specialAttackIcon;
            }
            else
            {
                Debug.Log("No special attacks set.");
            }
        }
    }

    public class UnitInfoPanel
    {
        public UnitInfo unitInfo;
        public UnitInfoPanel(UnitInfo unitInfoObj)
        {
            unitInfo = unitInfoObj;
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

        public void Set(RTSUnit primaryUnit, RTSUnit secondaryUnit, bool excludeStatus = false)
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

            if (secondaryUnit != null)
            {
                // secondary unit name
                unitInfo.secondaryUnit.unitNameText.text = secondaryUnit.unitName;
                // secondary unit health
                unitInfo.secondaryUnit.healthBar.gameObject.SetActive(true);
                unitInfo.secondaryUnit.healthBar.value = secondaryUnit.health;
            }
        }
    }

    public void SetUnitUI(string status)
    {
        unitInfo.primaryUnit.statusText.text = status;
    }

    public void SetUnitUI(Sprite icon, string unitName, float health)
    {
        unitInfo.primaryUnit.unitIcon.gameObject.SetActive(true);
        unitInfo.primaryUnit.healthBar.gameObject.SetActive(true);
        unitInfo.primaryUnit.unitIcon.sprite = icon;
        unitInfo.primaryUnit.healthBar.value = health;
        unitInfo.primaryUnit.unitNameText.text = unitName;
    }

    public void SetUnitUI(Sprite icon, string unitName, float health, int mana, string status)
    {
        unitInfo.primaryUnit.unitIcon.gameObject.SetActive(true);
        unitInfo.primaryUnit.healthBar.gameObject.SetActive(true);

        if (mana != 0)
        {
            unitInfo.primaryUnit.manaBar.gameObject.SetActive(true);
            unitInfo.primaryUnit.manaBar.value = mana;
        }

        unitInfo.primaryUnit.unitIcon.sprite = icon;
        unitInfo.primaryUnit.healthBar.value = health;
        unitInfo.primaryUnit.unitNameText.text = unitName;
        unitInfo.primaryUnit.statusText.text = status;
    }

    public void SetSelectedCount(int count)
    {
        selectedUnitCountText.text = count.ToString();
    }

    public void SetTotalUnitsCount(int count)
    {
        totalUnitsCountText.text = count.ToString();
    }

    public void SetStatisticsText(string text)
    {
        statisticsText.text = text;
    }

    public void SetDebugText(string text)
    {
        debugText.text = text;
    }

    public void SetManaUI(Inventory inventory)
    {
        manaProportionText.text = inventory.currentMana + "/" + inventory.totalManaStorage;

        manaRechargeRateText.text = "+" + inventory.totalManaIncome;
        manaUsageRateText.text = "-" + inventory.manaDrainRate;

        // Get index of chrystal ball texture by mana proportion 
        float prct = ((float)inventory.currentMana / (float)inventory.totalManaStorage) * 100;
        int textureIndex = Mathf.RoundToInt(prct / (100 / chrystalBallTextureArray.Length));
        if (textureIndex > chrystalBallTextureArray.Length - 1)
            textureIndex = chrystalBallTextureArray.Length - 1;
        // Debug.Log("Chrystal ball (corrected) texture indx " + textureIndex);
        chrystalBallTexture.sprite = chrystalBallTextureArray[textureIndex];
    }

    public void SetSecondaryUnitUI(string secondaryName, float secondaryHealth)
    {
        // secondary unit name
        unitInfo.secondaryUnit.unitNameText.text = secondaryName;
        // secondary unit health
        unitInfo.secondaryUnit.healthBar.gameObject.SetActive(true);
        unitInfo.secondaryUnit.healthBar.value = secondaryHealth;
    }
}
