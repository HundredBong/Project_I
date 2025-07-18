using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SkillCharge : SkillBase
{
    public SkillCharge(SkillData skillData) : base(skillData) { }

    public override void Execute(GameObject owner)
    {
        if (owner.TryGetComponent<Player>(out Player player))
        {
            player.StateMachine.ChangeState(StateType.Charge, skillData);
        }
    }
}
