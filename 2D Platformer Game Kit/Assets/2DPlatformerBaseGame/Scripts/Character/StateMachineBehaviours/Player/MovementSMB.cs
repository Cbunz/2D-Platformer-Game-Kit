using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MovementSMB : SceneLinkedSMB<PlayerCharacter>
{
    public override void OnSLStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        monoBehaviour.TeleportToColliderBottom();
    }

    public override void OnSLStateNoTransitionUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        monoBehaviour.UpdateFacing();
        monoBehaviour.GroundHorizontalMovement(true);
        monoBehaviour.GroundVerticalMovement();
        monoBehaviour.CheckGrounded();
        monoBehaviour.CheckForPushing();
        monoBehaviour.CheckForRangedAttackOut();
        monoBehaviour.CheckAndFireGun();
        if (monoBehaviour.shield.CheckForBoostInput())
        {
            monoBehaviour.SetAirborneDecelProportion(monoBehaviour.shield.shieldAirborneDecelProportion, true);
            monoBehaviour.SetMoveVector(-(new Vector2(monoBehaviour.shield.boost.x * monoBehaviour.shield.boostAmountX, monoBehaviour.shield.boost.y * monoBehaviour.shield.boostAmountY)));
            monoBehaviour.ForceFacing(Mathf.Sign(monoBehaviour.shield.boost.x) == 1);
        }
        else if (monoBehaviour.CheckForFallInput())
            monoBehaviour.MakePlatformFallThrough();
        else if (monoBehaviour.CheckForJumpInput())
            monoBehaviour.SetVerticalMovement(monoBehaviour.jumpSpeed);
        else if (monoBehaviour.CheckForMeleeAttackInput())
            monoBehaviour.MeleeAttack();
    }
}