using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

[RequireComponent(typeof(TextMeshProUGUI))]
public class FrameLogger : MonoBehaviour
{
    public TextMeshProUGUI text;

    private void Awake()
    {
        if (text == null)
        {
            text = GetComponent<TextMeshProUGUI>();
        }

    }

    private void Update()
    {
        float fps = 1.0f / Time.deltaTime;
        text.text = Mathf.Round(fps).ToString();
    }
}
