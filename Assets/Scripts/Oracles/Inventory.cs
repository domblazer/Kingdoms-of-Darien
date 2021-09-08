using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Inventory : InventoryBase<PlayerConjurerArgs>
{
    // Update is called once per frame
    void Update()
    {
        UpdateMana();

        // @TODO: show mana ui
        // UIManager.Instance.SetManaUI(inventory);
    }
}
