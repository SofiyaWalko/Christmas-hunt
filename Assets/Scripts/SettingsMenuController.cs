using System.Collections.Generic; // Для списка в Dropdown
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;

public class SettingsMenuController : MonoBehaviour
{
    public AudioMixer audioMixer; // Ссылку нужно будет указать в инспекторе

    private Slider _musicVolumeSlider;
    private Slider _sfxVolumeSlider;
    private DropdownField _qualityDropdown;
    private Button _backButton;

    void OnEnable()
    {
        var root = GetComponent<UIDocument>().rootVisualElement;

        _musicVolumeSlider = root.Q<Slider>("music-volume-slider");
        _sfxVolumeSlider = root.Q<Slider>("sfx-volume-slider");
        _qualityDropdown = root.Q<DropdownField>("quality-dropdown");
        _backButton = root.Q<Button>("back-button");

        // Настраиваем Dropdown
        _qualityDropdown.choices = new List<string> { "Низкое", "Среднее", "Высокое" };

        // Подписываемся на события
        _musicVolumeSlider.RegisterValueChangedCallback(OnMusicVolumeChanged);
        _sfxVolumeSlider.RegisterValueChangedCallback(OnSFXVolumeChanged);
        _qualityDropdown.RegisterValueChangedCallback(OnQualityChanged);
        _backButton.clicked += OnBackToMainMenu;

        LoadSettings();
    }

    private void OnDisable()
    {
        if (_musicVolumeSlider != null)
            _musicVolumeSlider.UnregisterValueChangedCallback(OnMusicVolumeChanged);
        
        if (_sfxVolumeSlider != null)
            _sfxVolumeSlider.UnregisterValueChangedCallback(OnSFXVolumeChanged);
        
        if (_qualityDropdown != null)
            _qualityDropdown.UnregisterValueChangedCallback(OnQualityChanged);
        
        if (_backButton != null)
            _backButton.clicked -= OnBackToMainMenu;
    }

    private void LoadSettings()
    {
        // Загружаем и применяем музыку
        float musicVolume = PlayerPrefs.GetFloat("MusicVolume", 0.7f);
        _musicVolumeSlider.value = musicVolume * 100;
        SetMusicVolume(musicVolume);

        // Загружаем и применяем звуки игры
        float sfxVolume = PlayerPrefs.GetFloat("SFXVolume", 0.7f);
        _sfxVolumeSlider.value = sfxVolume * 100;
        SetSFXVolume(sfxVolume);

        // Загружаем и применяем качество
        int qualityIndex = PlayerPrefs.GetInt("quality", 1); // 1 - среднее
        _qualityDropdown.index = qualityIndex;
        SetQuality(qualityIndex);
    }

    private void OnMusicVolumeChanged(ChangeEvent<float> evt) => SetMusicVolume(evt.newValue / 100f);

    private void OnSFXVolumeChanged(ChangeEvent<float> evt) => SetSFXVolume(evt.newValue / 100f);

    private void OnQualityChanged(ChangeEvent<string> evt) => SetQuality(_qualityDropdown.index);

    private void SetMusicVolume(float volume)
    {
        volume = Mathf.Clamp01(volume);
        PlayerPrefs.SetFloat("MusicVolume", volume);
        
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.SetMusicVolume(volume);
        }
        
        PlayerPrefs.Save();
    }

    private void SetSFXVolume(float volume)
    {
        volume = Mathf.Clamp01(volume);
        PlayerPrefs.SetFloat("SFXVolume", volume);
        
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.SetSFXVolume(volume);
        }
        
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
