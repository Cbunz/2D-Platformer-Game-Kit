using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

[System.Serializable]
public class StringEvent : UnityEvent<string> { }

public class EventManager : MonoBehaviour {

    private Dictionary<string, UnityEvent> eventDictionary;
    private Dictionary<string, StringEvent> stringEventDictionary;

    private static EventManager eventManager;

    public static EventManager Instance
    {
        get
        {
            if (!eventManager)
            {
                eventManager = FindObjectOfType(typeof(EventManager)) as EventManager;

                if (!eventManager)
                   eventManager = new GameObject("EventManager").AddComponent<EventManager>();

                if (!eventManager)
                    Debug.LogError("Could not create EventManager");
                else
                    eventManager.Init();
            }

            return eventManager;
        }
    }

    private void Init()
    {
        if (eventDictionary == null)
            eventDictionary = new Dictionary<string, UnityEvent>();
        if (stringEventDictionary == null)
            stringEventDictionary = new Dictionary<string, StringEvent>();
    }

    public static void StartListening(string eventName, UnityAction listener)
    {
        UnityEvent thisEvent = null;
        if (Instance.eventDictionary.TryGetValue(eventName, out thisEvent))
            thisEvent.AddListener(listener);
        else
        {
            thisEvent = new UnityEvent();
            thisEvent.AddListener(listener);
            Instance.eventDictionary.Add(eventName, thisEvent);
        }
    }

    public static void StartListening(string eventName, UnityAction<string> listener)
    {
        StringEvent thisEvent = null;
        if (Instance.stringEventDictionary.TryGetValue(eventName, out thisEvent))
            thisEvent.AddListener(listener);
        else
        {
            thisEvent = new StringEvent();
            thisEvent.AddListener(listener);
            Instance.stringEventDictionary.Add(eventName, thisEvent);
        }
    }

    public static void StopListening(string eventName, UnityAction listener)
    {
        if (eventManager == null)
            return;
        UnityEvent thisEvent = null;
        if (Instance.eventDictionary.TryGetValue(eventName, out thisEvent))
            thisEvent.RemoveListener(listener);
    }

    public static void StopListening(string eventName, UnityAction<string> listener)
    {
        if (eventManager == null)
            return;
        StringEvent thisEvent = null;
        if (Instance.stringEventDictionary.TryGetValue(eventName, out thisEvent))
            thisEvent.RemoveListener(listener);
    }

    public static void TriggerEvent(string eventName)
    {
        UnityEvent thisEvent = null;
        if (Instance.eventDictionary.TryGetValue(eventName, out thisEvent))
            thisEvent.Invoke();
    }

    public static void TriggerEvent(string eventName, string tag)
    {
        StringEvent thisEvent = null;
        if (Instance.stringEventDictionary.TryGetValue(eventName, out thisEvent))
            thisEvent.Invoke(tag);
    }
}
