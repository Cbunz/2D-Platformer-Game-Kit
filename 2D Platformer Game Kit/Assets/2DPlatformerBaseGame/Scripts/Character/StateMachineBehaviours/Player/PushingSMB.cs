using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PushingSMB : SceneLinkedSMB<PlayerCharacter>
{
    public override void OnSLStatePostEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        monoBehaviour.ForceNotRangedAttack();
        monoBehaviour.StartPushing();
    }

    public override void OnSLStateNoTransitionUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        monoBehaviour.UpdateFacing();
        monoBehaviour.GroundHorizontalMovement(true, monoBehaviour.pushingSpeedProportion);
        monoBehaviour.GroundVerticalMovement();
        monoBehaviour.CheckGrounded();
        monoBehaviour.CheckForPushing();
        monoBehaviour.MovePushable();
        if (monoBehaviour.shield.CheckForBoostInput())
        {
            monoBehaviour.SetAirborneDecelProportion(monoBehaviour.shield.shieldAirborneDecelProportion);
            monoBehaviour.SetMoveVector(-(new Vector2(monoBehaviour.shield.boost.x * monoBehaviour.shield.boostAmountX, monoBehaviour.shield.boost.y * monoBehaviour.shield.boostAmountY)));
            monoBehaviour.ForceFacing(Mathf.Sign(monoBehaviour.shield.boost.x) == 1);
        }
        monoBehaviour.CheckForJumpInput();
    }

    public override void OnSLStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {

    }
}
