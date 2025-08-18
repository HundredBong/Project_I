using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

public static class FwordFilter
{
    private static readonly HashSet<string> _filterNames = new HashSet<string>();
    private static bool _loaded = false;

    public static void EnsureLoad()
    {
        if (_loaded) return;

        _filterNames.Clear();

        TextAsset fwordList = Resources.Load<TextAsset>("Fwords");

        if (fwordList == null)
        {
            Debug.LogError("[FwordFilter] Fword 로딩 실패");
            _loaded = true;
            return;
        }

        string[] lines = fwordList.text.Split('\n');

        for (int i = 0; i < lines.Length; i++)
        {
            string token = lines[i].Trim();

            //공백이 있으면 
            if (string.IsNullOrWhiteSpace(token))
            {
                continue;
            }

            //문자 정규화로 대소문자 관계없이 처리
            token = token.Normalize(NormalizationForm.FormC).ToLowerInvariant();
            _filterNames.Add(token);
        }

        _loaded = true;
    }

    public static bool TryFindFword(string input)
    {
        if (_loaded == false)
        {
            EnsureLoad();
        }

        string normalized = input.Trim().Normalize(NormalizationForm.FormC).ToLowerInvariant();

        foreach (string fword in _filterNames)
        {
            if (normalized.Contains(fword))
            {
                return true;
            }
        }

        return false;
    }
}
