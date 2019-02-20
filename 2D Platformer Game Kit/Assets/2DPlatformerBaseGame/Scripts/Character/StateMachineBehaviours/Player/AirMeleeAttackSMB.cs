using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AirMeleeAttackSMB : SceneLinkedSMB<PlayerCharacter>
{
    public override void OnSLStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        monoBehaviour.ForceNotRangedAttack();
    }

    public override void OnSLStatePostEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        monoBehaviour.EnableMeleeDamager();
        if (monoBehaviour.dashWhileAirborne)
        {
            monoBehaviour.SetHorizontalMovement(monoBehaviour.meleeAttackDashSpeed * monoBehaviour.GetFacing());
        }
    }

    public override void OnSLStateNoTransitionUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        monoBehaviour.UpdateJump();
        monoBehaviour.AirHorizontalMovement();
        monoBehaviour.AirVerticalMovement();
        monoBehaviour.CheckGrounded();
        monoBehaviour.CheckWallSlide();
        if (monoBehaviour.shield.CheckForBoostInput())
        {
            monoBehaviour.SetAirborneDecelProportion(monoBehaviour.shield.shieldAirborneDecelProportion);
            monoBehaviour.SetMoveVector(-(new Vector2(monoBehaviour.shield.boost.x * monoBehaviour.shield.boostAmountX, monoBehaviour.shield.boost.y * monoBehaviour.shield.boostAmountY)));
            monoBehaviour.ForceFacing(Mathf.Sign(monoBehaviour.shield.boost.x) == 1);
        }
    }

    public override void OnSLStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        monoBehaviour.DisableMeleeDamager();
    }
}
