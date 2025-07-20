using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Transactions;
using UnityEngine;

public class ProjectileLightning : Projectile
{
    //SkillLightning�� skillData�� �����
    private SkillData skillData;

    private bool isInitialized = false;
    private bool hasHit = false;

    private void OnEnable()
    {
        hasHit = false;

        if (isInitialized)
        {
            DelayCallManager.Instance.CallLater(3f, () => { ObjectPoolManager.Instance.projectilePool.Return(this); });
        }
    }

    public void Initialize(SkillData data, GameObject target)
    {
        skillData = data;

        transform.position = target.transform.position + Vector3.up * 10f;

        isInitialized = true;
    }

    private void Update()
    {
        //�Ʒ��� ���������� �̵����� ����
        if (isInitialized == true)
        {
            transform.position += Vector3.down * Time.deltaTime * 75f;
        }
    }

    protected override void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Projectile") == false)
        {
            OnHit(other.gameObject);
            DelayCallManager.Instance.CallLater(0.05f, () => { ObjectPoolManager.Instance.projectilePool.Return(this); });
        }
    }

    protected override void OnHit(GameObject other)
    {
        //Debug.Log($"[ProjectileLightning] ��Ʈ��, {other.gameObject.name}");

        if (other.TryGetComponent<Enemy>(out Enemy enemy) == true && hasHit == false)
        {
            //��ƼŬ ���
            ObjectPoolManager.Instance.particlePool.GetPrefab(ParticleId.Lightning).Play(other.transform.position);

            //�÷��̾� ��ų ���� ��������
            PlayerSkillState state = SkillManager.Instance.GetSkillState(skillData.SkillId);

            //�÷��̾� ����� * (baseValue / 100) => �÷��̾� ����� * 1.3
            float playerDamage = GameManager.Instance.stats.damage;
            float damage = playerDamage * ((skillData.BaseValue + (skillData.BaseValueIncrease * state.Level)) / 100f);

            float shockChance = skillData.StatucChance;


            for (int i = 0; i < skillData.HitCount; i++)
            {
                enemy.TakeDamage(damage);
                //Debug.Log($"[ProjectileLightning] Enemy���� {damage}�� ���ظ� ����");
            }

            if (Random.value < shockChance / 100f)
            {
                //�����̻� ���� ����, ���ڷ� StatusEffectType �������ֱ�
            }

            hasHit = true;
        }
    }


}
