using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WallSlideSMB : SceneLinkedSMB<PlayerCharacter>
{
    public override void OnSLStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        monoBehaviour.SetWallSlideTimeout();
    }

    public override void OnSLStateNoTransitionUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        monoBehaviour.SetAirborneDecelProportion(monoBehaviour.wallSlideAirborneDecelProportion);
        monoBehaviour.WallSlideTimeout();
        monoBehaviour.UpdateFacing();
        monoBehaviour.AirHorizontalMovement();
        monoBehaviour.CheckGrounded();
        monoBehaviour.CheckWallSlide();
        if (monoBehaviour.IsRising())
            monoBehaviour.WallSlideUpVerticalMovement();
        else
            monoBehaviour.WallSlideDownVerticalMovement();

        if (monoBehaviour.CheckForJumpInput() && monoBehaviour.TouchingWall())
        {
            Vector2 jumpVector;
            if (monoBehaviour.GetFacing() == -1)
            {
                jumpVector = new Vector2(monoBehaviour.wallSlideJumpX, monoBehaviour.wallSlideJumpY);
                monoBehaviour.ForceFacing(false);
            }
            else
            {
                jumpVector = new Vector2(-monoBehaviour.wallSlideJumpX, monoBehaviour.wallSlideJumpY);
                monoBehaviour.ForceFacing(true);
            }
            monoBehaviour.SetMoveVector(jumpVector);
            monoBehaviour.ForceNotWallSlide();
        }
        else if (monoBehaviour.shield.CheckForBoostInput())
        {
            monoBehaviour.SetAirborneDecelProportion(monoBehaviour.shield.shieldAirborneDecelProportion);
            monoBehaviour.SetMoveVector(-(new Vector2(monoBehaviour.shield.boost.x * monoBehaviour.shield.boostAmountX, monoBehaviour.shield.boost.y * monoBehaviour.shield.boostAmountY)));
            monoBehaviour.ForceFacing(Mathf.Sign(monoBehaviour.shield.boost.x) == 1);
        }
    }

    public override void OnSLStatePreExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        monoBehaviour.ForceNotWallSlide();
    }
}