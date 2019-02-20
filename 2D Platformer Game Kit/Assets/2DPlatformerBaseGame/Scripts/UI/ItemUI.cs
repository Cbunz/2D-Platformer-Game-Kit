using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ItemUI : MonoBehaviour
{
    public enum ItemKey
    {
        coin,
        key,
    }

    public ItemKey itemKey;
    public InventoryController playerInventory;
    
    private Text text;

    private void Awake()
    {
        text = transform.Find("Count").GetComponent<Text>();
        UpdateText();
    }

    public void DebugIt()
    {
        Debug.Log("Well it is calling them.");
    }

    public void UpdateText()
    {
        switch (itemKey)
        {
            case ItemKey.coin:
                if (playerInventory.NumberOfItem("coin") <= 0 || playerInventory.HasItem("coin") == false)
                    text.text = "000";
                else
                    text.text = playerInventory.NumberOfItem("coin") < 10 ? "00" + (playerInventory.NumberOfItem("coin").ToString()) : (playerInventory.NumberOfItem("coin") < 100 ? "0" + (playerInventory.NumberOfItem("coin").ToString()) : (playerInventory.NumberOfItem("coin").ToString()));
                return;
            case ItemKey.key:
                if (playerInventory.NumberOfItem("key") <= 0 || playerInventory.HasItem("key") == false)
                    text.text = "00";
                else
                    text.text = playerInventory.NumberOfItem("key") < 10 ? "0" + (playerInventory.NumberOfItem("key").ToString()) : (playerInventory.NumberOfItem("key").ToString());
                return;
        }
    }
}
