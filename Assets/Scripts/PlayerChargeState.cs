using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class PlayerChargeState : IState
{
    private readonly PlayerStateMachine _owner;
    private SpriteRenderer[] _sprites;
    private Animator _anim;
    private int _hitCount;
    private SkillData _skillData;
    private List<Enemy> _enemies = new List<Enemy>(16);
    private Collider2D[] _buffer = new Collider2D[16];
    private PooledParticle _par;

    public PlayerChargeState(PlayerStateMachine owner)
    {
        _owner = owner;
        _anim = owner.player.Animator;
        _sprites = owner.player.SpriteRenderers;

    }

    public void OnEnter()
    {
        _skillData = _owner.CurrentSkillData;
        _enemies.Clear();
        _anim.SetBool("1_Move", true);
        _hitCount = 0;
        _par = ObjectPoolManager.Instance.particlePool.GetPrefab(ParticleId.ChargeLoop);
        _par.Play(_owner.transform.position,false);

        foreach (var sprite in _sprites)
        {
            sprite.enabled = false;
        }

        int count = Physics2D.OverlapCircleNonAlloc(_owner.transform.position, _skillData.Range, _buffer, SkillManager.Instance.targetMask);

        int limit = Mathf.Min(count, _skillData.TargetCount);

        for (int i = 0; i < limit; i++)
        {
            Collider2D col = _buffer[i];

            if (col.TryGetComponent<Enemy>(out Enemy enemy))
            {
                _enemies.Add(enemy);
            }
        }

        _enemies = _enemies.OrderBy(e => Vector3.Distance(_owner.transform.position, e.transform.position)).ToList();
    }

    public void OnExit()
    {
        _par.Stop();
        _par = null;

        _anim.SetBool("1_Move", false);

        foreach (var sprite in _sprites)
        {
            sprite.enabled = true;
        }
    }

    public void Update()
    {
        if (_owner.player.Stat.health <= 0)
        {
            //_owner.ChangeState(StateType.Dead);
        }

        if (_hitCount >= _enemies.Count)
        {
            _owner.ChangeState(StateType.Idle);
            return;
        }

        Charge();
    }

    private void Charge()
    {
        if (_par != null) { _par.transform.position = _owner.transform.position; }

        Enemy target = _enemies[_hitCount];

        if (target == null || target.isDead)
        {
            _hitCount++;
            return;
        }

        Vector3 dir = (target.transform.position - _owner.transform.position).normalized;
        _owner.transform.position += dir * 15f * Time.deltaTime;

        float distance = Vector3.Distance(_owner.transform.position, target.transform.position);

        if (distance <= 0.5f)
        {
            target.TakeDamage(SkillManager.Instance.CalculateSkillDamage(_skillData));
            ObjectPoolManager.Instance.particlePool.GetPrefab(ParticleId.ChargeHit).Play(target.transform.position);

            _hitCount++;
        }

    }
}
