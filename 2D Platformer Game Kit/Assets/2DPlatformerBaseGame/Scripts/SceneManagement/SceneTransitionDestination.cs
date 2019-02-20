using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class SceneTransitionDestination : MonoBehaviour {

    public enum DestinationTag
    {
        A,
        B,
        C,
        D,
        E,
        F,
        G,
    }

    public DestinationTag destinationTag;
    public GameObject transitioningGameObject;
    public UnityEvent OnReachDestination;
}
