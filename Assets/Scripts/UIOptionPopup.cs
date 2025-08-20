using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIOptionPopup : UIPopup
{
    [Header("TMP")]
    [SerializeField] private TextMeshProUGUI _optionText;
    [SerializeField] private TextMeshProUGUI _soundOptionText;
    [SerializeField] private TextMeshProUGUI _bgmText;
    [SerializeField] private TextMeshProUGUI _sfxText;
    [SerializeField] private TextMeshProUGUI _languageOptionText;
    [SerializeField] private TextMeshProUGUI _krText;
    [SerializeField] private TextMeshProUGUI _enText;
    [SerializeField] private TextMeshProUGUI _accountOptionText;
    [SerializeField] private TextMeshProUGUI _signOutText;
    [Header("Slider")]
    [SerializeField] private Slider _bgmSlider;
    [SerializeField] private Slider _sfxSlider;
    [Header("Button")]
    [SerializeField] private Button _krButton;
    [SerializeField] private Button _enButton;
    [SerializeField] private Button _signOutButton;

    protected override void Awake()
    {
        base.Awake();

        _bgmSlider.minValue = 0;
        _bgmSlider.maxValue = 1;

        _sfxSlider.minValue = 0;
        _sfxSlider.maxValue = 1;
    }

    private void Start()
    {
        _bgmSlider.value = AudioController.Instance.bgmVolume;
        _sfxSlider.value = AudioController.Instance.sfxVolume;
    }

    private void OnEnable()
    {
        LanguageManager.OnLanguageChanged += SetLocalizedText;

        _krButton.onClick.AddListener(ChangeLanguageKR);
        _enButton.onClick.AddListener(ChangeLanguageEN);
        _signOutButton.onClick.AddListener(SignOut);

        _bgmSlider.onValueChanged.AddListener(ChangeVolumeBGM);
        _sfxSlider.onValueChanged.AddListener(ChangeVolumeSFX);

        SetLocalizedText();
    }

    private void OnDisable()
    {
        LanguageManager.OnLanguageChanged -= SetLocalizedText;

        _krButton.onClick.RemoveListener(ChangeLanguageKR);
        _enButton.onClick.RemoveListener(ChangeLanguageEN);
        _signOutButton.onClick.RemoveListener(SignOut);
    }

    private void SetLocalizedText()
    {
        _optionText.text = DataManager.Instance.GetLocalizedText("Option_Title");
        _soundOptionText.text = DataManager.Instance.GetLocalizedText("Option_Sound");
        _bgmText.text = DataManager.Instance.GetLocalizedText("Option_BGM");
        _sfxText.text = DataManager.Instance.GetLocalizedText("Option_SFX");
        _languageOptionText.text = DataManager.Instance.GetLocalizedText("Option_Language");
        _krText.text = DataManager.Instance.GetLocalizedText("Option_KR");
        _enText.text = DataManager.Instance.GetLocalizedText("Option_EN");
        _accountOptionText.text = DataManager.Instance.GetLocalizedText("Option_Account");
        _signOutText.text = DataManager.Instance.GetLocalizedText("Option_SignOut");
    }

    private void ChangeLanguageKR()
    {
        LanguageManager.SetLanguage(LanguageType.KR);
    }

    private void ChangeLanguageEN()
    {
        LanguageManager.SetLanguage(LanguageType.EN);
    }

    private void SignOut()
    {
        GameManager.Instance.firebaseGoogleSignInAuth.SignOut();
    }

    private void ChangeVolumeBGM(float value)
    {
        AudioController.Instance.ChangeBgmVolume(value);
    }

    private void ChangeVolumeSFX(float value)
    {
        AudioController.Instance.ChangeSfxVolume(value);
    }
}
