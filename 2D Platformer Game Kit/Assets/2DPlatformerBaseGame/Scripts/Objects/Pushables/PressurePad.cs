using System.Collections;
using System.Collections.Generic;
#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;
using UnityEngine.Events;

public class PressurePad : MonoBehaviour
{
    public enum ActivationType
    {
        ItemCount, ItemMass
    }

    public PlatformCatcher platformCatcher;
    public ActivationType activationType;
    public int requiredCount;
    public float requiredMass;
    public Sprite deactivatedBoxSprite;
    public Sprite activatedBoxSprite;
    public SpriteRenderer[] boxes;
    public UnityEvent OnPressed;
    public UnityEvent OnReleased;

    protected bool eventFired;

    static int DELAYEDFRAME_COUNT = 2;
    protected int activationFrameCount = 0;
    protected bool previousWasPressed = false;

#if UNITY_EDITOR
    protected GUIStyle errorStyle = new GUIStyle();
    protected GUIStyle errorBackgroundStyle = new GUIStyle();
#endif

    private void FixedUpdate()
    {
        if (activationType == ActivationType.ItemCount)
        {
            if (platformCatcher.CaughtObjectCount >= requiredCount)
            {
                if (!previousWasPressed)
                {
                    previousWasPressed = true;
                    activationFrameCount = 1;
                }
                else
                    activationFrameCount += 1;

                if (activationFrameCount > DELAYEDFRAME_COUNT && !eventFired)
                {
                    OnPressed.Invoke();
                    eventFired = true;
                }
            }
            else
            {
                if (previousWasPressed)
                {
                    previousWasPressed = false;
                    activationFrameCount = 1;
                }
                else
                    activationFrameCount += 1;

                if (activationFrameCount > DELAYEDFRAME_COUNT && eventFired)
                {
                    OnReleased.Invoke();
                    eventFired = false;
                }
            }
        }
        else
        {
            if (platformCatcher.CaughtObjectsMass >= requiredMass)
            {
                if (!previousWasPressed)
                {
                    previousWasPressed = true;
                    activationFrameCount = 1;
                }
                else
                    activationFrameCount += 1;

                if (activationFrameCount > DELAYEDFRAME_COUNT && !eventFired)
                {
                    OnPressed.Invoke();
                    eventFired = true;
                }
            }
            else
            {
                if (previousWasPressed)
                {
                    previousWasPressed = false;
                    activationFrameCount = 1;
                }
                else
                    activationFrameCount += 1;

                if (activationFrameCount > DELAYEDFRAME_COUNT && eventFired)
                {
                    OnReleased.Invoke();
                    eventFired = false;
                }
            }
        }

        for (int i = 0; i < boxes.Length; i++)
        {
            boxes[i].sprite = platformCatcher.HasCaughtObject(boxes[i].gameObject) ? activatedBoxSprite : deactivatedBoxSprite;
        }
    }

#if UNITY_EDITOR
    private void OnDrawGizmos()
    {
        Rigidbody2D rigidbody = GetComponentInChildren<Rigidbody2D>();

        if (rigidbody == null)
            return;

        if (rigidbody.bodyType == RigidbodyType2D.Static && GetComponentInParent<MovingPlatform>() != null)
        {
            errorStyle.alignment = TextAnchor.MiddleLeft;
            errorStyle.fontSize = Mathf.FloorToInt(18 * (1.0f / HandleUtility.GetHandleSize(transform.position)));
            errorStyle.normal.textColor = Color.white;

            Handles.Label(transform.position + Vector3.up * 1.5f + Vector3.right, "ERROR : Rigidbody body type on that pressure plate is set to Static!\n It won't move with the moving platform. Change it to Kinematic.", errorStyle);

            Handles.color = Color.red;
            Handles.DrawWireDisc(transform.position, Vector3.back, 0.5f);
            Handles.color = Color.white;
            Handles.DrawLine(transform.position + Vector3.up * 1.0f + Vector3.right, transform.position);
        }
    }
#endif
}
