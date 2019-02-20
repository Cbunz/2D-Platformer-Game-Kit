using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// [RequireComponent(typeof(Collider2D))]
public class InventoryItem : MonoBehaviour, IDataPersister
{
    public string inventoryKey = "";
    public int amount = 1;
    public LayerMask layers;
    public bool disableOnEnter = false;
    public AudioClip clip;
    public DataSettings dataSettings;

    void OnEnable()
    {
        PersistentDataManager.RegisterPersister(this);
    }

    void OnDisable()
    {
        PersistentDataManager.UnregisterPersister(this);
    }

    void Reset()
    {
        dataSettings = new DataSettings();
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (layers.Contains(other.gameObject))
        {
            var invController = other.GetComponent<InventoryController>();
            invController.AddItem(inventoryKey, amount);
            if (disableOnEnter)
            {
                gameObject.SetActive(false);
                Save();
            }

            if (clip)
            {
                AudioSource.PlayClipAtPoint(clip, transform.position);
            }
        }
    }

    public void Save()
    {
        PersistentDataManager.SetDirty(this);
    }

    void OnDrawGizmos()
    {
        Gizmos.DrawIcon(transform.position, "InventoryItem", false);
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
        return new Data<bool>(gameObject.activeSelf);
    }

    public void LoadData(Data data)
    {
        Data<bool> inventoryItemData = (Data<bool>)data;
        gameObject.SetActive(inventoryItemData.value);
    }
}
