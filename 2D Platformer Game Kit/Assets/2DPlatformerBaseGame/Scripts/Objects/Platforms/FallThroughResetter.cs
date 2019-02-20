using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Switches platform layer to default for enough time to allow the player to fall through.
/// </summary>

public class FallThroughResetter : MonoBehaviour
{
    public void StartFall(PlatformEffector2D effector)
    {
        StartCoroutine(FallCoroutine(effector));
    }

    IEnumerator FallCoroutine(PlatformEffector2D effector)
    {
        int playerLayerMask = 1 << 9;  //LayerMask.NameToLayer("Player");

        effector.colliderMask &= ~playerLayerMask;
        gameObject.layer = 0; //LayerMask.NameToLayer("Default");

        yield return new WaitForSeconds(0.5f);

        effector.colliderMask |= playerLayerMask;
        gameObject.layer = 13; //LayerMask.NameToLayer("Platform");

        Destroy(this);
    }
}
