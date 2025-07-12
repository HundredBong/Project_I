using UnityEngine;

public class PlayerAttackState : IState
{
    private PlayerStateMachine owner;
    private Animator anim;

    public PlayerAttackState(PlayerStateMachine owner)
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