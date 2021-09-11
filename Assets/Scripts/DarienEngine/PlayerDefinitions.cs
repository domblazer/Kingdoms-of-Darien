using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace DarienEngine
{
    public class MainPlayerContext
    {
        public GameObject holder;
        public Player player;
        public Inventory inventory;
        public TeamNumbers team;
    }

    public class PlayerConjurerArgs
    {
        public Button menuButton;
        public GameObject prefab;
        public int buildQueueCount = 0;
    }

    public interface IUnitBuilderPlayer
    {
        float lastClickTime { get; set; }
        float clickDelay { get; set; }
        bool isCurrentActive { get; set; }
        List<PlayerConjurerArgs> virtualMenu { get; set; }

        void QueueBuild(PlayerConjurerArgs item, Vector2 clickPoint);
        void InitVirtualMenu(GameObject[] prefabs);
        void ProtectDoubleClick();
        void ToggleBuildMenu(bool value);
        void TakeOverButtonListeners();
        void ReleaseButtonListeners();
        void SetCurrentActive();
        void ReleaseCurrentActive();
    }
}
