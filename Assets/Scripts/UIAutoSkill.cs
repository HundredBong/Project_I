using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIAutoSkill : MonoBehaviour
{
    [SerializeField] private Button autoSkillButton;
    [SerializeField] private TextMeshProUGUI autoSkillText;
    [SerializeField] private ActiveSkillPanel activeSkillPanel;

    private bool autoSkillEnabled;
    private Coroutine autoSkillCoroutine;
    private WaitForSeconds wait;


    private void Awake()
    {
        wait = new WaitForSeconds(0.2f);
    }

    private void OnEnable()
    {
        autoSkillButton.onClick.AddListener(ToggleAutoSkill);
    }

    private void OnDisable()
    {
        autoSkillButton.onClick.RemoveListener(ToggleAutoSkill);

        if (autoSkillCoroutine != null)
        {
            StopCoroutine(autoSkillCoroutine);
            autoSkillCoroutine = null;
        }
    }

    private void Start()
    {
        autoSkillEnabled = LocalSetting.LoadAutoSkillActivate();

        if (autoSkillEnabled)
        {
            if (autoSkillCoroutine != null)
            {
                StopCoroutine(autoSkillCoroutine);
                autoSkillCoroutine = null;
            }
            autoSkillText.text = "AUTO ON";
            autoSkillCoroutine = StartCoroutine(AutoSkillCoroutine());
        }
    }

    private void ToggleAutoSkill()
    {
        autoSkillEnabled = !autoSkillEnabled;
        autoSkillText.text = autoSkillEnabled ? "AUTO ON" : "AUTO OFF";

        LocalSetting.SaveAutoSkillActivate(autoSkillEnabled);

        if (autoSkillEnabled)
        {
            autoSkillCoroutine = StartCoroutine(AutoSkillCoroutine());
        }
        else
        {
            StopCoroutine(autoSkillCoroutine);
            autoSkillCoroutine = null;
        }
    }

    private IEnumerator AutoSkillCoroutine()
    {
        while (true)
        {
            yield return wait;

            foreach (ActiveSkillSlot slot in activeSkillPanel.GetSlots())
            {
                if (slot.GetEquippedSkill() != null && slot.IsGlobalCooldown == false)
                {
                    bool success = slot.GetEquippedSkill().TryExecute(GameManager.Instance.player.gameObject);

                    if (success)
                    {
                        //개별 스킬 쿨타임 적용 및 글로벌 쿨타임 적용
                        slot.StartCooldown(slot.Cooldown);
                        slot.OnSkillExecuted?.Invoke(slot);
                        break;
                    }
                }
            }
        }
    }

}

