using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DarienEngine;

public class InventoryAI : InventoryBase<AIConjurerArgs>
{
    private void Update()
    {
        UpdateMana();
    }
}
