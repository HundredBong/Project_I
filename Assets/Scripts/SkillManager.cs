using Cysharp.Threading.Tasks;
using System;
using System.Collections.Generic;
using UnityEngine;

public class SkillManager : MonoBehaviour
{
    public static SkillManager Instance { get; private set; }

    private Dictionary<SkillId, PlayerSkillState> skillStates = new Dictionary<SkillId, PlayerSkillState>();
    private SkillId[] equippedSkills = new SkillId[6];

    public LayerMask targetMask;

    public Action onRequestResetAllCooldowns;

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

    public int GetRequiredCount(SkillData data)
    {
        PlayerSkillState state = GetSkillState(data.SkillId);

        //각성 레벨이 배열 길이 이상이면,
        if (state.AwakenLevel >= data.AwakenRequiredCount.Length)
        {
            Debug.LogWarning($"[SkillManager] 각성 수치 초과 접근 시도: {data.SkillId} Level {state.AwakenLevel}");
            return -1; // 또는 throw exception
        }

        return data.AwakenRequiredCount[state.AwakenLevel];
    }

    public bool TryLevelUp(SkillData data)
    {
        SkillId skillId = data.SkillId;
        if (skillStates.TryGetValue(skillId, out var state))
        {
            if (state.Level < data.MaxLevels[state.AwakenLevel])
            {
                state.Level++;
                return true;
            }
        }
        return false;
    }

    public bool TryAwaken(SkillData data)
    {
        SkillId skillId = data.SkillId;

        if (skillStates.TryGetValue(skillId, out var state))
        {
            int[] awakenRequiredCounts = DataManager.Instance.GetSkill(skillId).AwakenRequiredCount;

            if (state.AwakenLevel < awakenRequiredCounts.Length)
            {
                int requiredCount = awakenRequiredCounts[state.AwakenLevel];
                if (state.OwnedCount >= requiredCount)
                {
                    state.OwnedCount -= requiredCount;
                    state.AwakenLevel++;
                    return true;
                }
            }
        }

        return false;
    }

    public bool CanAwaken(SkillData data)
    {
        if (skillStates.TryGetValue(data.SkillId, out var state) == false)
        {
            return false;
        }

        int awakenLevel = state.AwakenLevel;

        //각성 단계가 최대치를 초과했는지 체크
        if (awakenLevel >= data.AwakenRequiredCount.Length)
        {
            return false;
        }

        int requiredCount = data.AwakenRequiredCount[awakenLevel];
        return state.OwnedCount >= requiredCount;
    }

    public void RequestResetAllCooldowns()
    {
        onRequestResetAllCooldowns?.Invoke();
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.R))
        {
            //디버그용, 모든 스킬 쿨타임 초기화
            RequestResetAllCooldowns();
        }
    }


    [ContextMenu("ALL")]
    private void AddAllSkill()
    {
        foreach (SkillId id in Enum.GetValues(typeof(SkillId)))
        {
            AddSkill(id, 1);
        }

        GameManager.Instance.statSaver.SavePlayerSkillDataAsync(BuildSaveData()).Forget();
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