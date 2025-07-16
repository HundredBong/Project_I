using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ProjectileIceArrow : Projectile
{
    private Vector3 _dir;
    [SerializeField] private float _speed;
    private SkillData _skillData;

    public void Initialize(Vector3 dir, SkillData data)
    {
        _skillData = data;
        _dir = dir.normalized;
        _speed = 10f;

        DelayCallManager.Instance.CallLater(1.5f, () => { ObjectPoolManager.Instance.projectilePool.Return(this); });
    }

    private void Update()
    {
        _speed += 30 * Time.deltaTime;

        transform.position += _dir * _speed * Time.deltaTime;
    }

    protected override void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Projectile") == false)
        {
            OnHit(other.gameObject);
        }
    }

    protected override void OnHit(GameObject other)
    {
        float damage = SkillManager.Instance.CalculateSkillDamage(_skillData);

        if (other.TryGetComponent<Enemy>(out Enemy enemy))
        {
            ObjectPoolManager.Instance.particlePool.GetPrefab(ParticleId.IceArrow).Play(enemy.transform.position);
            enemy.TakeDamage(damage);
        }
    }
}
