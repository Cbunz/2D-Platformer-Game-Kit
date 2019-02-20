using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;
using UnityEngine.UI;

[Serializable]
public class ScrollingTextBehaviour : PlayableBehaviour
{
    public string message;
    public float startDelay;
    public float holdDelay;

    protected float duration;
    protected float inverseScrollingDuration;

    public override void OnGraphStart(Playable playable)
    {
        duration = (float)playable.GetDuration();
        float scrollingDuration = Mathf.Clamp(duration - holdDelay - startDelay, float.Epsilon, duration);
        inverseScrollingDuration = 1f / scrollingDuration;
    }

    public string GetMessage(float localTime)
    {
        localTime = Mathf.Clamp(localTime - startDelay, 0f, duration);
        float messageProportion = Mathf.Clamp01(localTime * inverseScrollingDuration);
        int characterCount = Mathf.FloorToInt(message.Length * messageProportion);
        return message.Substring(0, characterCount);
    }
}
