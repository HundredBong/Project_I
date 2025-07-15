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
        //base.OnTriggerEnter2D(other); 의도적으로 비워둠

        if (other.CompareTag("Projectile") == false)
        {
            OnHit(other.gameObject);
        }
    }

    protected override void OnHit(GameObject other)
    {
        if (other.TryGetComponent<Enemy>(out Enemy enemy))
        {
            Debug.Log($"디버깅용 Enter, {enemy.name} : {enemy.GetInstanceID()}");

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
            Debug.Log($"디버깅용 Exit, {enemy.name} : {enemy.GetInstanceID()}");

            //참조 비교
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

        //누적 오차 방지
        _timer -= TickInterval;

        ApplyDamage();
    }

    private void ApplyDamage()
    {
        //중간에 스킬 레벨업 등으로 대미지가 바뀔 수 있으니 TakeDamage메서드 호출 전 계산

        if (_skillData == null) { return; }

        float damage = SkillManager.Instance.CalculateSkillDamage(_skillData);

        //foreach문으로 순회하다가 Remove호출하면 터질 수 있으니 역방향 for문으로 순회
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
