using UnityEngine;
using System.Collections;
using System;

[RequireComponent (typeof(AudioSource))]
[ExecuteInEditMode]
public class AudioManager : external.Singleton<AudioManager> {
    private AudioSource mAudioSourceEffect;
    private AudioSource mAudioSourceBackground;
    private bool mIsEnable;
    // Use this for initialization
    public bool isEnable()
    {
        return mIsEnable;
    }

    void Start () {
       
    }
    void Awake()
    {
        AudioSource[] audioSources = GetComponents<AudioSource>();
        if (audioSources.Length < 2)
        {
            this.gameObject.AddComponent<AudioSource>();
        }
        audioSources = GetComponents<AudioSource>();

        mAudioSourceEffect = audioSources[0];
        mAudioSourceBackground = audioSources[1];
        mAudioSourceEffect.loop = false;
        mAudioSourceBackground.loop = true;
        mAudioSourceEffect.playOnAwake = false;
        mAudioSourceBackground.playOnAwake = true;
		mAudioSourceBackground.hideFlags=HideFlags.None; //mAudioSourceEffect.hideFlags = HideFlags.None;//.

        mIsEnable = PlayerPrefs.GetInt("SOUND_ENABLE", 1) == 1 ? true : false;
        enableAudio(mIsEnable);
    }
        // Update is called once per frame
        void Update () {
	
	}

    public void playMusic(AudioClip clip, bool isResume,bool isLoop)
    {
        if (isResume && (mAudioSourceBackground.clip == clip))
            return;
        mAudioSourceBackground.loop = isLoop;
        mAudioSourceBackground.clip = clip;
        if (mIsEnable || mAudioSourceBackground.isPlaying)
            mAudioSourceBackground.Play();

    }
    private void stopMusic()
    {
        mAudioSourceBackground.clip = null;
        mAudioSourceBackground.Stop();
    }
 
    public static void playBackgroundMusic(string fileName, bool isResume = true, bool isLoop = true)
    {
        AudioClip clip = Resources.Load<AudioClip>("Audios/" + fileName);
        getInstance().playMusic(clip, isResume, isLoop);
    }
    public static void playBackgroundMusic(AudioClip clip, bool isResume = true, bool isLoop = true)
    {
        getInstance().mAudioSourceBackground.volume = 0.5f;
        getInstance().playMusic(clip, isResume, isLoop);
    }
    public void onOff()
    {
        enableAudio(!mIsEnable);
    }
    public void enableAudio(bool enabled)
    {
        PlayerPrefs.SetInt("SOUND_ENABLE", enabled?1:0);
        mIsEnable = enabled;
        mAudioSourceEffect.enabled = mIsEnable;
        mAudioSourceBackground.enabled = mIsEnable;
    }

    public static void stopBackgroundMusic()
    {
        getInstance().stopMusic();
    }
	public static float getVolumeBackgroundMusic()
	{
		return getInstance().mAudioSourceBackground.volume;
    }
    public static void SetVolume(float value)
    {
        SetVolumeBackgroundMusic(value);
        SetVolumeEffect(value);
    }
    public static void SetVolumeBackgroundMusic(float value)
    {
        getInstance().mAudioSourceBackground.volume = value;
    }
    public static void SetVolumeEffect(float value)
    {
        getInstance().mAudioSourceEffect.volume = value;
    }
    public static void playEffect(string fileName, float volume = 1)
    {
        AudioClip clip = Resources.Load<AudioClip>("Audios/" + fileName);
        playEffect(clip, volume);
    }
    public static void playEffect(AudioClip clip, float volume = 1)
    {
        if (!getInstance().mIsEnable)
            return;
        getInstance().mAudioSourceEffect.PlayOneShot(clip, volume);
    }
}
