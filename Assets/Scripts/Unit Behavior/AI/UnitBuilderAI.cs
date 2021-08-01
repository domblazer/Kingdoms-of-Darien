using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UnitBuilderAI : MonoBehaviour
{
    public BuildUnit[] buildUnitPrefabs;
    public bool isBuilding { get; set; } = false;
    public Transform builderSpawnPoint;
    public Transform builderRallyPoint;
    [HideInInspector] public Queue<GameObject> masterBuildQueue = new Queue<GameObject>();
    private bool nextQueueReady = false;

    [System.Serializable]
    public class BuildUnit
    {
        public RTSUnit.Categories unitCategory;
        public GameObject intangiblePrefab;
    }

    private BaseUnitScriptAI _BaseUnitAI;

    public void QueueBuild(GameObject intangiblePrefab)
    {

    }

    private void Start()
    {
        _BaseUnitAI = gameObject.GetComponent<BaseUnitScriptAI>();
    }

    private void Update()
    {
        // Handle stationary builder queues
        if (!_BaseUnitAI.isKinematic)
        {
            // Keep track of master queue to know when building
            isBuilding = masterBuildQueue.Count > 0;

            // While masterQueue is not empty, continue queueing up intangible prefabs
            if (masterBuildQueue.Count > 0 && nextQueueReady)
            {
                // @TODO: also need to check that the spawn point is clear before moving on to next unit
                _BaseUnitAI.state = RTSUnit.States.Conjuring;
                GameObject nextItg = masterBuildQueue.Peek();
                // InstantiateNextIntangible(next);
                nextQueueReady = false;
            }
        }
    }

    private void InstantiateNextIntangible(GameObject itg)
    {
        GameObject intangible = Instantiate(itg, builderSpawnPoint.position, new Quaternion(0, 180, 0, 1));
        // @TODO: this itg needs to tell it's final prefab to park then just start roaming around the park point
        // intangible.GetComponent<IntangibleUnitScript>().SetReferences(this, map, tryRightOrLeft);
    }
}
