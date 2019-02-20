using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Playables;

[RequireComponent(typeof(Collider2D))]
public class DirectorTrigger : MonoBehaviour, IDataPersister
{
    public enum TriggerType
    {
        Once,
        Everytime,
    }

    [Tooltip("This is the gameobject which will trigger the director to play. For example, the player.")]
    public GameObject triggeringGameObject;
    public PlayableDirector director;
    public TriggerType triggerType;
    public UnityEvent OnDirectorPlay;
    public UnityEvent OnDirectorFinish;
    [HideInInspector]
    public DataSettings dataSettings;

    protected bool alreadyTriggered;

    private void OnEnable()
    {
        PersistentDataManager.RegisterPersister(this);
    }

    void OnDisable()
    {
        PersistentDataManager.UnregisterPersister(this);
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.gameObject != triggeringGameObject)
            return;

        if (triggerType == TriggerType.Once && alreadyTriggered)
            return;

        director.Play();
        alreadyTriggered = true;
        OnDirectorPlay.Invoke();
        Invoke("FinishInvoke", (float)director.duration);
    }

    void FinishInvoke()
    {
        OnDirectorFinish.Invoke();
    }

    public void OverrideAlreadyTriggered(bool _alreadyTriggered)
    {
        alreadyTriggered = _alreadyTriggered;
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
        return new Data<bool>(alreadyTriggered);
    }

    public void LoadData(Data data)
    {
        Data<bool> directorTriggerData = (Data<bool>)data;
        alreadyTriggered = directorTriggerData.value;
    }
}
