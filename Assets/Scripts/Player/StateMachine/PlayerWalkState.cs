using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerWalkState : PlayerBaseState
{
    public PlayerWalkState(PlayerStateMachine currentContext, PlayerStateFactory playerStateFactory) : base(currentContext, playerStateFactory)
    {
        IsRootState = false;
        //InitialiseSubState(); Not a super state (i.e "grounded" -> walk, run, idle. OR: "digging" -> slow, fast)
    }

    public override void EnterState()
    {
        Debug.Log("Walking");

        // TODO: switch to network animator
        Ctx.Anim.CrossFade(Ctx.AnimWalkHash, 0, 0);
    }

    public override void UpdateState()
    {
        CheckSwitchState();

        Ctx.MoveVector = Ctx.MoveInput * Ctx.MoveSpeed;
        if (Ctx.MoveInput.x != 0 || Ctx.MoveInput.y != 0)
        {
            Ctx.DirectionVector = Ctx.MoveInput;
        }
    }

    public override void CheckSwitchState()
    {
        if (!Ctx.IsMovementPressed)
        {
            SwitchState(Factory.EmptyHand_Idle());
        }
        if (Vector2.Angle(Ctx.DirectionVector, Ctx.MoveInput) >= 120f)
        {
            SwitchState(Factory.EmptyHand_Flip());
        }
    }

    public override void ExitState() { }

    public override void InitialiseSubState() { }
}
