using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ProjectileExplosion : Projectile
{
    private SkillData _skillData;
    private List<Vector2> _placed = new List<Vector2>(64);
    private Coroutine spawnCoroutine;
    private Collider2D[] _buffer = new Collider2D[32];
    private readonly WaitForSeconds _delay = new WaitForSeconds(0.05f);

    private Vector2 temp;

    public void Initaialize(SkillData data, GameObject owner)
    {
        _skillData = data;
        _placed.Clear();

        if (spawnCoroutine != null)
        {
            StopCoroutine(spawnCoroutine);
            spawnCoroutine = null;
        }

        spawnCoroutine = StartCoroutine(SpawnCoroutine(owner));
    }

    private IEnumerator SpawnCoroutine(GameObject owner)
    {
        int spawned = 0;
        int tryCount = 0;
        const int maxTryPerBurst = 30; //�ִ� �õ� Ƚ��
        const float minDist = 1.2f; //���� ����

        while (spawned < _skillData.TargetCount && tryCount < maxTryPerBurst * _skillData.TargetCount)
        {
            yield return _delay;

            tryCount++;

            //�÷��̾� �ֺ��� ������ ��ġ �ʱ�ȭ
            Vector2 spawnPos = (Vector2)owner.transform.position + UnityEngine.Random.insideUnitCircle * _skillData.Range;

            bool tooClose = false;

            foreach (Vector2 place in _placed)
            {
                if (Vector2.SqrMagnitude(spawnPos - place) < minDist * minDist)
                {
                    tooClose = true;
                    break;
                }
            }

            if (tooClose)
            {
                continue;
            }

            //���� ��ġ���� ��ƼŬ ��� �� Ÿ���� ã�� ����� ó��
            ObjectPoolManager.Instance.particlePool.GetPrefab(ParticleId.Explosion).Play(spawnPos);
            FindTarget(spawnPos);

            _placed.Add(spawnPos);
            spawned++;
        }

        if (spawned < _skillData.TargetCount)
        {
            Debug.LogWarning("[SkillExplosion] �ּ� ���� ������ �Ϻ� ���� ������");
        }

        ObjectPoolManager.Instance.projectilePool.Return(this);
    }

    private void FindTarget(Vector2 pos)
    {
        //���� ��ġ�� �߽����� Ÿ���� ã�� ����Ʈ�� ����
        temp = pos;
        int count = Physics2D.OverlapCircleNonAlloc(pos, _skillData.Range * 0.33f, _buffer, SkillManager.Instance.targetMask);

        List<Enemy> enemyList = new List<Enemy>(64);

        for (int i = 0; i < count; i++)
        {
            Collider2D col = _buffer[i];

            if (col.TryGetComponent<Enemy>(out Enemy enemy))
            {
                enemyList.Add(enemy);
            }
        }

        float damage = SkillManager.Instance.CalculateSkillDamage(_skillData);

        foreach (Enemy enemy in enemyList)
        {
            for (int i = 0; i < _skillData.HitCount; i++)
            {
                if (enemy != null && enemy.isDead == false)
                {
                    enemy.TakeDamage(damage);
                }
            }
        }
    }

    private void OnDrawGizmos()
    {
        if (_skillData != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(temp, _skillData.Range * 0.33f);
        }

    }
}
