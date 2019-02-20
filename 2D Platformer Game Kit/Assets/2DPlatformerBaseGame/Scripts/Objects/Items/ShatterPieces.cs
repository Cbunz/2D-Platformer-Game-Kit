using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShatterPieces : MonoBehaviour {

    public float timeToDisappear = 0f;

    private void OnCollisionEnter2D(Collision2D collision)
    {
        Destroy(this.gameObject, timeToDisappear);
    }
}
