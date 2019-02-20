using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MeleeAttackSMB : SceneLinkedSMB<PlayerCharacter>
{
    int hashAirMeleeAttackState = Animator.StringToHash("AirMeleeAttack");

    public override void OnSLStatePostEnter(Animator animator, AnimatorStateInfo stateIfno, int layerIndex)
    {
        monoBehaviour.ForceNotRangedAttack();
        monoBehaviour.EnableMeleeDamager();
        monoBehaviour.SetHorizontalMovement(monoBehaviour.meleeAttackDashSpeed * monoBehaviour.GetFacing());
    }

    public override void OnSLStateNoTransitionUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        if (!monoBehaviour.CheckGrounded())
        {
            animator.Play(hashAirMeleeAttackState, layerIndex, stateInfo.normalizedTime);
        }

        monoBehaviour.GroundHorizontalMovement(true);

        if (monoBehaviour.shield.CheckForBoostInput())
        {
            monoBehaviour.SetAirborneDecelProportion(monoBehaviour.shield.shieldAirborneDecelProportion, true);
            monoBehaviour.SetMoveVector(-(new Vector2(monoBehaviour.shield.boost.x * monoBehaviour.shield.boostAmountX, monoBehaviour.shield.boost.y * monoBehaviour.shield.boostAmountY)));
            monoBehaviour.ForceFacing(Mathf.Sign(monoBehaviour.shield.boost.x) == 1);
        }
    }

    public override void OnSLStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        monoBehaviour.DisableMeleeDamager();
    }
}
