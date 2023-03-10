using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerIdleState : PlayerBaseState
{
    public PlayerIdleState(PlayerStateMachine currentContext, PlayerStateFactory playerStateFactory) : base(currentContext, playerStateFactory)
    {
        IsRootState = false;
        //InitialiseSubState(); Not a super state (i.e "grounded" -> walk, run, idle. OR: "digging" -> slow, fast)
    }

    public override void EnterState()
    {
        Debug.Log("Idle");

        // TODO: switch to network animator
        Ctx.Anim.CrossFade(Ctx.AnimIdleHash, 0, 0);

        Ctx.MoveVector = Vector2.zero;
    }

    public override void UpdateState()
    {
        CheckSwitchState();
    }

    public override void CheckSwitchState()
    {
        if (Ctx.IsMovementPressed)
        {
            if (Vector2.Angle(Ctx.DirectionVector, Ctx.MoveInput) >= 120f)
            {
                SwitchState(Factory.EmptyHand_Flip());
            }
            else
            {
                SwitchState(Factory.EmptyHand_Walk());
            }
        }
    }

    public override void ExitState() { }

    public override void InitialiseSubState() { }
}
