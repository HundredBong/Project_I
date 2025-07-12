using UnityEngine;

public class PlayerDeadState : IState
{
    private PlayerStateMachine owner;
    private Animator anim;

    public PlayerDeadState(PlayerStateMachine owner)
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