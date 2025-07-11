using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System;

public class ActiveSkillSlot : MonoBehaviour
{
    [Header("��ų ����")]
    [SerializeField] private Image skillIcon;
    [SerializeField] private Button skillButton;

    [Header("��Ÿ��")]
    [SerializeField] private Image cooldownImage;
    [SerializeField] private Image globalCooldownImage;
    [SerializeField] private TextMeshProUGUI cooldownText;

    private SkillBase equippedSkill;

    private float cooldownRemain = 0f;
    private float cooldownTotal = 0f;

    private float globalCooldownRemain = 0f;
    private float globalCooldownTotal = 0f;

    private bool isGlobalCooldown = false;

    public bool IsGlobalCooldown
    {
        get { return isGlobalCooldown; } 
    }

    public float Cooldown => equippedSkill.Cooldown;

    //ActiveSkillPanel�� ��ų�� ����ߴٰ� �˸��� �뵵
    //ActiveSkillSlotŸ���� ������ �ʿ�� ������ ����������, ��� ȣ���� �ݹ����� �˾ƺ��� �뵵 ������ �������� Ÿ�� ����
    public Action<ActiveSkillSlot> OnSkillExecuted;

    public void Init(SkillId id)
    {
        if (id == SkillId.None)
        {
            gameObject.SetActive(false);
            return;
        }
        gameObject.SetActive(true);

        SkillData data = DataManager.Instance.GetSkill(id);
        equippedSkill = SkillFactory.Create(id, data);

        skillIcon.sprite = DataManager.Instance.GetSpriteByKey(data.SkillIcon);
        skillIcon.enabled = true;
        skillButton.onClick.RemoveAllListeners();
        skillButton.onClick.AddListener(OnSkillButtonClicked);
    }

    private void OnSkillButtonClicked()
    {
        if (equippedSkill == null)
        {
            Debug.LogWarning("[ActiveSkillSlot] ������ ��ų�� ����");
            return;
        }

        if (isGlobalCooldown)
        {
            Debug.LogWarning("[ActiveSkillSlot] �۷ι� ��Ÿ�� ������");
            return;
        }

        bool success = equippedSkill.TryExecute(GameManager.Instance.player.gameObject);

        if (success == true)
        {
            StartCooldown(equippedSkill.Cooldown);
            //StartGlobalCooldown(1f);
            OnSkillExecuted?.Invoke(this);
        }
    }

    public void StartCooldown(float cooldown)
    {
        cooldownTotal = cooldown;
        cooldownRemain = cooldown;
        cooldownImage.fillAmount = 1f;
    }

    public void StartGlobalCooldown(float cooldown)
    {
        globalCooldownTotal = cooldown;
        globalCooldownRemain = cooldown;
        isGlobalCooldown = true;
        globalCooldownImage.fillAmount = 1f;
    }

    public SkillBase GetEquippedSkill()
    {
        return equippedSkill;
    }

    private void Update()
    {
        if (cooldownRemain > 0f)
        {
            cooldownRemain -= Time.deltaTime;
            cooldownImage.fillAmount = cooldownRemain / cooldownTotal;
        }

        if (isGlobalCooldown)
        {
            globalCooldownRemain -= Time.deltaTime;
            globalCooldownImage.fillAmount = globalCooldownRemain / globalCooldownTotal;

            if (globalCooldownRemain <= 0f)
            {
                isGlobalCooldown = false;
            }
        }

        cooldownText.text = cooldownRemain > 0f ? $"{cooldownRemain:F1}" : "";
    }

    

}
