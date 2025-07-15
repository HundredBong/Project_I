using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ProjectileDarkBoom : Projectile
{
    [SerializeField] private ParticleSystem par;

    private SkillData skillData;

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
        Collider2D[] colliders = Physics2D.OverlapCircleAll(this.transform.position, skillData.Range, SkillManager.Instance.targetMask);

        float damage = SkillManager.Instance.CalculateSkillDamage(skillData);

        foreach (Collider2D col in colliders)
        {
            if (col.TryGetComponent<Enemy>(out Enemy enemy) && enemy.isDead == false)
            {
                for (int i = 0; i < skillData.HitCount; i++)
                {
                    enemy.TakeDamage(damage);
                }
                ObjectPoolManager.Instance.particlePool.GetPrefab(ParticleId.DarkBoom).Play(enemy.transform.position);
            }
        }
    }
}
