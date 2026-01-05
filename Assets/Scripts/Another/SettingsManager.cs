using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SettingsManager : MonoBehaviour
{
    [Header("UI References")]
    public Slider volumeSlider;
    public Slider volumeSPX;
    public Slider brightnessSlider;
    public TextMeshProUGUI volumeValueText;
    public TextMeshProUGUI volumeSPXValueText;
    public TextMeshProUGUI brightnessValueText;
    //public Dropdown qualityDropdown;

    [Header("Panel")]
    public GameObject settingsPanel;

    // Default values
    private const float DEFAULT_VOLUME = 1f;
    private const float DEFAULT_VOLUME_SPX = 1f;
    private const float DEFAULT_BRIGHTNESS = 1f;
    //private const int DEFAULT_QUALITY = 2;

    // Lưu giá trị ban đầu để Cancel
    private float initialVolume;
    private float initialVolumeSPX;
    private float initialBrightness;
    //private int initialQuality;

    void Start()
    {
        // Gán sự kiện slider để Text cập nhật realtime
        volumeSlider.onValueChanged.AddListener(OnVolumeChanged);
        volumeSPX.onValueChanged.AddListener(OnVolumeSPXChanged);
        brightnessSlider.onValueChanged.AddListener(OnBrightnessChanged);
        //qualityDropdown.onValueChanged.AddListener(OnQualityChanged);

        // Load PlayerPrefs và lưu giá trị ban đầu
        LoadSettings();
        SaveInitialValues();
    }

    #region Slider Handlers
    public void OnVolumeChanged(float value)
    {
        AudioListener.volume = value;
        volumeValueText.text = (value * 100f).ToString("0") + "%";
    }

    public void OnVolumeSPXChanged(float value)
    {
        volumeSPXValueText.text = (value * 100f).ToString("0") + "%";
    }

    public void OnBrightnessChanged(float value)
    {
        RenderSettings.ambientLight = Color.white * value;
        brightnessValueText.text = (value * 100f).ToString("0") + "%";
    }

    //public void OnQualityChanged(int index)
    //{
    //    QualitySettings.SetQualityLevel(index);
    //}
    #endregion

    #region Buttons
    public void ApplySettings()
    {
        PlayerPrefs.SetFloat("Volume", volumeSlider.value);
        PlayerPrefs.SetFloat("VolumeSPX", volumeSPX.value);
        PlayerPrefs.SetFloat("Brightness", brightnessSlider.value);
        //PlayerPrefs.SetInt("Quality", qualityDropdown.value);
        PlayerPrefs.Save();

        // Cập nhật giá trị ban đầu để Cancel lần sau
        SaveInitialValues();

        CloseSettings();
    }

    public void CancelSettings()
    {
        // Phục hồi giá trị ban đầu
        volumeSlider.value = initialVolume;
        volumeSPX.value = initialVolumeSPX;
        brightnessSlider.value = initialBrightness;
        //qualityDropdown.value = initialQuality;

        // Cập nhật Text và hệ thống ngay
        OnVolumeChanged(volumeSlider.value);
        OnVolumeSPXChanged(volumeSPX.value);
        OnBrightnessChanged(brightnessSlider.value);
        //OnQualityChanged(qualityDropdown.value);

        CloseSettings();
    }

    public void ResetToDefault()
    {
        volumeSlider.value = DEFAULT_VOLUME;
        volumeSPX.value = DEFAULT_VOLUME_SPX;
        brightnessSlider.value = DEFAULT_BRIGHTNESS;
        //qualityDropdown.value = DEFAULT_QUALITY;

        OnVolumeChanged(DEFAULT_VOLUME);
        OnVolumeSPXChanged(DEFAULT_VOLUME_SPX);
        OnBrightnessChanged(DEFAULT_BRIGHTNESS);
        //OnQualityChanged(DEFAULT_QUALITY);
    }
    #endregion

    void LoadSettings()
    {
        volumeSlider.value = PlayerPrefs.GetFloat("Volume", DEFAULT_VOLUME);
        volumeSPX.value = PlayerPrefs.GetFloat("VolumeSPX", DEFAULT_VOLUME_SPX);
        brightnessSlider.value = PlayerPrefs.GetFloat("Brightness", DEFAULT_BRIGHTNESS);
        //qualityDropdown.value = PlayerPrefs.GetInt("Quality", DEFAULT_QUALITY);

        // Cập nhật Text và hệ thống
        OnVolumeChanged(volumeSlider.value);
        OnVolumeSPXChanged(volumeSPX.value);
        OnBrightnessChanged(brightnessSlider.value);
        //OnQualityChanged(qualityDropdown.value);
    }

    void SaveInitialValues()
    {
        initialVolume = volumeSlider.value;
        initialVolumeSPX = volumeSPX.value;
        initialBrightness = brightnessSlider.value;
        //initialQuality = qualityDropdown.value;
    }

    void CloseSettings()
    {
        if (settingsPanel != null)
            settingsPanel.SetActive(false);
    }
}
