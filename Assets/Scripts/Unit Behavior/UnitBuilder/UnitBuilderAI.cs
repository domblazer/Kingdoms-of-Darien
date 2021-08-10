using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UnitBuilderAI<T> : UnitBuilderBase<T>
{
    [System.Serializable]
    public class BuildUnit
    {
        public RTSUnit.Categories unitCategory;
        public GameObject intangiblePrefab;
        public override string ToString()
        {
            return intangiblePrefab.GetComponent<IntangibleUnitScript>().finalUnit.unitName;
        }
    }
    public BuildUnit[] buildUnitPrefabs;

    public void QueueBuild(GameObject intangiblePrefab)
    {
        Debug.Log("UnitBuilderAI queued: " + intangiblePrefab.GetComponent<IntangibleUnitScript>().finalUnit.unitName);
        if (masterBuildQueue.Count == 0)
            nextQueueReady = true;
        // Enqueue master queue to keep track of build order and total queue
        masterBuildQueue.Enqueue(intangiblePrefab);
    }


}
