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
        owner.player.IsDead = true;

        anim.SetTrigger("Death");
        StageManager.Instance.NotifyPlayerDead();
    }

    public void Update()
    {

    }

    public void OnExit()
    {
        anim?.SetTrigger("Undead");

        owner.player.IsDead = false;
    }
}