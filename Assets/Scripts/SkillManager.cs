using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SkillManager : MonoBehaviour
{
    public static SkillManager Instance { get; private set; }

    private Dictionary<SkillId, PlayerSkillState> skillStates = new Dictionary<SkillId, PlayerSkillState>();
    private SkillId[] equippedSkills = new SkillId[6];

    public LayerMask targetMask;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    public void AddSkill(SkillId id, int couunt = 1)
    {
        //���� �ش� ��ų�� ���������� �ʴٸ� ���ο� PlayerSkillState �����ϰ� �ʱ�ȭ,
        //�� �� ��ųʸ��� �߰���.
        if (skillStates.TryGetValue(id, out PlayerSkillState state) == false)
        {
            state = new PlayerSkillState(id);
            skillStates[id] = state;
        }

        //�������̶� state�� ���� ���� ���� ����
        //skillStates[id].OwnedCount += couunt;
        state.OwnedCount += couunt;
    }

    public PlayerSkillState GetSkillState(SkillId id)
    {
        //PlayerSkillState tempState = new PlayerSkillState(id);
        //skillStates[id] = tempState;
        //tempState.OwnedCount = 0; 
        //tempState.Level = 1;
        //tempState.AwakenLevel = 0;


        if (skillStates.TryGetValue(id, out PlayerSkillState state))
        {
            return state;
        }
        Debug.LogWarning($"[SkillManager] ��ų ���¸� ã�� �� ����, {id}");
        return null;
    }

    public bool IsUnlocked(SkillId id)
    {
        //�ش� ��ų�� ��ųʸ��� �����ϴ��� Ȯ��
        return skillStates.ContainsKey(id);
    }

    public SkillId[] GetEquippedSkills()
    {
        return equippedSkills;
    }

    public void SetEquippedSkills(SkillId[] newEquips)
    {
        if (newEquips == null)
        {
            Debug.LogWarning("[SkillManager] SetEquippedSkills null ���޵�");
            return;
        }

        for (int i = 0; i < equippedSkills.Length; i++)
        {
            equippedSkills[i] = (i < newEquips.Length) ? newEquips[i] : SkillId.None;
        }

        Debug.Log($"[SkillManager] SetEquippedSkills ���� : {string.Join(",", equippedSkills)}");


    }

    public Dictionary<SkillId, PlayerSkillState> GetAllSkills()
    {
        return skillStates;
    }

    public void LoadFrom(PlayerSkillSaveData saveData)
    {
        skillStates.Clear();

        //PlayerSkillSaveData������ SkillStateSaveData����Ʈ ��ȸ
        foreach (SkillStateSaveData skillState in saveData.skillStates)
        {
            PlayerSkillState state = new PlayerSkillState(skillState.skillId)
            {
                OwnedCount = skillState.ownedCount,
                Level = skillState.level,
                AwakenLevel = skillState.awakenLevel
            };
            skillStates[skillState.skillId] = state;
        }
    }

    public PlayerSkillSaveData BuildSaveData()
    {
        PlayerSkillSaveData saveData = new PlayerSkillSaveData();

        foreach (var kvp in skillStates)
        {
            SkillStateSaveData data = new SkillStateSaveData()
            {
                skillId = kvp.Key,
                level = kvp.Value.Level,
                ownedCount = kvp.Value.OwnedCount,
                awakenLevel = kvp.Value.AwakenLevel
            };

            saveData.skillStates.Add(data);
        }

        return saveData;
    }

    public float CalculateSkillDamage(SkillData skillData)
    {
        PlayerSkillState state = GetSkillState(skillData.SkillId);

        float playerDamage = GameManager.Instance.stats.damage;

        float multiplier = (skillData.BaseValue + (skillData.BaseValueIncrease * state.Level)) / 100f;

        float finalDamage = playerDamage * multiplier;

        return finalDamage;
    }

    [ContextMenu("Add Dark Boom")]
    private void AddDarkBoom()
    {
        AddSkill(SkillId.DarkBoom, 1);
        GameManager.Instance.statSaver.SavePlayerSkillData(BuildSaveData());
    }

    [ContextMenu("Add Holy Burst")]
    private void AddHolyBurst()
    {
        AddSkill(SkillId.HolyBurst, 1);
        GameManager.Instance.statSaver.SavePlayerSkillData(BuildSaveData());
    }

    [ContextMenu("Add Dragon Breath")]
    private void AddDragonBreath()
    {
        AddSkill(SkillId.DragonBreath, 1);
        GameManager.Instance.statSaver.SavePlayerSkillData(BuildSaveData());
    }

    [ContextMenu("Add Ice Arrow")]
    private void AddIceArrow()
    {
        AddSkill(SkillId.IceArrow, 1);
        GameManager.Instance.statSaver.SavePlayerSkillData(BuildSaveData());
    }

    [ContextMenu("ALL")]
    private void AddAllSkill()
    {
        foreach (SkillId id in Enum.GetValues(typeof(SkillId)))
        {
            AddSkill(id, 1);
        }

        GameManager.Instance.statSaver.SavePlayerSkillData(BuildSaveData());
    }
}

[SerializeField]
public class PlayerSkillState
{
    public SkillId SkillId;
    public int OwnedCount;
    public int Level;
    public int AwakenLevel;

    public PlayerSkillState(SkillId id)
    {
        //�����ڿ� �Ű����� ���� ����ȭ�� �ȵ�
        //�̰� �ǻ�� Ŭ����, ���� ��ȯ�� Ŭ���� ���� �����ؾ� ��
        SkillId = id;
        OwnedCount = 0;
        Level = 1;
        AwakenLevel = 0;
    }
}