using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FactoryBase<T> : UnitBuilderBase<T>
{
    public Transform spawnPoint;
    public Transform rallyPoint;
    protected bool parkingDirectionToggle = false;
}
