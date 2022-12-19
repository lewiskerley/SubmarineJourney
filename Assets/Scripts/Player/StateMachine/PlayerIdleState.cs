using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerIdleState : PlayerBaseState
{
    public PlayerIdleState(PlayerStateMachine currentContext, PlayerStateFactory playerStateFactory) : base(currentContext, playerStateFactory)
    {
        //_isRootState = true; IF this was a root state
        //InitialiseSubState(); Not a super state (i.e "grounded" -> walk, run, idle. OR: "digging" -> slow, fast)
    }

    public override void EnterState()
    {
        Debug.Log("Idle");

        // TODO: switch to network animator
        Ctx.Anim.CrossFade(Ctx.AnimIdleHash, 1, 0);

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
            SwitchState(Factory.Walk());
        }
    }

    public override void ExitState() { }

    public override void InitialiseSubState() { }
}
