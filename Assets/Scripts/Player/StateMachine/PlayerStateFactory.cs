public class PlayerStateFactory
{
    PlayerStateMachine _context;

    public PlayerStateFactory(PlayerStateMachine currentContext)
    {
        _context = currentContext;
    }

    public PlayerBaseState EmptyHand()
    {
        return new PlayerEmptyHandState(_context, this);
    }
    public PlayerBaseState DrillHand()
    {
        return new PlayerDrillHandState(_context, this);
    }

    public PlayerBaseState EmptyHand_Idle()
    {
        return new PlayerIdleState(_context, this);
    }
    public PlayerBaseState EmptyHand_Walk()
    {
        return new PlayerWalkState(_context, this);
    }
    public PlayerBaseState EmptyHand_Flip()
    {
        return new PlayerFlipState(_context, this);
    }

    public PlayerBaseState DrillHand_Idle()
    {
        return new PlayerDrillIdleState(_context, this);
    }
    public PlayerBaseState DrillHand_Walk()
    {
        return new PlayerDrillWalkState(_context, this);
    }
}
