using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class ProjectileLightning : Projectile
{
    //SkillLightning의 skillData를 사용함
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
        //아래로 떨어지도록 이동로직 구현
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
        Debug.Log($"[ProjectileLightning] 히트함, {other.gameObject.name}");

        if (other.TryGetComponent<Enemy>(out Enemy enemy) == true && hasHit == false)
        {
            //플레이어 스킬 정보 가져오기
            PlayerSkillState state = SkillManager.Instance.GetSkillState(skillData.SkillId);
            float damage = skillData.BaseValue + (skillData.BaseValueIncrease * state.Level);
            float shockChance = skillData.StatucChance;


            for (int i = 0; i < skillData.HitCount; i++)
            {
                enemy.TakeDamage(damage);
                Debug.Log($"[ProjectileLightning] Enemy에게 {damage}의 피해릅 입힘");
            }

            if (Random.value < shockChance / 100f)
            {
                //상태이상 적용 로직, 인자로 StatusEffectType 전달해주기
            }

            hasHit = true;
        }
    }


}
