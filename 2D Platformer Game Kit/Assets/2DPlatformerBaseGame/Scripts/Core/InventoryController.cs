using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class InventoryController : MonoBehaviour, IDataPersister, IEnumerable
{
    [System.Serializable]
    public struct StartingItem
    {
        public string item;
        public int amount;
    }

    public StartingItem[] startingInventory;

    [System.Serializable]
    public class InventoryEvent
    {
        public string key;
        public UnityEvent OnAdd, OnRemove;
    }

    [System.Serializable]
    public class InventoryChecker
    {
        [Serializable]
        public struct NeededItems
        {
            public string item;
            public int amount;
        }
        public NeededItems[] neededItemsArray;
        public UnityEvent OnHasItem, OnDoesNotHaveItem;

        Dictionary<string, int> neededItems = new Dictionary<string, int>();
        bool dictionaryPopulated = false;

        public bool CheckInventory(InventoryController inventory)
        {
            if (neededItemsArray != null && dictionaryPopulated == false)
            {
                foreach (NeededItems nItem in neededItemsArray)
                {
                    if (!neededItems.ContainsKey(nItem.item))
                    {
                        neededItems.Add(nItem.item, nItem.amount);
                    }
                    else
                    {
                        neededItems[nItem.item]++;
                    }
                }
                dictionaryPopulated = true;
            }

            if (inventory != null)
            {
                foreach (KeyValuePair<string, int> item in neededItems)
                {
                    if (inventory.HasItem(item.Key) && inventory.NumberOfItem(item.Key) >= item.Value)
                    {
                        OnHasItem.Invoke();
                        return true;
                    }
                }
                OnDoesNotHaveItem.Invoke();
                return false;
            }
            return false;
        }
    }

    public InventoryEvent[] inventoryEvents;
    public event System.Action OnInventoryLoaded;

    public DataSettings dataSettings;

    [HideInInspector]
    public Dictionary<string, int> inventoryItems = new Dictionary<string, int>();

    private void Awake()
    {
        foreach (StartingItem startingItem in startingInventory)
        {
            if (!inventoryItems.ContainsKey(startingItem.item))
                inventoryItems.Add(startingItem.item, startingItem.amount);
            else
                inventoryItems[startingItem.item]++;
        }
    }

    [ContextMenu("Dump")]
    public void Dump()
    {
        foreach (KeyValuePair<string, int> item in inventoryItems)
        {
            Debug.Log(item.Key + ": " + item.Value);
        }
    }

    void OnEnable()
    {
        PersistentDataManager.RegisterPersister(this);
    }

    void OnDisable()
    {
        PersistentDataManager.UnregisterPersister(this);
    }

    public void AddItem(string item)
    {
        if (!inventoryItems.ContainsKey(item))
        {
            inventoryItems.Add(item, 1);
            var invEvent = GetInventoryEvent(item);
            if (invEvent != null)
            {
                invEvent.OnAdd.Invoke();
            }
        }
        else
        {
            inventoryItems[item]++;
            var invEvent = GetInventoryEvent(item);
            if (invEvent != null)
                invEvent.OnAdd.Invoke();
        }
    }

    public void AddItem(string item, int amount)
    {
        if (!inventoryItems.ContainsKey(item))
        {
            inventoryItems.Add(item, amount);
            var invEvent = GetInventoryEvent(item);
            if (invEvent != null)
                invEvent.OnAdd.Invoke();
        }
        else
        {
            inventoryItems[item] += amount;
            var invEvent = GetInventoryEvent(item);
            if (invEvent != null)
                invEvent.OnAdd.Invoke();
        }
    }
    
    IEnumerator IEnumerable.GetEnumerator()
    {
        return inventoryItems.GetEnumerator();
    }

    public void AddAllItems(InventoryController desiredInventory)
    {
        foreach (KeyValuePair<string, int> desiredItem in desiredInventory)
        {
            if (!inventoryItems.ContainsKey(desiredItem.Key))
            {
                inventoryItems.Add(desiredItem.Key, desiredItem.Value);
                var invEvent = GetInventoryEvent(desiredItem.Key);
                if (invEvent != null)
                    invEvent.OnAdd.Invoke();
            }
            else
            {
                inventoryItems[desiredItem.Key] += desiredItem.Value;
                var invEvent = GetInventoryEvent(desiredItem.Key);
                if (invEvent != null)
                    invEvent.OnAdd.Invoke();
            }
        }
    }

    public void RemoveItem(string item)
    {
        if (inventoryItems.ContainsKey(item) && inventoryItems[item] == 1)
        {
            var invEvent = GetInventoryEvent(item);
            if (invEvent != null)
            {
                invEvent.OnRemove.Invoke();
            }
            inventoryItems.Remove(item);
        }
        else if (inventoryItems.ContainsKey(item))
        {
            var invEvent = GetInventoryEvent(item);
            if (invEvent != null)
                invEvent.OnRemove.Invoke();
            inventoryItems[item]--;
        }
    }

    public void RemoveItem(string item, int amount)
    {
        if (inventoryItems.ContainsKey(item) && inventoryItems[item] == amount)
        {
            var invEvent = GetInventoryEvent(item);
            if (invEvent != null)
                invEvent.OnRemove.Invoke();
            inventoryItems.Remove(item);
        }
        else if (inventoryItems.ContainsKey(item) && inventoryItems[item] > amount)
        {
            var invEvent = GetInventoryEvent(item);
            if (invEvent != null)
                invEvent.OnRemove.Invoke();
            inventoryItems[item] -= amount;
        }
        else
        {
            Debug.Log("Insufficient amount of " + item);
        }
    }

    public bool HasItem(string item)
    {
        return inventoryItems.ContainsKey(item);
    }

    public int NumberOfItem(string item)
    {
        if (inventoryItems.ContainsKey(item))
            return inventoryItems[item];
        else
            return 0;
    }

    public void Clear()
    {
        inventoryItems.Clear();
    }

    InventoryEvent GetInventoryEvent(string item)
    {
        foreach (var invEvent in inventoryEvents)
        {
            if (invEvent.key == item)
            {
                return invEvent;
            }
        }
        return null;
    }

    public DataSettings GetDataSettings()
    {
        return dataSettings;
    }

    public void SetDataSettings(string dataTag, DataSettings.PersistenceType persistenceType)
    {
        dataSettings.dataTag = dataTag;
        dataSettings.persistenceType = persistenceType;
    }

    public Data SaveData()
    {
        return new Data<Dictionary<string, int>>(inventoryItems);
    }

    public void LoadData(Data data)
    {
        Data<Dictionary<string, int>> inventoryData = (Data<Dictionary<string, int>>)data;
        foreach (var item in inventoryData.value)
        {
            AddItem(item.Key);
        }
        if (OnInventoryLoaded != null) OnInventoryLoaded();
    }
}
