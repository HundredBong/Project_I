using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ProjectileIceArrow : Projectile
{
    private Vector3 _dir;
    [SerializeField] private float _speed;
    private SkillData _skillData;
    [SerializeField] private ParticleSystem _par;

    [SerializeField]
    private bool _GCTest;
    private float _elapsed;
    private const float LIFE_TIME = 1.5f;

    public void Initialize(Vector3 dir, SkillData data)
    {
        _elapsed = 0f;

        _skillData = data;
        _dir = dir.normalized;
        _speed = 10f;
    }

    private void Update()
    {
        _speed += 30 * Time.deltaTime;

        transform.position += _dir * _speed * Time.deltaTime;

        _elapsed += Time.deltaTime;

        if (LIFE_TIME < _elapsed)
        {
            ObjectPoolManager.Instance.projectilePool.Return(this);
        }
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
            if (_GCTest)
            {
                ParticleSystem par = Instantiate(_par);
                par.transform.position = enemy.transform.position;
                par.Play();
                float duration = par.main.duration;
                Destroy(par.gameObject, duration);
            }
            else
            {
                ObjectPoolManager.Instance.particlePool.GetPrefab(ParticleId.IceArrow).Play(enemy.transform.position, false);
            }
            enemy.TakeDamage(damage);
        }
    }
}
