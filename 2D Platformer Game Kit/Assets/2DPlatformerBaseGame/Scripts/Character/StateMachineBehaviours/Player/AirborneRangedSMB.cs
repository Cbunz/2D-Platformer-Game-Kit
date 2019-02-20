using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AirborneRangedSMB : SceneLinkedSMB<PlayerCharacter>
{
    public override void OnSLStateNoTransitionUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        monoBehaviour.UpdateFacing();
        monoBehaviour.UpdateJump();
        monoBehaviour.AirHorizontalMovement();
        monoBehaviour.AirVerticalMovement();
        monoBehaviour.CheckGrounded();
        monoBehaviour.CheckWallSlide();
        monoBehaviour.CheckForRangedAttackOut();
        if (monoBehaviour.CheckForMeleeAttackInput())
        {
            monoBehaviour.MeleeAttack();
        }
        if (monoBehaviour.shield.CheckForBoostInput())
        {
            monoBehaviour.SetAirborneDecelProportion(monoBehaviour.shield.shieldAirborneDecelProportion);
            monoBehaviour.SetMoveVector(-(new Vector2(monoBehaviour.shield.boost.x * monoBehaviour.shield.boostAmountX, monoBehaviour.shield.boost.y * monoBehaviour.shield.boostAmountY)));
            monoBehaviour.ForceFacing(Mathf.Sign(monoBehaviour.shield.boost.x) == 1);
        }
        monoBehaviour.CheckAndFireGun();
    }

    public override void OnSLStatePreExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        AnimatorStateInfo nextState = animator.GetNextAnimatorStateInfo(0);
        if (!nextState.IsTag("RangedAttack"))
        {
            monoBehaviour.ForceNotRangedAttack();
        }
    }
}
