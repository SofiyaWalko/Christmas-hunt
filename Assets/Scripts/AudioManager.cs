using UnityEngine;

public class AudioManager : MonoBehaviour
{
    [Header("Audio Settings")]
    public AudioClip backgroundMusic;
    public float musicVolume = 0.7f;
    public float sfxVolume = 0.7f;

    private AudioSource _musicSource;
    private static AudioManager _instance;

    private void Awake()
    {
        // Реализуем паттерн Singleton
        if (_instance != null && _instance != this)
        {
            Destroy(gameObject);
            return;
        }

        _instance = this;
        DontDestroyOnLoad(gameObject);
    }

    private void Start()
    {
        // Инициализируем музыку
        _musicSource = GetComponent<AudioSource>();
        
        if (_musicSource == null)
        {
            _musicSource = gameObject.AddComponent<AudioSource>();
        }

        _musicSource.clip = backgroundMusic;
        _musicSource.volume = musicVolume;
        _musicSource.loop = true;
        _musicSource.playOnAwake = false;

        if (backgroundMusic != null)
        {
            _musicSource.Play();
        }

        // Загружаем сохранённые настройки
        LoadAudioSettings();
    }

    public void SetMusicVolume(float newVolume)
    {
        musicVolume = Mathf.Clamp01(newVolume);
        if (_musicSource != null)
        {
            _musicSource.volume = musicVolume;
        }
        SaveAudioSettings();
    }

    public void SetSFXVolume(float newVolume)
    {
        sfxVolume = Mathf.Clamp01(newVolume);
        SaveAudioSettings();
    }

    public float GetMusicVolume()
    {
        return musicVolume;
    }

    public float GetSFXVolume()
    {
        return sfxVolume;
    }

    public void StopMusic()
    {
        if (_musicSource != null)
        {
            _musicSource.Stop();
        }
    }

    public void ResumeMusic()
    {
        if (_musicSource != null && !_musicSource.isPlaying)
        {
            _musicSource.Play();
        }
    }

    private void SaveAudioSettings()
    {
        PlayerPrefs.SetFloat("MusicVolume", musicVolume);
        PlayerPrefs.SetFloat("SFXVolume", sfxVolume);
        PlayerPrefs.Save();
    }

    private void LoadAudioSettings()
    {
        musicVolume = PlayerPrefs.GetFloat("MusicVolume", 0.7f);
        sfxVolume = PlayerPrefs.GetFloat("SFXVolume", 0.7f);

        if (_musicSource != null)
        {
            _musicSource.volume = musicVolume;
        }
    }

    public static AudioManager Instance => _instance;
}
