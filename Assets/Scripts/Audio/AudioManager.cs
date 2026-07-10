using System.Collections.Generic;
using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance { get; private set; }

    [Header("Audio Sources")]
    [SerializeField] private AudioSource bgmAudioSource;
    [SerializeField] private AudioSource sfxAudioSource;

    [Header("Musica")]
    [SerializeField] private AudioClip backgroundMusic;
    [SerializeField] private AudioClip buttonClickSFX;

    [Header("Efeitos gerais")]
    [SerializeField] private AudioClip pickupObjectSFX;
    [SerializeField] private AudioClip placeObjectSFX;
    [SerializeField] private AudioClip addBurgerIngredientSFX;
    [SerializeField] private AudioClip readySFX;

    [Header("Efeitos de processo")]
    [SerializeField] private AudioClip grillMeatLoopSFX;
    [SerializeField] private AudioClip fryFriesLoopSFX;
    [SerializeField] private AudioClip fillDrinkLoopSFX;

    [Header("Volume Settings")]
    [SerializeField] private float defaultBGMVolume = 0.5f;
    [SerializeField] private float defaultSFXVolume = 0.7f;

    private float bgmVolume;
    private float sfxVolume;

    private bool initialized = false;

    private readonly List<AudioSource> activeLoopingSources = new List<AudioSource>();

    private const string BGM_VOLUME_KEY = "BGMVolume";
    private const string SFX_VOLUME_KEY = "SFXVolume";

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        InitializeAudio();
    }

    private void InitializeAudio()
    {
        if (initialized)
            return;

        initialized = true;

        GameSettings.LoadSettings();

        bgmVolume = PlayerPrefs.GetFloat(BGM_VOLUME_KEY, defaultBGMVolume);
        sfxVolume = PlayerPrefs.GetFloat(SFX_VOLUME_KEY, defaultSFXVolume);

        if (bgmAudioSource == null)
        {
            bgmAudioSource = gameObject.AddComponent<AudioSource>();
        }

        bgmAudioSource.loop = true;
        bgmAudioSource.playOnAwake = false;
        bgmAudioSource.spatialBlend = 0f;
        bgmAudioSource.volume = bgmVolume;

        if (sfxAudioSource == null)
        {
            sfxAudioSource = gameObject.AddComponent<AudioSource>();
        }

        sfxAudioSource.loop = false;
        sfxAudioSource.playOnAwake = false;
        sfxAudioSource.spatialBlend = 0f;
        sfxAudioSource.volume = sfxVolume;

        if (backgroundMusic != null && !bgmAudioSource.isPlaying)
        {
            PlayBackgroundMusic(backgroundMusic);
        }
    }

    public void PlayBackgroundMusic(AudioClip clip)
    {
        InitializeAudio();

        if (bgmAudioSource == null || clip == null)
            return;

        if (bgmAudioSource.clip == clip && bgmAudioSource.isPlaying)
            return;

        bgmAudioSource.clip = clip;
        bgmAudioSource.volume = bgmVolume;
        bgmAudioSource.Play();
    }

    public void StopBackgroundMusic()
    {
        if (bgmAudioSource != null)
        {
            bgmAudioSource.Stop();
        }
    }

    public void PlaySFX(AudioClip clip)
    {
        PlaySFX(clip, 1f);
    }

    public void PlaySFX(AudioClip clip, float volumeMultiplier)
    {
        InitializeAudio();

        if (sfxAudioSource == null || clip == null)
            return;

        float finalVolume = sfxVolume * Mathf.Clamp01(volumeMultiplier);
        sfxAudioSource.PlayOneShot(clip, finalVolume);
    }

    public AudioSource PlayLoopingSFX(AudioClip clip)
    {
        InitializeAudio();

        if (clip == null)
            return null;

        GameObject loopObject = new GameObject("LoopingSFX_" + clip.name);
        loopObject.transform.SetParent(transform);

        AudioSource source = loopObject.AddComponent<AudioSource>();
        source.clip = clip;
        source.loop = true;
        source.playOnAwake = false;
        source.spatialBlend = 0f;
        source.volume = sfxVolume;

        source.Play();

        activeLoopingSources.Add(source);

        return source;
    }

    public void StopLoopingSFX(AudioSource source)
    {
        if (source == null)
            return;

        if (activeLoopingSources.Contains(source))
        {
            activeLoopingSources.Remove(source);
        }

        source.Stop();
        Destroy(source.gameObject);
    }

    public void StopAllLoopingSFX()
    {
        for (int i = activeLoopingSources.Count - 1; i >= 0; i--)
        {
            if (activeLoopingSources[i] != null)
            {
                activeLoopingSources[i].Stop();
                Destroy(activeLoopingSources[i].gameObject);
            }
        }

        activeLoopingSources.Clear();
    }

    public void PlayButtonClickSFX()
    {
        PlaySFX(buttonClickSFX);
    }

    public void PlayPickupObjectSFX()
    {
        PlaySFX(pickupObjectSFX);
    }

    public void PlayPlaceObjectSFX()
    {
        PlaySFX(placeObjectSFX);
    }

    public void PlayAddBurgerIngredientSFX()
    {
        PlaySFX(addBurgerIngredientSFX);
    }

    public void PlayReadySFX()
    {
        PlaySFX(readySFX);
    }

    public AudioSource PlayGrillMeatLoopSFX()
    {
        return PlayLoopingSFX(grillMeatLoopSFX);
    }

    public AudioSource PlayFryFriesLoopSFX()
    {
        return PlayLoopingSFX(fryFriesLoopSFX);
    }

    public AudioSource PlayFillDrinkLoopSFX()
    {
        return PlayLoopingSFX(fillDrinkLoopSFX);
    }

    public void SetBGMVolume(float volume)
    {
        bgmVolume = Mathf.Clamp01(volume);

        if (bgmAudioSource != null)
        {
            bgmAudioSource.volume = bgmVolume;
        }

        PlayerPrefs.SetFloat(BGM_VOLUME_KEY, bgmVolume);
        PlayerPrefs.Save();
    }

    public void SetSFXVolume(float volume)
    {
        sfxVolume = Mathf.Clamp01(volume);

        if (sfxAudioSource != null)
        {
            sfxAudioSource.volume = sfxVolume;
        }

        for (int i = 0; i < activeLoopingSources.Count; i++)
        {
            if (activeLoopingSources[i] != null)
            {
                activeLoopingSources[i].volume = sfxVolume;
            }
        }

        PlayerPrefs.SetFloat(SFX_VOLUME_KEY, sfxVolume);
        PlayerPrefs.Save();
    }

    public float GetBGMVolume()
    {
        return bgmVolume;
    }

    public float GetSFXVolume()
    {
        return sfxVolume;
    }
}