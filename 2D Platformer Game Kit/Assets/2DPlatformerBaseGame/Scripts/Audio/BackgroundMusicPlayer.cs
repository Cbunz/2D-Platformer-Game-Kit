using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

public class BackgroundMusicPlayer : MonoBehaviour {

	public static BackgroundMusicPlayer Instance
    {
        get
        {
            if (instance != null)
            {
                return instance;
            }

            instance = FindObjectOfType<BackgroundMusicPlayer>();

            return instance;
        }
    }

    protected static BackgroundMusicPlayer instance;

    [Header("Music Settings")]
    public AudioClip musicAudioClip;
    public AudioMixerGroup musicOutput;
    public bool musicPlayOnAwake = true;
    [Range(0f, 1f)]
    public float musicVolume = 1f;
    [Header("Ambient Settings")]
    public AudioClip ambientAudioClip;
    public AudioMixerGroup ambientOutput;
    public bool ambientPlayOnAwake = true;
    [Range(0f, 1f)]
    public float ambientVolume = 1f;

    protected AudioSource musicAudioSource;
    protected AudioSource ambientAudioSource;

    protected bool transferMusicTime, transferAmbientTime;
    protected BackgroundMusicPlayer oldInstanceToDestroy = null;

    protected Stack<AudioClip> musicStack = new Stack<AudioClip>();

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            if (Instance.musicAudioClip == musicAudioClip)
            {
                transferMusicTime = true;
            }

            if (Instance.ambientAudioClip == ambientAudioClip)
            {
                transferAmbientTime = true;
            }

            oldInstanceToDestroy = Instance;
        }

        instance = this;

        DontDestroyOnLoad(gameObject);

        musicAudioSource = gameObject.AddComponent<AudioSource>();
        musicAudioSource.clip = musicAudioClip;
        musicAudioSource.outputAudioMixerGroup = musicOutput;
        musicAudioSource.loop = true;
        musicAudioSource.volume = musicVolume;

        if (musicPlayOnAwake)
        {
            musicAudioSource.time = 0f;
            musicAudioSource.Play();
        }

        ambientAudioSource = gameObject.AddComponent<AudioSource>();
        ambientAudioSource.clip = ambientAudioClip;
        ambientAudioSource.outputAudioMixerGroup = ambientOutput;
        ambientAudioSource.loop = true;
        ambientAudioSource.volume = ambientVolume;

        if (ambientPlayOnAwake)
        {
            ambientAudioSource.time = 0f;
            ambientAudioSource.Play();
        }
    }

    private void Start()
    {
        if (oldInstanceToDestroy != null)
        {
            if (transferAmbientTime)
                ambientAudioSource.timeSamples = oldInstanceToDestroy.ambientAudioSource.timeSamples;
            if (transferMusicTime)
                musicAudioSource.timeSamples = oldInstanceToDestroy.musicAudioSource.timeSamples;
            oldInstanceToDestroy.Stop();
            Destroy(oldInstanceToDestroy.gameObject);
        }
    }

    private void Update()
    {
        if (musicStack.Count > 0)
        {
            if (!musicAudioSource.isPlaying)
            {
                musicStack.Pop();
                if (musicStack.Count > 0)
                {
                    musicAudioSource.clip = musicStack.Peek();
                    musicAudioSource.Play();
                }
                else
                {
                    musicAudioSource.clip = musicAudioClip;
                    musicAudioSource.loop = true;
                    musicAudioSource.Play();
                }
            }
        }
    }

    public void PushClip(AudioClip clip)
    {
        musicStack.Push(clip);
        musicAudioSource.Stop();
        musicAudioSource.loop = false;
        musicAudioSource.time = 0;
        musicAudioSource.clip = clip;
        musicAudioSource.Play();
    }

    public void ChangeMusic(AudioClip clip)
    {
        musicAudioClip = clip;
        musicAudioSource.clip = clip;
    }

    public void ChangeAmbient(AudioClip clip)
    {
        ambientAudioClip = clip;
        ambientAudioSource.clip = clip;
    }

    public void Play()
    {
        PlayJustAmbient();
        PlayJustMusic();
    }

    public void PlayJustMusic()
    {
        musicAudioSource.time = 0f;
        musicAudioSource.Play();
    }

    public void PlayJustAmbient()
    {
        ambientAudioSource.Play();
    }

    public void Stop()
    {
        StopJustAmbient();
        StopJustMusic();
    }

    public void StopJustMusic()
    {
        musicAudioSource.Stop();
    }

    public void StopJustAmbient()
    {
        ambientAudioSource.Stop();
    }

    public void Mute()
    {
        MuteJustAmbient();
        MuteJustMusic();
    }

    public void MuteJustMusic()
    {
        musicAudioSource.volume = 0f;
    }

    public void MuteJustAmbient()
    {
        ambientAudioSource.volume = 0f;
    }

    public void Unmute()
    {
        UnmuteJustAmbient();
        UnmuteJustMusic();
    }

    public void UnmuteJustMusic()
    {
        musicAudioSource.volume = musicVolume;
    }

    public void UnmuteJustAmbient()
    {
        ambientAudioSource.volume = ambientVolume;
    }

    public void Mute(float fadeTime)
    {
        MuteJustAmbient(fadeTime);
        MuteJustMusic(fadeTime);
    }

    public void MuteJustMusic(float fadeTime)
    {
        StartCoroutine(VolumeFade(musicAudioSource, 0f, fadeTime));
    }

    public void MuteJustAmbient(float fadeTime)
    {
        StartCoroutine(VolumeFade(ambientAudioSource, 0f, fadeTime));
    }

    public void Unmute(float fadeTime)
    {
        UnmuteJustAmbient(fadeTime);
        UnmuteJustMusic(fadeTime);
    }

    public void UnmuteJustMusic (float fadeTime)
    {
        StartCoroutine(VolumeFade(musicAudioSource, musicVolume, fadeTime));
    }

    public void UnmuteJustAmbient(float fadeTime)
    {
        StartCoroutine(VolumeFade(ambientAudioSource, ambientVolume, fadeTime));
    }

    protected IEnumerator VolumeFade(AudioSource source, float finalVolume, float fadeTime)
    {
        float volumeDifference = Mathf.Abs(source.volume - finalVolume);
        float inverseFadeTime = 1f / fadeTime;

        while (!Mathf.Approximately(source.volume, finalVolume))
        {
            float delta = Time.deltaTime * volumeDifference * inverseFadeTime;
            source.volume = Mathf.MoveTowards(source.volume, finalVolume, delta);
            yield return null;
        }
        source.volume = finalVolume;
    }
}
