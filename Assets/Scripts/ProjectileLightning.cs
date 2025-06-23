using System.Collections;
using System.Collections.Generic;
using System.Linq;
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

        DelayCallManager.Instance.CallLater(3f, () => { ObjectPoolManager.Instance.projectilePool.Return(this); });
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
            transform.position += Vector3.down * Time.deltaTime * 50f;
        }
    }

    protected override void OnHit(GameObject other)
    {
        Debug.Log($"[ProjectileLightning] ��Ʈ��, {other.gameObject.name}");

        if (other.TryGetComponent<Enemy>(out Enemy enemy) == true && hasHit == false)
        {
            //�÷��̾� ��ų ���� ��������
            PlayerSkillState state = SkillManager.Instance.GetSkillState(skillData.SkillId);
            float damage = skillData.BaseValue + (skillData.BaseValueIncrease * state.Level);
            float shockChance = skillData.StatucChance;


            for (int i = 0; i < skillData.HitCount; i++)
            {
                enemy.TakeDamage(damage);
                Debug.Log($"[ProjectileLightning] Enemy���� {damage}�� ���ظ� ����");
            }

            if (Random.value < shockChance / 100f)
            {
                //�����̻� ���� ����, ���ڷ� StatusEffectType �������ֱ�
            }

            hasHit = true;
        }
    }


}
