using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UnitBuilderAI : MonoBehaviour
{
    public BuildUnit[] buildUnitPrefabs;
    private bool isBuilding = false;
    public Transform builderSpawnPoint;
    public Transform builderRallyPoint;
    [HideInInspector] public Queue<GameObject> masterBuildQueue = new Queue<GameObject>();
    private bool nextQueueReady = false;

    [System.Serializable]
    public class BuildUnit {
        public RTSUnit.Categories unitCategory;
        public GameObject intangiblePrefab;
    }

    public void QueueBuild(GameObject intangiblePrefab) {

    }
}
