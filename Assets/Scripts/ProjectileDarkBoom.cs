using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ProjectileDarkBoom : Projectile
{
    [SerializeField] private ParticleSystem par;

    private SkillData skillData;
    private readonly Collider2D[] _buffer = new Collider2D[64];

    private void Awake()
    {
        if (par == null)
        {
            par = GetComponent<ParticleSystem>();

            if (par == null)
            {
                Debug.LogError("[ProjectileDarkBoom] 파티클을 찾을 수 없음");
            }
        }
    }

    public void Initialize(SkillData data, GameObject owner)
    {
        this.skillData = data;

        transform.position = owner.transform.position + new Vector3(0, 0.5f, 0);

        float duration = par.main.duration;

        //파티클 재생후 풀에 되돌리고
        DelayCallManager.Instance.CallLater(duration, () => { ObjectPoolManager.Instance.projectilePool.Return(this); });
        DelayCallManager.Instance.CallLater(duration - 0.1f, () => { Explode(); });
    }

    private void Explode()
    {
        int awakenLevel = SkillManager.Instance.GetSkillState(skillData.SkillId).AwakenLevel;
        int count = Physics2D.OverlapCircleNonAlloc(this.transform.position, skillData.Range + (skillData.Range * 0.25f * awakenLevel),
            _buffer, SkillManager.Instance.targetMask);
        float damage = SkillManager.Instance.CalculateSkillDamage(skillData);

        for (int i = 0; i < count; i++)
        {
            Collider2D col = _buffer[i];

            if (col != null && col.TryGetComponent<Enemy>(out Enemy enemy) && enemy.isDead == false)
            {
                for (int k = 0; k < skillData.HitCount; k++)
                {
                    enemy.TakeDamage(damage);
                }
                ObjectPoolManager.Instance.particlePool.GetPrefab(ParticleId.DarkBoom).Play(enemy.transform.position);
            }
        }

        ObjectPoolManager.Instance.audioPool.GetAudio().PlaySFX("Skill_DarkBoom_Explode");
    }
}
