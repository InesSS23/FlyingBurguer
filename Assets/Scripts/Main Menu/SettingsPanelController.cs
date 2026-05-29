using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SettingsPanelController : MonoBehaviour
{
    [Header("sliders")]
    [SerializeField] private Slider volumeSlider;
    [SerializeField] private Slider mouseSensitivitySlider;

    [Header("textos opcionais")]
    [SerializeField] private TMP_Text volumeText;
    [SerializeField] private TMP_Text mouseSensitivityText;

    private void OnEnable()
    {
        GameSettings.LoadSettings();

        SetupSliders();
        AddListeners();
        UpdateTexts();
    }

    private void OnDisable()
    {
        RemoveListeners();
    }

    private void SetupSliders()
    {
        if (volumeSlider != null)
        {
            volumeSlider.minValue = 0f;
            volumeSlider.maxValue = 1f;
            volumeSlider.value = GameSettings.GetMasterVolume();
        }

        if (mouseSensitivitySlider != null)
        {
            mouseSensitivitySlider.minValue = 0.02f;
            mouseSensitivitySlider.maxValue = 0.5f;
            mouseSensitivitySlider.value = GameSettings.GetMouseSensitivity();
        }
    }

    private void AddListeners()
    {
        if (volumeSlider != null)
        {
            volumeSlider.onValueChanged.AddListener(OnVolumeChanged);
        }

        if (mouseSensitivitySlider != null)
        {
            mouseSensitivitySlider.onValueChanged.AddListener(OnMouseSensitivityChanged);
        }
    }

    private void RemoveListeners()
    {
        if (volumeSlider != null)
        {
            volumeSlider.onValueChanged.RemoveListener(OnVolumeChanged);
        }

        if (mouseSensitivitySlider != null)
        {
            mouseSensitivitySlider.onValueChanged.RemoveListener(OnMouseSensitivityChanged);
        }
    }

    private void OnVolumeChanged(float value)
    {
        GameSettings.SetMasterVolume(value);
        UpdateTexts();
    }

    private void OnMouseSensitivityChanged(float value)
    {
        GameSettings.SetMouseSensitivity(value);
        UpdateTexts();
    }

    private void UpdateTexts()
    {
        if (volumeText != null)
        {
            int volumePercent = Mathf.RoundToInt(GameSettings.GetMasterVolume() * 100f);
            volumeText.text = "Som: " + volumePercent + "%";
        }

        if (mouseSensitivityText != null)
        {
            mouseSensitivityText.text = "Sensibilidade: " + GameSettings.GetMouseSensitivity().ToString("0.00");
        }
    }
}