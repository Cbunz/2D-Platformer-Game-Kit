using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class InteractOnButton2D : InteractOnTrigger2D
{
    public InventoryController playerInventory;
    public UnityEvent OnButtonPress;

    bool canExecuteButtons;
    bool inventoryCheckPassed = false;

    protected override void ExecuteOnEnter(Collider2D other)
    {
        canExecuteButtons = true;
        OnEnter.Invoke();
    }

    protected override void ExecuteOnExit(Collider2D other)
    {
        canExecuteButtons = false;
        OnExit.Invoke();
    }

    private void Update()
    {
        if (canExecuteButtons && OnButtonPress.GetPersistentEventCount() > 0 && PlayerInput.Instance.Interact.Down)
        {
            playerInventory.Dump();
            if (inventoryCheckPassed)
            {
                OnButtonPress.Invoke();
            }
            else if (inventoryCheck.CheckInventory(playerInventory))
            {
                inventoryCheckPassed = true;
                if (consumeItemOnCheck)
                    foreach (InventoryController.InventoryChecker.NeededItems neededItems in inventoryCheck.neededItemsArray)
                        playerInventory.RemoveItem(neededItems.item, neededItems.amount);
                OnButtonPress.Invoke();
            }    
        }
    }
}
