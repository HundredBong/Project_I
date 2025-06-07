using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class LocalSetting
{
    private const string UpgradeAmountKey = "UpgradeAmount";
    private const string LanguageKey = "Language";

    public static void SaveLanguage(string language)
    {
        PlayerPrefs.SetString(LanguageKey, language);
        PlayerPrefs.Save();
    }

    public static string LoadLanguage()
    {
        //두 번째 인자는 기본 값
        return PlayerPrefs.GetString(LanguageKey, "KR");
    }

    public static void SaveUpgradeAmount(int count)
    {
        PlayerPrefs.SetInt(UpgradeAmountKey, count);
        PlayerPrefs.Save();
    }

    public static int LoadUpgradeAmount()
    {
        return PlayerPrefs.GetInt(UpgradeAmountKey, 1);
    }

}
