using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerWalkState : PlayerBaseState
{
    public PlayerWalkState(PlayerStateMachine currentContext, PlayerStateFactory playerStateFactory) : base(currentContext, playerStateFactory)
    {
        //_isRootState = true; IF this was a root state
        //InitialiseSubState(); Not a super state (i.e "grounded" -> walk, run, idle. OR: "digging" -> slow, fast)
    }

    public override void EnterState()
    {
        Debug.Log("Walking");

        // TODO: switch to network animator
        Ctx.Anim.CrossFade(Ctx.AnimWalkHash, 1, 0);
    }

    public override void UpdateState()
    {
        CheckSwitchState();
        Ctx.MoveVector = Ctx.MoveInput * Ctx.MoveSpeed;
    }

    public override void CheckSwitchState()
    {
        if (!Ctx.IsMovementPressed)
        {
            SwitchState(Factory.Idle());
        }
    }

    public override void ExitState() { }

    public override void InitialiseSubState() { }
}
