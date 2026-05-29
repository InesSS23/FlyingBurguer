using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class SettingsPanelController : MonoBehaviour
{
    [Header("Volume Sliders")]
    [SerializeField] private Slider bgmVolumeSlider;
    [SerializeField] private Slider sfxVolumeSlider;

    [Header("Volume Text (Optional)")]
    [SerializeField] private TextMeshProUGUI bgmVolumeText;
    [SerializeField] private TextMeshProUGUI sfxVolumeText;

    [Header("Navigation")]
    [SerializeField] private Button backButton;

    private void OnEnable()
    {
        InitializeSliders();
        AddListeners();
    }

    private void OnDisable()
    {
        RemoveListeners();
    }

    private void InitializeSliders()
    {
        if (bgmVolumeSlider != null)
        {
            bgmVolumeSlider.value = AudioManager.Instance.GetBGMVolume();
        }

        if (sfxVolumeSlider != null)
        {
            sfxVolumeSlider.value = AudioManager.Instance.GetSFXVolume();
        }

        UpdateVolumeText();
    }

    private void AddListeners()
    {
        if (bgmVolumeSlider != null)
        {
            bgmVolumeSlider.onValueChanged.AddListener(OnBGMVolumeChanged);
        }

        if (sfxVolumeSlider != null)
        {
            sfxVolumeSlider.onValueChanged.AddListener(OnSFXVolumeChanged);
        }

        if (backButton != null)
        {
            backButton.onClick.AddListener(OnBackButtonClicked);
        }
    }

    private void RemoveListeners()
    {
        if (bgmVolumeSlider != null)
        {
            bgmVolumeSlider.onValueChanged.RemoveListener(OnBGMVolumeChanged);
        }

        if (sfxVolumeSlider != null)
        {
            sfxVolumeSlider.onValueChanged.RemoveListener(OnSFXVolumeChanged);
        }

        if (backButton != null)
        {
            backButton.onClick.RemoveListener(OnBackButtonClicked);
        }
    }

    private void OnBGMVolumeChanged(float value)
    {
        AudioManager.Instance.SetBGMVolume(value);
        UpdateVolumeText();
        AudioManager.Instance.PlayButtonClickSFX();
    }

    private void OnSFXVolumeChanged(float value)
    {
        AudioManager.Instance.SetSFXVolume(value);
        UpdateVolumeText();
        AudioManager.Instance.PlayButtonClickSFX();
    }

    private void OnBackButtonClicked()
    {
        AudioManager.Instance.PlayButtonClickSFX();
        // O MainMenuController vai lidar com mostrar o menu anterior
    }

    private void UpdateVolumeText()
    {
        if (bgmVolumeText != null)
        {
            bgmVolumeText.text = $"Música: {Mathf.RoundToInt(AudioManager.Instance.GetBGMVolume() * 100)}%";
        }

        if (sfxVolumeText != null)
        {
            sfxVolumeText.text = $"Efeitos: {Mathf.RoundToInt(AudioManager.Instance.GetSFXVolume() * 100)}%";
        }
    }
}
