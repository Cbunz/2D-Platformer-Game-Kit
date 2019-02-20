using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class Switch : MonoBehaviour
{
	public enum SwitchType
    {
        once,
        binary,
    }
    
    public SwitchType switchType;

    public string onceEventToTrigger;
    public string binaryEventToTriggerGreen;
    public string binaryEventToTriggerRed;
    public string triggerTag;

    public Sprite[] sprites;

    [HideInInspector]
    public bool on = false;
    [HideInInspector]
    public bool green = false;

    private SpriteRenderer spriteRenderer;
    private int index = 0;
    private bool canTrigger = true;

    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    private void OnEnable()
    {
        switch (switchType)
        {
            case SwitchType.binary:
                spriteRenderer.sprite = sprites[0];
                break;
            case SwitchType.once:
                spriteRenderer.sprite = sprites[2];
                break;
        }
        canTrigger = true;

        EventManager.StartListening("TriggerSwitchReset", TriggerSwitchReset);
    }

    private void OnDisable()
    {
        EventManager.StopListening("TriggerSwitchReset", TriggerSwitchReset);
    }

    public void Activate()
    {
        switch (switchType)
        {
            case SwitchType.binary:
                index = 1 - index;
                spriteRenderer.sprite = sprites[index];
                green = !green;
                if (green)
                    EventManager.TriggerEvent(binaryEventToTriggerGreen, triggerTag);
                else
                    EventManager.TriggerEvent(binaryEventToTriggerRed, triggerTag);
                break;
            case SwitchType.once:
                spriteRenderer.sprite = sprites[3];
                on = true;
                if (canTrigger)
                {
                    canTrigger = false;
                    EventManager.TriggerEvent(onceEventToTrigger, triggerTag);
                }
                break;
        }
    }

    public void TriggerSwitchReset(string tag)
    {
        if (green == true && tag == triggerTag)
        {
            index = 1 - index;
            spriteRenderer.sprite = sprites[index];
            green = !green;
        }
    }
}
