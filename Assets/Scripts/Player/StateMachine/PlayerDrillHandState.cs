using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerDrillHandState : PlayerBaseState
{
    public PlayerDrillHandState(PlayerStateMachine currentContext, PlayerStateFactory playerStateFactory) : base(currentContext, playerStateFactory)
    {
        IsRootState = true;
        InitialiseSubState();
    }

    public override void EnterState()
    {
        Debug.Log("Hand: Drill");
    }

    public override void UpdateState()
    {
        CheckSwitchState();

        Ctx.UseEquipmentUpdate();
    }

    public override void CheckSwitchState()
    {
        if (Ctx.EquippedEmpty)
        {
            SwitchState(Factory.EmptyHand());
        }
    }

    public override void ExitState() { }

    public override void InitialiseSubState()
    {
        if (!Ctx.IsMovementPressed)
        {
            SetSubStateAndEnter(Factory.DrillHand_Idle());
        }
        else
        {
            SetSubStateAndEnter(Factory.DrillHand_Walk());
        }
    }
}
