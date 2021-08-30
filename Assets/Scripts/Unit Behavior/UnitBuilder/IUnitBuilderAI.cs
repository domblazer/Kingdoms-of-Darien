using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Constants;

[System.Serializable]
public class BuildUnit
{
    public UnitCategories unitCategory;
    public GameObject intangiblePrefab;
    public override string ToString()
    {
        return intangiblePrefab.GetComponent<IntangibleUnitScript>().finalUnit.unitName;
    }
}

public interface IUnitBuilderAI
{
    void QueueBuild(GameObject intangiblePrefab);
}
