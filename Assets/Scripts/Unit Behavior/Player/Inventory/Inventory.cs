using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DarienEngine;

public class Inventory : InventoryBase
{
    // Update is called once per frame
    void Update()
    {
        UpdateMana();

        CheckWinLoseState();

        // @TODO: only update ui if there is a non-zero mana changeRate
        UIManager.Instance.SetManaUI(this);
    }
}
