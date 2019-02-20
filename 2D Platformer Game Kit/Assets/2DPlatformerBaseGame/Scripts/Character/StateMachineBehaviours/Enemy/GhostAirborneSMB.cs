using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GhostAirborneSMB : SceneLinkedSMB<EnemyBehaviour>
{
    public override void OnSLStateNoTransitionUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        monoBehaviour.NoMovement();
        monoBehaviour.AirVerticalMovement();
        monoBehaviour.CheckGrounded();
    }
}
