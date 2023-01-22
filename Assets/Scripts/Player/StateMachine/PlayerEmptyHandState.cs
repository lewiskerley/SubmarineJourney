using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerEmptyHandState : PlayerBaseState
{
    public PlayerEmptyHandState(PlayerStateMachine currentContext, PlayerStateFactory playerStateFactory) : base(currentContext, playerStateFactory)
    {
        IsRootState = true;
        InitialiseSubState();
    }

    public override void EnterState()
    {
        Debug.Log("Hand: Empty");
    }

    public override void UpdateState()
    {
        CheckSwitchState();
    }

    public override void CheckSwitchState()
    {
        if (Ctx.EquippedDrill)
        {
            SwitchState(Factory.DrillHand());
        }
    }

    public override void ExitState() { }

    public override void InitialiseSubState()
    {
        if (!Ctx.IsMovementPressed)
        {
            SetSubStateAndEnter(Factory.EmptyHand_Idle());
        }
        else
        {
            if (Vector2.Angle(Ctx.DirectionVector, Ctx.MoveInput) >= 120f)
            {
                SetSubStateAndEnter(Factory.EmptyHand_Flip());
            }
            else
            {
                SetSubStateAndEnter(Factory.EmptyHand_Walk());
            }
        }
    }
}
