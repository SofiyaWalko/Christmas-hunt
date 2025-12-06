using System.Collections.Generic; // Для списка в Dropdown
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;

public class SettingsMenuController : MonoBehaviour
{
    public AudioMixer audioMixer; // Ссылку нужно будет указать в инспекторе

    private Slider _volumeSlider;
    private DropdownField _qualityDropdown;
    private Button _backButton;

    void OnEnable()
    {
        var root = GetComponent<UIDocument>().rootVisualElement;

        _volumeSlider = root.Q<Slider>("volume-slider");
        _qualityDropdown = root.Q<DropdownField>("quality-dropdown");
        _backButton = root.Q<Button>("back-button");

        // Настраиваем Dropdown
        _qualityDropdown.choices = new List<string> { "Низкое", "Среднее", "Высокое" };

        // Подписываемся на события
        _volumeSlider.RegisterValueChangedCallback(OnVolumeChanged);
        _qualityDropdown.RegisterValueChangedCallback(OnQualityChanged);
        _backButton.clicked += OnBackToMainMenu;

        LoadSettings();
    }

    private void LoadSettings()
    {
        // Загружаем и применяем громкость
        float volume = PlayerPrefs.GetFloat("volume", 0.75f);
        _volumeSlider.value = volume;
        SetVolume(volume);
        // Загружаем и применяем качество
        int qualityIndex = PlayerPrefs.GetInt("quality", 1); // 1 - среднее
        _qualityDropdown.index = qualityIndex;
        SetQuality(qualityIndex);
    }

    private void OnVolumeChanged(ChangeEvent<float> evt) => SetVolume(evt.newValue);

    private void OnQualityChanged(ChangeEvent<string> evt) => SetQuality(_qualityDropdown.index);

    private void SetVolume(float volume)
    {
        PlayerPrefs.SetFloat("volume", volume);
        // Для AudioMixer нужно значение в децибелах
        audioMixer.SetFloat("MasterVolume", Mathf.Log10(Mathf.Max(volume, 0.0001f)) * 20);
        PlayerPrefs.Save();
    }

    private void SetQuality(int index)
    {
        PlayerPrefs.SetInt("quality", index);
        QualitySettings.SetQualityLevel(index);
        PlayerPrefs.Save();
    }

    private void OnBackToMainMenu()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene("MainMenu");
    }
}
