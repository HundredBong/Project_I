
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ProjectileFireball : Projectile
{
    private Enemy _targetEnemy;
    private SkillData _skillData;

    private Vector3 _dir;
    //private bool _hasRetarget;
    private float _speed = 10f;
    //private float _turnRate = 25f;
    private Collider2D[] _buffer = new Collider2D[32];

    public void SetTarget(SkillData data, Enemy enemy, GameObject owner)
    {
        _skillData = data;
        _targetEnemy = enemy;
        transform.position = owner.transform.position + new Vector3(Random.Range(-3f, 3f), Random.Range(-3f, 3f), 0);

        _dir = (_targetEnemy.transform.position - transform.position).normalized;
        //_hasRetarget = false;
    }

    private void Update()
    {
        //if (_targetEnemy == null || _targetEnemy.isDead)
        //{

        //    ObjectPoolManager.Instance.projectilePool.Return(this);
        //    //if (_hasRetarget == false)
        //    //{
        //    //    _targetEnemy = FindNewTarget();
        //    //    _hasRetarget = true;
        //    //}

        //    //if (_targetEnemy == null)
        //    //{
        //    //    DelayCallManager.Instance.CallLater(1f, () => { ObjectPoolManager.Instance.projectilePool.Return(this); });
        //    //    return;
        //    //}
        //}

        //Vector3 toTarget = (_targetEnemy.transform.position - transform.position).normalized;
        //_dir = Vector3.Slerp(_dir, toTarget, _turnRate * Time.deltaTime).normalized;

        transform.position += _dir * _speed * Time.deltaTime;
    }

    private Enemy FindNewTarget()
    {
        int count = Physics2D.OverlapCircleNonAlloc(transform.position, _skillData.Range, _buffer, SkillManager.Instance.targetMask);


        float shortestDistance = float.MaxValue;
        Enemy closest = null;

        for (int i = 0; i < count; i++)
        {
            if (_buffer[i].TryGetComponent(out Enemy enemy) && enemy.isDead == false)
            {
                float distance = Vector2.Distance(transform.position, enemy.transform.position);

                if (distance < shortestDistance)
                {
                    shortestDistance = distance;
                    closest = enemy;
                }
            }
        }

        return closest;
    }

    protected override void OnTriggerEnter2D(Collider2D other)
    {
        if (other.TryGetComponent<Enemy>(out Enemy enemy))
        {
            OnHit(enemy);
        }
    }

    private void OnHit(Enemy enemy)
    {

        float damage = SkillManager.Instance.CalculateSkillDamage(_skillData);

        for (int i = 0; i < _skillData.HitCount; i++)
        {
            enemy.TakeDamage(damage);
        }

        ObjectPoolManager.Instance.particlePool.GetPrefab(ParticleId.Fireball).Play(enemy.transform.position);
        DelayCallManager.Instance.CallLater(1f, () => { ObjectPoolManager.Instance.projectilePool.Return(this); });
    }
}
