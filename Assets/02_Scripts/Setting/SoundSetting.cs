using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SoundSetting : MonoBehaviour
{
    [Header("���� �����̴� UI")]
    [SerializeField] private Slider masterSlider;
    [SerializeField] private Slider bgmSlider;
    [SerializeField] private Slider sfxSlider;

    [Header("BGM ���� UI")]
    [SerializeField] private TMP_Dropdown bgmDropdown;
    void Start()
    {
        Init();
        InitEvent();
    }
    private void Init()
    {
        // ����
        if (SoundManager.Instance != null)
        {
            this.masterSlider.value = SoundManager.Instance.MasterVolume;
            this.bgmSlider.value = SoundManager.Instance.BgmVolume;
            this.sfxSlider.value = SoundManager.Instance.SfxVolume;
        }

        // ��� ��Ӵٿ�
        if (this.bgmDropdown != null)
        {
            // ������ ����� �����ʰ� �ִٸ� �����ִ� ���� �����մϴ�.
            this.bgmDropdown.onValueChanged.RemoveAllListeners();
            this.bgmDropdown.onValueChanged.AddListener(OnBgmSelectionChanged);
        }

        InitBgmDropdown();
    }
    private void InitEvent()
    {
        this.masterSlider.onValueChanged.AddListener(SetMasterVolume);
        this.bgmSlider.onValueChanged.AddListener(SetBgmVolume);
        this.sfxSlider.onValueChanged.AddListener(SetSfxVolume);
    }

    private void InitBgmDropdown()
    {
        List<BGMData> bgmList = SoundManager.Instance.BgmList;

        if (bgmList == null || bgmList.Count == 0) return;

        this.bgmDropdown.ClearOptions();
        List<string> options = new List<string>();
        foreach (var bgm in bgmList)
        {
            options.Add(bgm.soundName);
        }
        this.bgmDropdown.AddOptions(options);

        int initialIndex = SoundManager.Instance.CurrentBgmIndex;

        if (initialIndex < 0) initialIndex = 0;

        this.bgmDropdown.value = initialIndex;
        this.bgmDropdown.RefreshShownValue();
    }

    // �����̴��� ������ ������ ����Ǵ� �Լ���

    public void SetMasterVolume(float volume)
    {
        if (SoundManager.Instance != null)
            SoundManager.Instance.SetMasterVolume(volume);
    }

    public void SetBgmVolume(float volume)
    {
        if (SoundManager.Instance != null)
            SoundManager.Instance.SetBGMVolume(volume);
    }

    public void SetSfxVolume(float volume)
    {
        if (SoundManager.Instance != null)
            SoundManager.Instance.SetSFXVolume(volume);
    }

    public void OnBgmSelectionChanged(int index)
    {
        if (SoundManager.Instance == null) return;

        if (StoryManager.Instance != null && StoryManager.Instance.IsRecallPlaying)
            return;
        
        if (StoryManager.Instance != null && StoryManager.Instance.IsPlaying)
            return;

        string selectedBgmName = this.bgmDropdown.options[index].text;

        SoundManager.Instance.PlayBGM(selectedBgmName);
    }
}
