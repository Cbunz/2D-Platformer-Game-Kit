using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VisibleBubbleUp : MonoBehaviour
{
    public System.Action<VisibleBubbleUp> objectBecameVisible;

    private void OnBecameInvisible()
    {
        objectBecameVisible(this);
    }
}
