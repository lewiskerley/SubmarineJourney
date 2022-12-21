using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerFlipState : PlayerBaseState
{
    public PlayerFlipState(PlayerStateMachine currentContext, PlayerStateFactory playerStateFactory) : base(currentContext, playerStateFactory)
    {
        IsRootState = true;
        //InitialiseSubState(); Not a super state (i.e "grounded" -> walk, run, idle. OR: "digging" -> slow, fast)
    }

    public override void EnterState()
    {
        Debug.Log("Flipping");

        Ctx.DirectionVector = Ctx.MoveInput;

        // TODO: switch to network animator
        Ctx.Anim.CrossFade(Ctx.AnimFlipHash, 0, 0);
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

    public override bool CheckSwitchState()
    {
        AnimatorStateInfo animStateInfo = Ctx.Anim.GetCurrentAnimatorStateInfo(0);
        float NTime = animStateInfo.normalizedTime;

        if (NTime > 1.0f)
        {
            if (!Ctx.IsMovementPressed)
            {
                SwitchState(Factory.Idle());
            }
            else
            {
                SwitchState(Factory.Walk());
            }
        }
        return false;
    }

    public override void ExitState() { }

    public override void InitialiseSubState() { }
}
