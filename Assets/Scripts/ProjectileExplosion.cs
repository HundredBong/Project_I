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
        const int maxTryPerBurst = 30; //최대 시도 횟수
        const float minDist = 1.2f; //폭발 간격

        while (spawned < _skillData.TargetCount && tryCount < maxTryPerBurst * _skillData.TargetCount)
        {
            yield return _delay;

            tryCount++;

            //플레이어 주변에 랜덤한 위치 초기화
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

            //폭발 위치에서 파티클 재생 및 타겟을 찾고 대미지 처리
            ObjectPoolManager.Instance.particlePool.GetPrefab(ParticleId.Explosion).Play(spawnPos);
            FindTarget(spawnPos);

            _placed.Add(spawnPos);
            spawned++;
        }

        if (spawned < _skillData.TargetCount)
        {
            Debug.LogWarning("[SkillExplosion] 최소 간격 때문에 일부 폭발 생략됨");
        }

        ObjectPoolManager.Instance.projectilePool.Return(this);
    }

    private void FindTarget(Vector2 pos)
    {
        //폭발 위치를 중심으로 타겟을 찾고 리스트에 넣음
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
