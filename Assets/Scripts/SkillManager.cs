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
        //만약 해당 스킬을 가지고있지 않다면 새로운 PlayerSkillState 생성하고 초기화,
        //그 후 딕셔너리에 추가함.
        if (skillStates.TryGetValue(id, out PlayerSkillState state) == false)
        {
            state = new PlayerSkillState(id);
            skillStates[id] = state;
        }

        //참조형이라서 state를 통해 직접 수정 가능
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
        Debug.LogWarning($"[SkillManager] 스킬 상태를 찾을 수 없음, {id}");
        return null;
    }

    public bool IsUnlocked(SkillId id)
    {
        //해당 스킬이 딕셔너리에 존재하는지 확인
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
            Debug.LogWarning("[SkillManager] SetEquippedSkills null 전달됨");
            return;
        }

        for (int i = 0; i < equippedSkills.Length; i++)
        {
            equippedSkills[i] = (i < newEquips.Length) ? newEquips[i] : SkillId.None;
        }

        Debug.Log($"[SkillManager] SetEquippedSkills 적용 : {string.Join(",", equippedSkills)}");


    }

    public Dictionary<SkillId, PlayerSkillState> GetAllSkills()
    {
        return skillStates;
    }

    public void LoadFrom(PlayerSkillSaveData saveData)
    {
        skillStates.Clear();

        //PlayerSkillSaveData내부의 SkillStateSaveData리스트 순회
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
        //생성자에 매개변수 들어가면 직렬화가 안됨
        //이건 실사용 클래스, 저장 전환용 클래스 따로 생성해야 함
        SkillId = id;
        OwnedCount = 0;
        Level = 1;
        AwakenLevel = 0;
    }
}