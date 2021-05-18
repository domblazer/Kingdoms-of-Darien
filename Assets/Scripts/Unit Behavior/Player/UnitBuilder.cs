using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UnitBuilder : MonoBehaviour
{
    public string parentMenuPath = "/Menus/";
    private GameObject parentMenu;
    public string buildMenuName;
    private RectTransform buildMenu = null;
    private List<Button> buildMenuBtns = new List<Button>();
    public GameObject[] buildUnitPrefabs;

    [HideInInspector] public BaseUnitScript _BaseUnit;

    private bool isBuilding = false;

    public Transform builderSpawnPoint;
    public Transform builderRallyPoint; // Rally point position of stationary builders, e.g. if this BaseUnit is barracks
    // private Vector3 rallyPointPosition;

    // Master queue keeps track of order, e.g. 2 swordsmen, 1 archer, then 2 more swordsmen
    [HideInInspector] public Queue<BuildMapping> masterBuildQueue = new Queue<BuildMapping>();
    private bool nextQueueReady = false;

    private float lastClickTime;
    private float clickDelay = 0.25f;

    private bool tryRightOrLeft = true;

    private int placedSinceLastShift = 0;

    public struct BuildMapping
    {
        public Button button { get; set; }
        public GameObject prefab { get; set; }
        public Queue<GameObject> buildQueue { get; }
        public GameObject ghost { get; set; }

        public BuildMapping(Button btn, GameObject pref)
        {
            button = btn;
            prefab = pref;
            ghost = null;
            buildQueue = new Queue<GameObject>();
        }
    }

    private List<BuildMapping> virtualMenu = new List<BuildMapping>();

    private void Awake()
    {
        _BaseUnit = gameObject.GetComponent<BaseUnitScript>();

        // @TODO: handle kinematic builder behavior 
        // @TODO: also note kinematic builders menus do not have queue count text

        if (!_BaseUnit.isKinematic && !builderSpawnPoint)
        {
            throw new System.Exception("Cannot find a spawn point for builder. Create empty child on this object with name 'spawn-point'.");
        }

        // Get rally point for stationary builder
        if (!_BaseUnit.isKinematic && !builderRallyPoint)
        {
            // @Note: this gets passed to IntangibleUnit as referrer.builderRallyPoint which it passes back to newly instantiated BaseUnit
            throw new System.Exception("Stationary builders need a rally point. Create empty child and name it 'rally-point'.");
        }

        parentMenu = GameObject.Find(parentMenuPath);
        buildMenu = FindMenu(parentMenu, buildMenuName).GetComponent<RectTransform>(); // GameObject.Find(buildMenuName).GetComponent<RectTransform>();
        Debug.Log("Build menu found? " + buildMenu.gameObject);
        Debug.Log("Build menu active? " + buildMenu.gameObject.activeInHierarchy);
        // Hide menu and rally point by default
        buildMenu.gameObject.SetActive(false);
        Debug.Log("Build menu active? " + buildMenu.gameObject.activeInHierarchy);
    }

    // Start is called before the first frame update
    void Start()
    {
        // Warn if number of prefabs does not match number of buttons
        if (buildMenuBtns.Count != buildUnitPrefabs.Length)
        {
            Debug.LogWarning("The number of assigned prefabs does not match the size of the build menu.");
        }

        // @TODO: check for any unassigned prefabs

        // Get buttons from buildMenu
        int i = 0;
        foreach (Transform buildBtn in buildMenu)
        {
            Button btn = buildBtn.GetComponent<Button>();
            buildMenuBtns.Add(btn);
            BuildMapping item = new BuildMapping(btn, buildUnitPrefabs[i]);
            virtualMenu.Add(item);
            i++;
        }

        ToggleRallyPoint(false);
    }

    private void Update()
    {
        // Handle stationary builder queues
        if (!_BaseUnit.isKinematic)
        {
            // Keep track of master queue to know when building
            isBuilding = masterBuildQueue.Count > 0;

            UpdateAllButtonsText();

            // While masterQueue is not empty, continue queueing up intangible prefabs
            if (masterBuildQueue.Count > 0 && nextQueueReady)
            {
                // @TODO: also need to check that the spawn point is clear before moving on to next unit
                _BaseUnit.state = RTSUnit.States.Conjuring;
                BuildMapping next = masterBuildQueue.Peek();
                InstantiateNextIntangible(next);
                // Toggle whether new unit parks towards the right or left
                tryRightOrLeft = !tryRightOrLeft;
                nextQueueReady = false;

                // @TODO: handle infinite. If one unit got the infinite command, all subsequent left-clicks on that unit need to be ignored
                // (right-click) to clear. Also, as long as the condition is true, this should just keep pumping out the same unit
                // maybe something like:
                // if (mode === Modes.Infinite) {masterBuildQueue.Enqueue(map); map.buildQueue.Enqueue(map.prefab);}
            }
        }
        else
        {
            string a = "Master queue count: " + masterBuildQueue.Count + "\n";
            string b = "";
            // If builder is kinematic, keep traveling to ghosts in the queue until queue empty
            if (masterBuildQueue.Count > 0 && nextQueueReady)
            {
                BuildMapping next = masterBuildQueue.Peek();
                b = "Peek ghost name: " + next.ghost.name;
                // _BaseUnit.GetUIManager().SetDebugText(a + b);

                // @TODO: take an average of x and y for boxcollider sizes? 
                Vector3 offsetRange = next.ghost.GetComponent<GhostUnitScript>().sizeOffset;
                // Move to next ghost in the queue
                if (next.ghost.GetComponent<GhostUnitScript>().IsSet() && !_BaseUnit.IsInRangeOf(next.ghost.transform.position, offsetRange.x))
                {
                    _BaseUnit.SetMove(next.ghost.transform.position);
                    Debug.Log("Builder moving to ghost");
                }
                // When arrived at ghost, start building intangible
                else 
                {
                    Debug.Log("Arrived at ghost");
                    _BaseUnit.SetMove(transform.position);
                    _BaseUnit.state = RTSUnit.States.Conjuring;
                    nextQueueReady = false;
                    isBuilding = true;
                    masterBuildQueue.Dequeue();
                    next.ghost.GetComponent<GhostUnitScript>().StartBuild();
                }
            }
            else
            {
                // Debug.Log("Builder waiting");
            }

            // _BaseUnit.GetUIManager().SetDebugText(a + b);
        }
    }

    private GameObject FindMenu(GameObject parent, string name)
    {
        RectTransform[] trs = parent.GetComponentsInChildren<RectTransform>(true);
        foreach (RectTransform t in trs)
        {
            if (t.name == name)
            {
                return t.gameObject;
            }
        }
        return null;
    }

    public void ToggleRallyPoint(bool value)
    {
        if (!_BaseUnit.isKinematic)
        {
            builderRallyPoint.gameObject.SetActive(value);
        }
    }

    // Since each unitBuilder script will share the same UI objects, listeners will have to change hands each time a builder is selected or deselected
    public void TakeOverButtonListeners()
    {
        foreach (BuildMapping virtualBtn in virtualMenu)
        {
            virtualBtn.button.onClick.AddListener(delegate { QueueBuild(virtualBtn, Input.mousePosition); }); // Adds a listener on the button
        }
    }

    // Clear listeners for next selected builder
    public void ReleaseButtonListeners()
    {
        foreach (Button btn in buildMenuBtns)
        {
            btn.onClick.RemoveAllListeners();
        }
    }

    void QueueBuild(BuildMapping map, Vector2 clickPoint)
    {
        // First, protect double clicks with click delay
        if (lastClickTime + clickDelay > Time.unscaledTime)
        {
            return;
        }
        lastClickTime = Time.unscaledTime;

        // Handle kinematic builder behavior
        if (_BaseUnit.isKinematic)
        {
            GameObject ghost = InstantiateGhost(map, clickPoint); // Instantiate ghost prefab            
        }
        // Handle stationary builder behavior
        else
        {
            // Capture +5 or infinite command for stationary builder queue
            // string mode = "normal";
            bool plusFive = false;
            if (HoldingShift())
            {
                plusFive = true;
                // mode = "plus-five";
            }
            else if (HoldingCtrl())
            {
                // mode = "infinite";
            }

            if (plusFive)
            {
                for (int i = 0; i < 5; i++)
                {
                    map.buildQueue.Enqueue(map.prefab);
                    // Instantiate first immediately
                    if (masterBuildQueue.Count == 0)
                    {
                        nextQueueReady = true;
                        // InstantiateNextIntangible(map);
                    }
                    masterBuildQueue.Enqueue(map);
                    UpdateButtonText(map);
                }
            }
            // @TODO: else if mode is infinite, UpdateButtonText("+++");
            else
            {
                // Enqueue individual queue to keep track of count for each unit type in the buildMenu
                map.buildQueue.Enqueue(map.prefab);
                // Instantiate first immediately
                if (masterBuildQueue.Count == 0)
                {
                    nextQueueReady = true;
                    // InstantiateNextIntangible(map);
                }
                // Enqueue master queue to keep track of build order and total queue
                masterBuildQueue.Enqueue(map);
                UpdateButtonText(map);
            }
        }
    }

    void InstantiateNextIntangible(BuildMapping map)
    {
        GameObject intangible = Instantiate(map.prefab, builderSpawnPoint.position, new Quaternion(0, 180, 0, 1));
        intangible.GetComponent<IntangibleUnitScript>().SetReferences(this, map, tryRightOrLeft);
    }

    GameObject InstantiateGhost(BuildMapping map, Vector2 clickPoint)
    {
        Vector3 instantiatePoint = new Vector3(clickPoint.x, 1, clickPoint.y);
        GameObject ghost = Instantiate(map.prefab, instantiatePoint, map.prefab.transform.localRotation);
        map.ghost = ghost;
        ghost.GetComponent<GhostUnitScript>().SetReferences(this, map);
        return ghost;
    }

    public void PlusPlaced()
    {
        placedSinceLastShift++;
    }

    public void ResetPlaced()
    {
        placedSinceLastShift = 0;
    }

    public int GetPlaced()
    {
        return placedSinceLastShift;
    }

    void UpdateAllButtonsText()
    {
        foreach (BuildMapping map in virtualMenu)
        {
            UpdateButtonText(map);
        }
    }

    void UpdateButtonText(BuildMapping map)
    {
        string newBtnText = map.buildQueue.Count == 0 ? "" : "+" + map.buildQueue.Count.ToString();
        map.button.GetComponentInChildren<Text>().text = newBtnText;
    }

    public void ToggleBuildMenu(bool val)
    {
        buildMenu.gameObject.SetActive(val);
    }

    public void SetNextQueueReady(bool val)
    {
        nextQueueReady = val;

        if (_BaseUnit.isKinematic)
        {
            // Reset isBuilding for kinematic builders since once one intangible is done, builder must re-enter move routine before starting another
            isBuilding = false;
        }
    }

    public bool IsBuilding()
    {
        return isBuilding;
    }

    private bool HoldingShift()
    {
        return Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift);
    }

    private bool HoldingCtrl()
    {
        return Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl);
    }
}
