using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DarienEngine
{
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
