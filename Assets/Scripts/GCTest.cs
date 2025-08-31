using System.Collections;
using System.Collections.Generic;
using System.Text;
using TMPro;
using UnityEngine;

public class GCTest : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI _text;
    [SerializeField] private bool _useStringBuilder = false;
    private StringBuilder _sb = new StringBuilder(32);

    private int _value = 0;
    private int _sbValue = 0;
    private void Update()
    {
        _value++;
        _sbValue--;

        if (_useStringBuilder == false)
        {
            _text.SetText("{0}", _value);
            //_text.text = $"{_value}"; //20 ~ 30 확인됨
        }
        else
        {
            //stringBuild사용시 80정도 확인됨
            _sb.Clear();
            _sb.Append(_sbValue);
            _text.SetText(_sb);
        }
    }
}
