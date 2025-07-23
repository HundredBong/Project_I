using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class LocalSetting
{
    private const string UpgradeAmountKey = "UpgradeAmount";
    private const string LanguageKey = "Language";
    private const string AutoSkillKey = "AutoSkillActivate";
    private const string StageLoopKey = "StageLoopActivate";


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

    public static void SaveAutoSkillActivate(bool active)
    {
        PlayerPrefs.SetInt(AutoSkillKey, active ? 1 : 0);
        PlayerPrefs.Save();
    }

    public static bool LoadAutoSkillActivate()
    {
        //디폴트 OFF
        return PlayerPrefs.GetInt(AutoSkillKey, 0) == 1;
    }

    public static void SaveStageLoop(bool active)
    {
        PlayerPrefs.SetInt(StageLoopKey, active ? 1 : 0);
        PlayerPrefs.Save();
    }

    public static bool LoadStageLoop()
    {
        return PlayerPrefs.GetInt(StageLoopKey, 0) == 1;
    }

}
