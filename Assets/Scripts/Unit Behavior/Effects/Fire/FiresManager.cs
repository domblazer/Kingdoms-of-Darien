using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FiresManager : MonoBehaviour
{
    public GameObject firesHolder;
    private ParticleSystem[] fires;
    private RTSUnit _Unit;

    // Start is called before the first frame update
    void Start()
    {
        _Unit = GetComponent<RTSUnit>();
        fires = firesHolder.GetComponentsInChildren<ParticleSystem>();
    }

    // Update is called once per frame
    void Update()
    {
        if (_Unit.health <= 70 && _Unit.health >= 50)
        {
            // show one fire?
            // @TODO: number and size of fires should be controlled by mix of 3D start size, and emission rate
        }
        else if (_Unit.health <= 50 && _Unit.health >= 30)
        {

        }
        else if (_Unit.health <= 30 && _Unit.health >= 10)
        {

        }
    }
}
