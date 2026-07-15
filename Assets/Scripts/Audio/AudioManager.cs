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
    [SerializeField] private AudioClip wrongOrderSFX;
    [SerializeField] private AudioClip trashSFX;

    [Header("Efeitos de processo")]
    [SerializeField] private AudioClip grillMeatLoopSFX;
    [SerializeField] private AudioClip fryFriesLoopSFX;
    [SerializeField] private AudioClip fillDrinkLoopSFX;

    [Header("Volume Settings")]
    [SerializeField] private float defaultBGMVolume = 0.5f;
    [SerializeField] private float defaultSFXVolume = 0.7f;

    /*
        AUMENTAR OU BAIXAR SONS ESPECIFICOS AQUI

        1f   = volume normal
        1.2f = um pouco mais alto
        1.5f = mais alto
        0.7f = mais baixo

        Evita passar muito de 2f para nao distorcer.
    */
    private const float BUTTON_CLICK_VOLUME = 1f;

    private const float PICKUP_OBJECT_VOLUME = 1.3f;
    private const float PLACE_OBJECT_VOLUME = 1.25f;
    private const float ADD_BURGER_INGREDIENT_VOLUME = 1.25f;
    private const float READY_VOLUME = 1.5f;
    private const float WRONG_ORDER_VOLUME = 2f;
    private const float TRASH_VOLUME = 1.4f;

    private const float GRILL_MEAT_VOLUME = 1.25f;
    private const float FRY_FRIES_VOLUME = 1.25f;
    private const float FILL_DRINK_VOLUME = 1.35f;

    private float bgmVolume;
    private float sfxVolume;

    private bool initialized = false;

    private const string BGM_VOLUME_KEY = "BGMVolume";
    private const string SFX_VOLUME_KEY = "SFXVolume";

    private class LoopingSFXInstance
    {
        public AudioSource source;
        public float volumeMultiplier;
    }

    private readonly List<LoopingSFXInstance> activeLoopingSources = new List<LoopingSFXInstance>();

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

    public void ResumeBackgroundMusic()
    {
        InitializeAudio();

        if (bgmAudioSource == null)
            return;

        if (bgmAudioSource.clip == null && backgroundMusic != null)
            bgmAudioSource.clip = backgroundMusic;

        if (bgmAudioSource.clip == null)
            return;

        bgmAudioSource.loop = true;
        bgmAudioSource.volume = bgmVolume;

        if (!bgmAudioSource.isPlaying)
            bgmAudioSource.Play();
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

        float safeMultiplier = Mathf.Clamp(volumeMultiplier, 0f, 3f);

        /*
            O volume geral continua controlado pela barra das opções.
            Aqui só estamos a dizer se este som é mais forte/fraco que os outros.
        */
        sfxAudioSource.volume = sfxVolume;
        sfxAudioSource.PlayOneShot(clip, safeMultiplier);
    }

    public AudioSource PlayLoopingSFX(AudioClip clip)
    {
        return PlayLoopingSFX(clip, 1f);
    }

    public AudioSource PlayLoopingSFX(AudioClip clip, float volumeMultiplier)
    {
        InitializeAudio();

        if (clip == null)
            return null;

        float safeMultiplier = Mathf.Clamp(volumeMultiplier, 0f, 3f);

        GameObject loopObject = new GameObject("LoopingSFX_" + clip.name);
        loopObject.transform.SetParent(transform);

        AudioSource source = loopObject.AddComponent<AudioSource>();
        source.clip = clip;
        source.loop = true;
        source.playOnAwake = false;
        source.spatialBlend = 0f;
        source.volume = Mathf.Clamp(sfxVolume * safeMultiplier, 0f, 3f);

        source.Play();

        LoopingSFXInstance instance = new LoopingSFXInstance();
        instance.source = source;
        instance.volumeMultiplier = safeMultiplier;

        activeLoopingSources.Add(instance);

        return source;
    }

    public void StopLoopingSFX(AudioSource source)
    {
        if (source == null)
            return;

        for (int i = activeLoopingSources.Count - 1; i >= 0; i--)
        {
            if (activeLoopingSources[i].source == source)
            {
                activeLoopingSources.RemoveAt(i);
                break;
            }
        }

        source.Stop();
        Destroy(source.gameObject);
    }

    public void StopAllLoopingSFX()
    {
        for (int i = activeLoopingSources.Count - 1; i >= 0; i--)
        {
            if (activeLoopingSources[i] != null && activeLoopingSources[i].source != null)
            {
                activeLoopingSources[i].source.Stop();
                Destroy(activeLoopingSources[i].source.gameObject);
            }
        }

        activeLoopingSources.Clear();
    }

    public void PlayButtonClickSFX()
    {
        PlaySFX(buttonClickSFX, BUTTON_CLICK_VOLUME);
    }

    public void PlayPickupObjectSFX()
    {
        PlaySFX(pickupObjectSFX, PICKUP_OBJECT_VOLUME);
    }

    public void PlayPlaceObjectSFX()
    {
        PlaySFX(placeObjectSFX, PLACE_OBJECT_VOLUME);
    }

    public void PlayAddBurgerIngredientSFX()
    {
        PlaySFX(addBurgerIngredientSFX, ADD_BURGER_INGREDIENT_VOLUME);
    }

    public void PlayReadySFX()
    {
        PlaySFX(readySFX, READY_VOLUME);
    }

    public void PlayWrongOrderSFX()
    {
        PlaySFX(wrongOrderSFX, WRONG_ORDER_VOLUME);
    }

    public void PlayTrashSFX()
    {
        PlaySFX(trashSFX, TRASH_VOLUME);
    }

    public AudioSource PlayGrillMeatLoopSFX()
    {
        return PlayLoopingSFX(grillMeatLoopSFX, GRILL_MEAT_VOLUME);
    }

    public AudioSource PlayFryFriesLoopSFX()
    {
        return PlayLoopingSFX(fryFriesLoopSFX, FRY_FRIES_VOLUME);
    }

    public AudioSource PlayFillDrinkLoopSFX()
    {
        return PlayLoopingSFX(fillDrinkLoopSFX, FILL_DRINK_VOLUME);
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

        for (int i = activeLoopingSources.Count - 1; i >= 0; i--)
        {
            if (activeLoopingSources[i] == null || activeLoopingSources[i].source == null)
            {
                activeLoopingSources.RemoveAt(i);
                continue;
            }

            activeLoopingSources[i].source.volume = Mathf.Clamp(
                sfxVolume * activeLoopingSources[i].volumeMultiplier,
                0f,
                3f
            );
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
