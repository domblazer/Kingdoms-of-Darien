using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DarienEngine
{
    /// <summary>Class <c>CanvasConfigs</c> defines hierarchical paths to Canvas elements.</summary>
    public static class CanvasConfigs
    {
        public static string GetCanvasRoot(Factions faction)
        {
            if (faction == Factions.Aramon)
                return "/AraCanvas";
            else if (faction == Factions.Taros)
                return "/TaroCanvas";
            else if (faction == Factions.Veruna)
                return "/VeruCanvas";
            else if (faction == Factions.Zhon)
                return "/ZhonCanvas";
            else
                return "/AraCanvas";
        }

        private static string UNIT_INFO_MENU_ROOT = "/UnitInfoMenu";
        private static string BATTLE_MENU_ROOT = "/BattleMenu";
        private static string F4_MENU_ROOT = "/F4Menu";
        public static string manaProportionTextPath = BATTLE_MENU_ROOT + "/mana/mana-counts/mana-proportion-text";
        public static string manaRechargeRateTextPath = BATTLE_MENU_ROOT + "/mana/mana-rates/mana-recharge-rate-text";
        public static string manaDrainRateTextPath = BATTLE_MENU_ROOT + "/mana/mana-rates/mana-drain-rate-text";
        public static string selectedUnitCountTextPath = F4_MENU_ROOT + "/selected-units-count";
        public static string totalUnitsCountTextPath = F4_MENU_ROOT + "/total-units-count";
        public static string statisticsTextPath;
        public static string debugTextPath;

        public static string primaryUnitIconPath = UNIT_INFO_MENU_ROOT + "/self-icon";
        public static string primaryUnitHealthBarPath = UNIT_INFO_MENU_ROOT + "/health-bar";
        public static string primaryUnitManaBarPath = UNIT_INFO_MENU_ROOT + "/mana-bar";
        public static string primaryUnitNameTextPath = UNIT_INFO_MENU_ROOT + "/unit-name";
        public static string primaryUnitStatusTextPath = UNIT_INFO_MENU_ROOT + "/unit-status";

        public static string secondaryUnitHealthBarPath = UNIT_INFO_MENU_ROOT + "/secondary-unit-health-bar";
        public static string secondaryUnitNameTextPath = UNIT_INFO_MENU_ROOT + "/secondary-unit-name";

        public static string moveBtnPath = BATTLE_MENU_ROOT + "/move";
        public static string patrolBtnPath = BATTLE_MENU_ROOT + "/patrol";
        public static string attackBtnPath = BATTLE_MENU_ROOT + "/attack";
        public static string guardBtnPath = BATTLE_MENU_ROOT + "/guard";
        public static string repairBtnPath = BATTLE_MENU_ROOT + "/repair";
        public static string cleanBtnPath = BATTLE_MENU_ROOT + "/clean";
        public static string stopBtnPath = BATTLE_MENU_ROOT + "/stop";
        public static string specialAttackOneBtnPath = BATTLE_MENU_ROOT + "/special-attack-01";
        public static string specialAttackTwoBtnPath = BATTLE_MENU_ROOT + "/special-attack-02";
        public static string specialAttackThreeBtnPath = BATTLE_MENU_ROOT + "/special-attack-03";
        public static string offensiveBtnPath = BATTLE_MENU_ROOT + "/offensive";
        public static string defensiveBtnPath = BATTLE_MENU_ROOT + "/defensive";
        public static string passiveBtnPath = BATTLE_MENU_ROOT + "/passive";
    }
}

