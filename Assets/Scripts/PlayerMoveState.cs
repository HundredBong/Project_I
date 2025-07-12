using UnityEngine;

public class PlayerMoveState : IState
{
    private PlayerStateMachine owner;
    private Animator anim;

    public PlayerMoveState(PlayerStateMachine owner)
    {
        this.owner = owner;
    }

    public void OnEnter()
    {
        anim = owner.player.Animator;
    }

    public void Update()
    {

    }

    public void OnExit()
    {

    }
}