using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SoundManager : Singleton<SoundManager>
{
    [Header("����� �ҽ�")]
    [SerializeField] private AudioSource bgmSource;
    [SerializeField] private AudioSource sfxSource;
    [SerializeField] private AudioSource typingSource;

    [Header("���� ������ ����")]
    [SerializeField] private SoundList soundList;
    public List<BGMData> BgmList => this.soundList.bgmList;


    [Header("���� ����")]
    [Range(0f, 1f)][SerializeField] private float masterVolume = 1f;
    [Range(0f, 1f)][SerializeField] private float bgmVolume = 1f;
    [Range(0f, 1f)][SerializeField] private float sfxVolume = 1f;

    private int currentBgmIndex = -1;
    public int CurrentBgmIndex => currentBgmIndex;

    public float MasterVolume => this.masterVolume;
    public float BgmVolume => this.bgmVolume;
    public float SfxVolume => this.sfxVolume;

    private Dictionary<string, SoundData> dicBGM = new Dictionary<string, SoundData>();
    private Dictionary<string, SoundData> dicSFX = new Dictionary<string, SoundData>();


    protected override void Awake()
    {
        base.Awake();
        Setting();
        Init();
    }

    void Start()
    {
        PlayRandomBGM();
    }

    private void Init()
    {
        if (this.soundList == null) return;

        foreach (var data in this.soundList.bgmList)
        {
            if (data != null && !this.dicBGM.ContainsKey(data.soundName))
                this.dicBGM.Add(data.soundName, data);
        }

        foreach (var data in this.soundList.sfxList)
        {
            if (data != null && !this.dicSFX.ContainsKey(data.soundName))
                this.dicSFX.Add(data.soundName, data);
        }
    }
    public void PlayRandomBGM()
    {
        if (this.BgmList == null || this.BgmList.Count == 0) return;

        this.currentBgmIndex = Random.Range(0, this.BgmList.Count);

        PlayBGM(this.BgmList[this.currentBgmIndex].soundName);
    }
    private void Setting()
    {
        if (this.bgmSource == null)
            this.bgmSource = this.gameObject.AddComponent<AudioSource>();
        if (this.sfxSource == null)
            this.sfxSource = this.gameObject.AddComponent<AudioSource>();
        if (typingSource == null)
            typingSource = gameObject.AddComponent<AudioSource>();
        
        this.bgmSource.loop = true;
        this.sfxSource.loop = false;
        typingSource.loop = false;

        this.masterVolume = PlayerPrefs.GetFloat("MasterVolume", 1f);
        this.bgmVolume = PlayerPrefs.GetFloat("BgmVolume", 1f);
        this.sfxVolume = PlayerPrefs.GetFloat("SfxVolume", 1f);
    }

    /// <summary>
    /// ������ �̸��� BGM�� ���
    /// </summary>
    public void PlayBGM(string soundName)
    {
        if (!this.dicBGM.TryGetValue(soundName, out SoundData data))
            return;

        if (this.bgmSource.clip == data.clip && this.bgmSource.isPlaying)
            return;

        this.bgmSource.clip = data.clip;
        this.bgmSource.volume = data.volume * this.bgmVolume * this.masterVolume;
        this.bgmSource.Play();
    }
    
    public void PlayBGM(BGMData data)
    {
        if (data == null)
            return;

        PlayBGM(data.soundName);
    }
    
    /// <summary>
    /// ������ �̸��� SFX�� ���
    /// </summary>
    public void PlaySFX(string soundName)
    {
        if (!this.dicSFX.TryGetValue(soundName, out SoundData data))
            return;

        this.sfxSource.PlayOneShot(data.clip, data.volume * this.sfxVolume * this.masterVolume);
    }

    public void PlaySFX(SFXData data)
    {
        if (data == null)
            return;

        PlaySFX(data.soundName);
    }
    
    /// <summary>
    /// Ư�� ������Ʈ�� ��ġ���� ȿ������ ���
    /// </summary>
    public void PlaySoundOnObj(string soundName, GameObject obj)
    {
        if (!this.dicSFX.TryGetValue(soundName, out SoundData data))
            return;

        AudioSource source = obj.GetComponent<AudioSource>();

        if (source == null)
        {
            source = obj.AddComponent<AudioSource>();
            source.playOnAwake = false;
        }

        source.PlayOneShot(data.clip, data.volume * this.sfxVolume * this.masterVolume);
    }

    /// <summary>
    /// BGM ����� Ŭ���� �˻��Ͽ� ��ȯ
    /// </summary>
    public AudioClip GetBGMClip(string soundName)
    {
        if (this.dicBGM.TryGetValue(soundName, out SoundData data))
        {
            return data.clip;
        }
        return null;
    }

    /// <summary>
    /// SFX ����� Ŭ���� �˻��Ͽ� ��ȯ
    /// </summary>
    public AudioClip GetSFXClip(string soundName)
    {
        if (this.dicSFX.TryGetValue(soundName, out SoundData data))
        {
            return data.clip;
        }
        return null;
    }

    public void PlayTypeSound()
    {
        PlaySFX("Type");
    }
    
    /// <summary>
    /// ������ ������ ���� (0.0 ~ 1.0)
    /// </summary>
    public void SetMasterVolume(float volume)
    {
        this.masterVolume = Mathf.Clamp01(volume);

        PlayerPrefs.SetFloat("MasterVolume", this.masterVolume);
        PlayerPrefs.Save();

        UpdateVolumes();
    }
    /// <summary>
    /// BGM ������ ���� (0.0 ~ 1.0)
    /// </summary>
    public void SetBGMVolume(float volume)
    {
        this.bgmVolume = Mathf.Clamp01(volume);

        PlayerPrefs.SetFloat("BgmVolume", this.bgmVolume);
        PlayerPrefs.Save();

        UpdateVolumes();
    }

    /// <summary>
    /// SFX ������ ���� (0.0 ~ 1.0)
    /// </summary>
    public void SetSFXVolume(float volume)
    {
        this.sfxVolume = Mathf.Clamp01(volume);

        PlayerPrefs.SetFloat("SfxVolume", this.sfxVolume);
        PlayerPrefs.Save();

        UpdateVolumes();
    }

    /// <summary>
    /// ����� ���� ��ġ�� AudioSource�� ��� �ݿ�
    /// </summary>
    private void UpdateVolumes()
    {
        if (this.bgmSource != null)
            this.bgmSource.volume = this.bgmVolume * this.masterVolume;

        if (this.sfxSource != null)
            this.sfxSource.volume = this.sfxVolume * this.masterVolume;
        
        if (typingSource != null)
            typingSource.volume = sfxVolume * masterVolume;
    }

    /// <summary>
    /// ���� ��� ���� ��������� ����
    /// </summary>
    public void StopBGM() => this.bgmSource.Stop();

    /// <summary>
    /// ���� ��� ���� ȿ������ ����
    /// </summary>
    public void StopSFX() => this.sfxSource.Stop();

    /// <summary>
    /// ���� ��� ���� ��������� �Ͻ�����
    /// </summary>
    public void PauseBGM() => this.bgmSource.Pause();

    /// <summary>
    /// �Ͻ������� ��������� �ٽ� ���
    /// </summary>
    public void ResumeBGM() => this.bgmSource.UnPause();

    /// <summary>
    /// ���� ��� ���� ��� ȿ������ �Ͻ�����
    /// </summary>
    public void PauseSFX() => this.sfxSource.Pause();

    /// <summary>
    /// �Ͻ������� ȿ�������� �ٽ� ���
    /// </summary>
    public void ResumeSFX() => this.sfxSource.UnPause();

    /// <summary>
    /// ��������� ���Ұ� ����
    /// </summary>
    public void SetMuteBGM(bool isMute) => this.bgmSource.mute = isMute;

    /// <summary>
    /// ȿ������ ���Ұ� ����
    /// </summary>
    public void SetMuteSFX(bool isMute) => this.sfxSource.mute = isMute;
}