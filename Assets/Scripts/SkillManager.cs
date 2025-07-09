using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SkillManager : MonoBehaviour
{
    public static SkillManager Instance { get; private set; }

    private Dictionary<SkillId, PlayerSkillState> skillStates = new Dictionary<SkillId, PlayerSkillState>();
    private SkillId[] equippedSkills = new SkillId[6];

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
        for (int i = 0; i < equippedSkills.Length; i++)
        {
            equippedSkills[i] = newEquips[i];
        }
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
                ownedCount = kvp.Value.OwnedCount,
                awakenLevel = kvp.Value.AwakenLevel
            };

            saveData.skillStates.Add(data);
        }

        return saveData;
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