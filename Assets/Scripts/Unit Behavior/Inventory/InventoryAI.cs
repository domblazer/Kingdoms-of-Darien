using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DarienEngine;
using DarienEngine.AI;

public class InventoryAI : InventoryBase<AIConjurerArgs>
{
    private void Update()
    {
        UpdateMana();
    }
}
