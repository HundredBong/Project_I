using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.SceneManagement;

public class AudioController : MonoBehaviour
{
    public static AudioController Instance { get; private set; }

    public float bgmVolume;
    public float sfxVolume;

    public AudioSource bgmSource;

    private Dictionary<DungeonType, string> dungeonBgmDic = new Dictionary<DungeonType, string>()
    {
        { DungeonType.EnhanceDungeon, "EnhanceDungeon" },
        { DungeonType.SkillDungeon, "SkillDungeon" }
    };

    public event Action<float> OnSfxVolumeChanged;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        bgmVolume = LocalSetting.LoadBgmVolume();
        sfxVolume = LocalSetting.LoadSfxVolume();

        bgmSource.spatialBlend = 0f;
        bgmSource.loop = true;

        SceneManager.activeSceneChanged += ChangeBGM;
    }

    private void OnDestroy()
    {
        SceneManager.activeSceneChanged -= ChangeBGM;
    }

    public void ChangeBgmVolume(float volume)
    {
        bgmVolume = volume;
        bgmSource.volume = volume;
        LocalSetting.SaveBgmVolume(volume);
    }

    public void ChangeSfxVolume(float volume)
    {
        sfxVolume = volume;
        LocalSetting.SaveSfxVolume(volume);
        OnSfxVolumeChanged?.Invoke(volume);
    }

    public void MuteAllVolume()
    {
        bgmVolume = 0;
        bgmSource.volume = 0;

        sfxVolume = 0;
    }

    private void ChangeBGM(Scene arg0, Scene arg1)
    {
        bgmSource.Stop();
        bgmSource.volume = bgmVolume;

        if (arg1.name.Contains("Loading"))
        {
            return;
        }

        if (arg1.name.Contains("Main"))
        {
            bgmSource.clip = DataManager.Instance.GetAudioClipByKey("MainBGM");
            bgmSource.Play();
            return;
        }

        DungeonType dungeonType = FindObjectOfType<GameStarter>(true)?.GetDungeonType() ?? DungeonType.None;

        if (dungeonType == DungeonType.None)
        {
            int stage = StageManager.Instance.currentStage;
            bgmSource.clip = DataManager.Instance.GetAudioClipByKey(DataManager.Instance.stageDataTable[stage].BGM);
        }
        else
        {
            string bgm = DataManager.Instance.GetDungeonData(dungeonType).BGM;
            bgmSource.clip = DataManager.Instance.GetAudioClipByKey(bgm);
        }

        bgmSource.Play();
    }

    public void ChangeBGM(int stage)
    {
        bgmSource.Stop();
        bgmSource.volume = bgmVolume;
        bgmSource.clip = DataManager.Instance.GetAudioClipByKey(DataManager.Instance.stageDataTable[stage].BGM);
        bgmSource.Play();
    }
}
