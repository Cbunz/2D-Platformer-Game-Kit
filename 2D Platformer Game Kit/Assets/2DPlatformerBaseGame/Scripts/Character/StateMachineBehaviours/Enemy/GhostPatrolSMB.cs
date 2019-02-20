using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GhostPatrolSMB : SceneLinkedSMB<EnemyBehaviour>
{
    public override void OnSLStateNoTransitionUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        monoBehaviour.CheckGrounded();

        float dist = monoBehaviour.speed;

        if (monoBehaviour.CheckForObstacle(dist) || (monoBehaviour.usePatrolBorders && !monoBehaviour.CheckWithinPatrolBorders()))
            monoBehaviour.TurnAround(dist);
        else
            monoBehaviour.SetHorizontalSpeed(dist);
    }

    public override void OnSLStatePreExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        monoBehaviour.NoMovement();
    }
}
