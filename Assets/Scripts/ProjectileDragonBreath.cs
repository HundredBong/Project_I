using System.Collections.Generic;
using UnityEngine;

public class ProjectileDragonBreath : Projectile
{
    private readonly List<Enemy> _enemies = new List<Enemy>(64);
    private SkillData _skillData;
    private float _timer;
    private const float TickInterval = 0.2f;

    private Vector3 originScale;

    private void Awake()
    {
        originScale = transform.localScale;
    }

    public void Initialize(GameObject owner, SkillData data)
    {
        _skillData = data;

        bool isFlip = owner.transform.localScale.x < 0f;

        Vector3 dir = isFlip ? Vector3.right : Vector3.left;
        Vector3 spawnPos = owner.transform.position + dir;
        Quaternion rot = Quaternion.FromToRotation(Vector3.right, dir);

        transform.position = spawnPos;
        transform.rotation = rot;

        Vector3 flipScale = new Vector3(-1f, 1f, 1f);

        transform.localScale = isFlip ? Vector3.Scale(originScale, flipScale) : originScale;

        PooledParticle par = ObjectPoolManager.Instance.particlePool.GetPrefab(ParticleId.DragonBreath);
        par.transform.localScale = isFlip ? flipScale : Vector3.one;
        par.Play(transform.position);

        DelayCallManager.Instance.CallLater(2f, () => { ObjectPoolManager.Instance.projectilePool.Return(this); });
    }

    private void OnEnable()
    {
        _timer = 0f;
    }

    private void OnDisable()
    {
        _enemies.Clear();
    }


    protected override void OnTriggerEnter2D(Collider2D other)
    {
        //base.OnTriggerEnter2D(other); �ǵ������� �����

        if (other.CompareTag("Projectile") == false)
        {
            OnHit(other.gameObject);
        }
    }

    protected override void OnHit(GameObject other)
    {
        if (other.TryGetComponent<Enemy>(out Enemy enemy))
        {
            //Debug.Log($"������ Enter, {enemy.name} : {enemy.GetInstanceID()}");

            if (_enemies.Contains(enemy) == false)
            {
                _enemies.Add(enemy);
            }
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.TryGetComponent<Enemy>(out Enemy enemy))
        {
            //Debug.Log($"������ Exit, {enemy.name} : {enemy.GetInstanceID()}");

            //���� ��
            _enemies.Remove(enemy);
        }
    }

    private void Update()
    {
        _timer += Time.deltaTime;

        if (_timer < TickInterval)
        {
            return;
        }

        //���� ���� ����
        _timer -= TickInterval;

        ApplyDamage();
    }

    private void ApplyDamage()
    {
        //�߰��� ��ų ������ ������ ������� �ٲ� �� ������ TakeDamage�޼��� ȣ�� �� ���

        if (_skillData == null) { return; }

        float damage = SkillManager.Instance.CalculateSkillDamage(_skillData);

        //foreach������ ��ȸ�ϴٰ� Removeȣ���ϸ� ���� �� ������ ������ for������ ��ȸ
        for (int i = _enemies.Count - 1; i >= 0; i--)
        {
            Enemy enemy = _enemies[i];

            if (enemy != null)
            {
                enemy.TakeDamage(damage);

                if (enemy.isDead)
                {
                    _enemies.RemoveAt(i);
                }
            }
        }
    }
}
