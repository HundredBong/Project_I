using System.Collections.Generic;
using UnityEngine;

public class ProjectileDragonBreath : Projectile
{
    private readonly List<Enemy> _enemies = new List<Enemy>(64);
    private SkillData _skillData;
    private float _timer;
    private const float TickInterval = 0.2f;

    public void Initialize(SkillData data)
    {
        _skillData = data;
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
            Debug.Log($"������ Enter, {enemy.name} : {enemy.GetInstanceID()}");

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
            Debug.Log($"������ Exit, {enemy.name} : {enemy.GetInstanceID()}");

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
