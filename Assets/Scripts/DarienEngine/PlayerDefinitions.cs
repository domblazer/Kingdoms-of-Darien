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

    public interface IUnitBuilderPlayer
    {
        float lastClickTime { get; set; }
        float clickDelay { get; set; }
        bool isCurrentActive { get; set; }
        List<ConjurerArgs> virtualMenu { get; set; }

        void QueueBuild(ConjurerArgs item, Vector2 clickPoint);
        void InitVirtualMenu(GameObject[] prefabs);
        void ProtectDoubleClick();
        void ToggleBuildMenu(bool value);
        void TakeOverButtonListeners();
        void ReleaseButtonListeners();
        void SetCurrentActive();
        void ReleaseCurrentActive();
    }
}
