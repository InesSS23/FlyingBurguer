using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance { get; private set; }

    [Header("Audio Sources")]
    [SerializeField] private AudioSource bgmAudioSource;
    [SerializeField] private AudioSource sfxAudioSource;

    [Header("Audio Clips")]
    [SerializeField] private AudioClip backgroundMusic;
    [SerializeField] private AudioClip buttonClickSFX;

    [Header("Volume Settings")]
    [SerializeField] private float defaultBGMVolume = 0.5f;
    [SerializeField] private float defaultSFXVolume = 0.7f;

    private float bgmVolume;
    private float sfxVolume;

    private const string BGM_VOLUME_KEY = "BGMVolume";
    private const string SFX_VOLUME_KEY = "SFXVolume";

    private void Awake()
    {
        // Implementa Singleton pattern
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    private void Start()
    {
        InitializeAudio();
    }

    private void InitializeAudio()
    {
        // Carrega volumes salvos ou usa valores padrão
        bgmVolume = PlayerPrefs.GetFloat(BGM_VOLUME_KEY, defaultBGMVolume);
        sfxVolume = PlayerPrefs.GetFloat(SFX_VOLUME_KEY, defaultSFXVolume);

        // Configura os audio sources
        if (bgmAudioSource == null)
        {
            bgmAudioSource = gameObject.AddComponent<AudioSource>();
            bgmAudioSource.loop = true;
            bgmAudioSource.playOnAwake = false;
        }

        if (sfxAudioSource == null)
        {
            sfxAudioSource = gameObject.AddComponent<AudioSource>();
            sfxAudioSource.loop = false;
            sfxAudioSource.playOnAwake = false;
        }

        bgmAudioSource.volume = bgmVolume;
        sfxAudioSource.volume = sfxVolume;

        // Toca a música de fundo se existir
        if (backgroundMusic != null && !bgmAudioSource.isPlaying)
        {
            PlayBackgroundMusic(backgroundMusic);
        }
    }

    /// <summary>
    /// Toca a música de fundo
    /// </summary>
    public void PlayBackgroundMusic(AudioClip clip)
    {
        if (bgmAudioSource != null && clip != null)
        {
            bgmAudioSource.clip = clip;
            bgmAudioSource.Play();
        }
    }

    /// <summary>
    /// Para a música de fundo
    /// </summary>
    public void StopBackgroundMusic()
    {
        if (bgmAudioSource != null)
        {
            bgmAudioSource.Stop();
        }
    }

    /// <summary>
    /// Toca um efeito sonoro (one-shot)
    /// </summary>
    public void PlaySFX(AudioClip clip)
    {
        if (sfxAudioSource != null && clip != null)
        {
            sfxAudioSource.PlayOneShot(clip, sfxVolume);
        }
    }

    /// <summary>
    /// Toca o som de clique do botão
    /// </summary>
    public void PlayButtonClickSFX()
    {
        if (buttonClickSFX != null)
        {
            PlaySFX(buttonClickSFX);
        }
    }

    /// <summary>
    /// Define o volume da música de fundo (0 a 1)
    /// </summary>
    public void SetBGMVolume(float volume)
    {
        bgmVolume = Mathf.Clamp01(volume);
        if (bgmAudioSource != null)
        {
            bgmAudioSource.volume = bgmVolume;
        }
        PlayerPrefs.SetFloat(BGM_VOLUME_KEY, bgmVolume);
    }

    /// <summary>
    /// Define o volume dos efeitos sonoros (0 a 1)
    /// </summary>
    public void SetSFXVolume(float volume)
    {
        sfxVolume = Mathf.Clamp01(volume);
        if (sfxAudioSource != null)
        {
            sfxAudioSource.volume = sfxVolume;
        }
        PlayerPrefs.SetFloat(SFX_VOLUME_KEY, sfxVolume);
    }

    /// <summary>
    /// Retorna o volume atual da música
    /// </summary>
    public float GetBGMVolume()
    {
        return bgmVolume;
    }

    /// <summary>
    /// Retorna o volume atual dos efeitos sonoros
    /// </summary>
    public float GetSFXVolume()
    {
        return sfxVolume;
    }
}
