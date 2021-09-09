using UnityEngine;

namespace DarienEngine
{
    public class AIPlayerContext
    {
        public GameObject holder;
        public AIPlayer player;
        public InventoryAI inventory;
        public TeamNumbers team;
    }
    
    [System.Serializable]
    public class BuildUnit
    {
        public UnitCategories unitCategory;
        public GameObject intangiblePrefab;
        public override string ToString()
        {
            return intangiblePrefab.GetComponent<IntangibleUnitAI>().finalUnit.unitName;
        }
    }

    [System.Serializable]
    public class AIConjurerArgs
    {
        public GameObject nextIntangible;
    }

    public interface IUnitBuilderAI
    {
        void QueueBuild(GameObject intangiblePrefab);
    }
}