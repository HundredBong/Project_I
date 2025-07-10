using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIAutoSkill : MonoBehaviour
{
    [SerializeField] private Button autoSkillButton;
    [SerializeField] private TextMeshProUGUI autoSkillText;

    private bool autoSkillEnabled = false;
    private Coroutine autoSkillCoroutine;
    private WaitForSeconds wait;


    private void Start()
    {
        autoSkillButton.onClick.AddListener(ToggleAutoSkill);
        wait = new WaitForSeconds(0.2f);    
    }

    private void ToggleAutoSkill()
    {

        autoSkillEnabled = !autoSkillEnabled;
        autoSkillText.text = autoSkillEnabled ? "AUTO On" : "AUTO OFF";

        if (autoSkillEnabled)
        {
            autoSkillCoroutine = StartCoroutine(AutoSkillCoroutine());
        }
        else
        {
            StopCoroutine(autoSkillCoroutine);
        }
    }

    private IEnumerator AutoSkillCoroutine()
    {
        while (true)
        {

        }
    }

}

