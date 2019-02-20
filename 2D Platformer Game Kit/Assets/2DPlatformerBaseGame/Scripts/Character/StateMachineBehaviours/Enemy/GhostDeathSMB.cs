using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GhostDeathSMB : SceneLinkedSMB<EnemyBehaviour>
{
    public override void OnSLStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        monoBehaviour.Death();
    }
}
